// <copyright file="DetectionProgram.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.OpcodeWizard.PacketDetection;

using System.Diagnostics;
using System.Runtime.InteropServices;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Athavar.FFXIV.Plugin.OpcodeWizard.Models;
using Machina.FFXIV;
using Machina.Infrastructure;

internal sealed class DetectionProgram : IDisposable
{
    public bool Debug;
    private readonly IPluginLogger logger;
    private readonly ScannerRegistry scannerRegistry;
    private readonly IOpcodeManager opcodeManager;

    private readonly Queue<Packet> pq;

    private bool aborted;
    private bool stopped = true;
    private bool skipped;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DetectionProgram"/> class.
    /// </summary>
    /// <param name="logger"><see cref="IPluginLogger"/> added by DI.</param>
    /// <param name="opcodeManager"><see cref="IOpcodeManager"/> added by DI.</param>
    /// <param name="scannerRegistry"><see cref="ScannerRegistry"/> added by DI.</param>
    public DetectionProgram(IPluginLogger logger, IOpcodeManager opcodeManager, ScannerRegistry scannerRegistry)
    {
        this.logger = logger;
        this.opcodeManager = opcodeManager;
        this.scannerRegistry = scannerRegistry;

        this.pq = new Queue<Packet>();
    }

    public bool IsRunning => !this.stopped;

    public int ScannerIndex { get; private set; }

    public string CurrentTutorial { get; private set; } = string.Empty;

    public void Dispose() => this.Abort();

    /// <summary>
    ///     Runs the detection program.
    /// </summary>
    /// <param name="skipCount"></param>
    /// <param name="requestParameter">A function called when a parameter needs to be requested from the user.</param>
    /// <returns></returns>
    public async Task<bool> Run(int skipCount, Func<Scanner, int, Task<(string Parameter, bool SkipRequested)>> requestParameter)
    {
        this.aborted = false;
        this.Start();

        this.pq.Clear();

        this.ScannerIndex = skipCount;

        using var monitor = this.BuildNetworkMonitor();
        monitor.Start();

        var scanners = this.scannerRegistry.AsList();

        for (; this.ScannerIndex < scanners.Count; this.ScannerIndex++)
        {
            if (this.stopped)
            {
                monitor.Stop();
                this.Stop();
                return this.aborted;
            }

            var scanner = scanners[this.ScannerIndex];

            var parameters = new string[scanner.ParameterPrompts.Length];

            scanner.Opcode = 0;
            scanner.Running = true;
            this.CurrentTutorial = scanner.Tutorial;

            if (parameters.Length > 0)
            {
                var skip = await RequestParameters(scanner, parameters, requestParameter);

                if (skip)
                {
                    scanner.Opcode = this.opcodeManager.GetOpcode(scanner.OpcodeType);
                    scanner.Running = false;
                    continue;
                }
            }

            await this.RunScanner(scanner, parameters);

            scanner.Running = false;

            if (this.stopped)
            {
                scanner.Opcode = this.opcodeManager.GetOpcode(scanner.OpcodeType);
            }
        }

        monitor.Stop();
        this.Stop();
        return this.aborted;
    }

    public void SetIndex(int index)
    {
        if (!this.stopped)
        {
            return;
        }

        if (index < 0)
        {
            index = 0;
        }

        this.ScannerIndex = index;
    }

    public async Task<bool> RunOne(Func<Scanner, int, Task<(string Parameter, bool SkipRequested)>> requestParameter)
    {
        this.Start();

        this.pq.Clear();

        var scanner = this.scannerRegistry.GetScanner(this.ScannerIndex);
        if (scanner is null)
        {
            this.Abort();
            this.Stop();
            return this.aborted;
        }

        using var monitor = this.BuildNetworkMonitor();
        monitor.Start();

        var parameters = new string[scanner.ParameterPrompts.Length];

        scanner.Opcode = 0;
        scanner.Running = true;
        this.CurrentTutorial = scanner.Tutorial;

        if (parameters.Length > 0)
        {
            var skip = await RequestParameters(scanner, parameters, requestParameter);

            if (skip)
            {
                scanner.Opcode = this.opcodeManager.GetOpcode(scanner.OpcodeType);
                scanner.Running = false;
                monitor.Stop();
                this.Stop();
                return this.aborted;
            }
        }

        await this.RunScanner(scanner, parameters);

        scanner.Running = false;

        monitor.Stop();
        this.Stop();
        return this.aborted;
    }

    public void Start()
    {
        this.stopped = false;
        this.skipped = false;
    }

    public void Abort()
    {
        this.aborted = true;
        this.Skip();
        this.Stop();
    }

    public void Stop()
    {
        this.stopped = true;
        this.Skip();
    }

    public void Skip() => this.skipped = true;

    private static async Task<bool> RequestParameters(Scanner scanner, IList<string> parameters, Func<Scanner, int, Task<(string Parameter, bool SkipRequested)>> requestParameter)
    {
        var skip = false;
        for (var paramIndex = 0; paramIndex < parameters.Count; paramIndex++)
        {
            var (parameter, skipRequested) = await requestParameter(scanner, paramIndex);
            if (skipRequested)
            {
                skip = true;
                break;
            }

            parameters[paramIndex] = parameter ?? string.Empty;
        }

        return skip;
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern nint FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    private FFXIVNetworkMonitor BuildNetworkMonitor()
    {
        var window = FindWindow("FFXIVGAME", null);
        if (window == nint.Zero)
        {
            throw new Exception("FFXIVGAME Window not found. Please check if the game is running.");
        }

        GetWindowThreadProcessId(window, out var pid);
        var proc = Process.GetProcessById(Convert.ToInt32(pid));
        var gamePath = proc.MainModule?.FileName;

        var monitor = new FFXIVNetworkMonitor
        {
            MessageReceivedEventHandler = this.OnMessageReceived,
            MessageSentEventHandler = this.OnMessageSent,
            WindowName = "FINAL FANTASY XIV",
            UseDeucalion = true,
            ProcessID = pid,
        };

        if (!string.IsNullOrEmpty(gamePath))
        {
            monitor.OodlePath = gamePath;
        }

        return monitor;
    }

    private Task RunScanner(Scanner scanner, string[] parameters)
        => Task.Run(() =>
        {
            try
            {
                var opcode = PacketScanner.Scan(this.pq, scanner, parameters, ref this.skipped, ref this.stopped, this.Debug ? this.logger : null, this.opcodeManager.Opcodes);

                if (this.skipped || this.stopped)
                {
                    scanner.Opcode = this.opcodeManager.GetOpcode(scanner.OpcodeType);
                    this.skipped = false;
                    return;
                }

                scanner.Opcode = opcode;

                if (opcode != 0 && opcode != this.opcodeManager.GetOpcode(scanner.OpcodeType))
                {
                    this.opcodeManager.AddOrUpdate(scanner.OpcodeType, opcode);
                }
            }
            catch (FormatException)
            {
            }
        });

    private void OnMessageReceived(TCPConnection connection, long epoch, byte[] data) => this.OnMessage(connection.ToString(), epoch, data, PacketSource.Server);

    private void OnMessageSent(TCPConnection connection, long epoch, byte[] data) => this.OnMessage(connection.ToString(), epoch, data, PacketSource.Client);

    private void OnMessage(string? connection, long epoch, byte[] data, PacketSource source)
    {
        if (connection is null)
        {
            return;
        }

        lock (this.pq)
        {
            this.pq.Enqueue(new Packet(
                connection,
                epoch,
                data,
                source));
        }
    }
}