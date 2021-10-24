// <copyright file="MacroManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    using Athavar.FFXIV.Plugin;
    using Dalamud.Hooking;
    using Dalamud.Logging;

    /// <summary>
    /// Manager that handles running macros.
    /// </summary>
    internal partial class MacroManager : IDisposable
    {
        private readonly CancellationTokenSource eventLoopTokenSource = new();
        private readonly List<ActiveMacro> runningMacros = new();
        private readonly Hook<EventFrameworkDelegate> eventFrameworkHook;
        private CancellationTokenSource? stepTokenSource = null;
        private bool isPaused = true;
        private bool isLoggedIn = false;

        private MacroModule plugin;

        private Dictionary<string, BaseCommand> commands = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MacroManager"/> class.
        /// </summary>
        /// <param name="plugin">.</param>
        public MacroManager(MacroModule plugin)
        {
            this.plugin = plugin;

            DalamudBinding.ClientState.Login += this.OnLogin;
            DalamudBinding.ClientState.Logout += this.OnLogout;

            this.PopulateCommands();

            if (DalamudBinding.ClientState.LocalPlayer != null)
            {
                this.isLoggedIn = true;
            }

            this.eventFrameworkHook = new Hook<EventFrameworkDelegate>(plugin.Address.EventFrameworkFunctionAddress, this.EventFrameworkDetour);
            this.eventFrameworkHook.Enable();

            Task.Run(() => this.EventLoop(this.eventLoopTokenSource.Token));

            DalamudBinding.CommandManager.AddHandler("/runmacro", new Dalamud.Game.Command.CommandInfo(this.CommandHandler)
            {
                HelpMessage = "Start run a macro of the macro module.",
                ShowInHelp = true,
            });
        }

        private delegate IntPtr EventFrameworkDelegate(IntPtr a1, IntPtr a2, uint a3, ushort a4, IntPtr a5, IntPtr dataPtr, byte dataSize);

        /// <summary>
        /// The state of the macro manager.
        /// </summary>
        internal enum LoopState
        {
            /// <summary>
            /// Not logged in.
            /// </summary>
            NotLoggedIn,

            /// <summary>
            /// Waiting.
            /// </summary>
            Waiting,

            /// <summary>
            /// Running.
            /// </summary>
            Running,

            /// <summary>
            /// Cancel.
            /// </summary>
            Cancel,

            /// <summary>
            /// Paused.
            /// </summary>
            Paused,

            /// <summary>
            /// Stopped.
            /// </summary>
            Stopped,
        }

        /// <summary>
        /// Gets the state of the macro manager.
        /// </summary>
        public LoopState State { get; private set; } = LoopState.Waiting;

        /// <summary>
        /// Gets the <see cref="ChatManager"/>.
        /// </summary>
        public ChatManager ChatManager => this.plugin.ChatManager;

        /// <summary>
        /// Gets the <see cref="MacroConfiguration"/>.
        /// </summary>
        public MacroConfiguration Configuration => this.plugin.Configuration;

        /// <summary>
        /// Gets the current crafting data.
        /// </summary>
        public CraftingState CraftingData { get; private set; } = default;

        /// <inheritdoc/>
        public void Dispose()
        {
            DalamudBinding.CommandManager.RemoveHandler("/runmacro");

            DalamudBinding.ClientState.Login -= this.OnLogin;
            DalamudBinding.ClientState.Logout -= this.OnLogout;

            this.eventLoopTokenSource.Cancel();
            this.eventLoopTokenSource.Dispose();
            this.eventFrameworkHook.Dispose();

            // Dispose disposeable commands.
            foreach (var command in this.commands.Select(c => c.Value as IDisposable).Where(d => d is not null))
            {
                command?.Dispose();
            }
        }

        private void CommandHandler(string command, string arguments)
        {
            var args = arguments.Trim().Replace("\"", string.Empty);

            switch (this.State)
            {
                case LoopState.Waiting:
                    {
                        var node = this.plugin.Configuration.GetAllNodes().FirstOrDefault(node => node.Name == args);

                        if (node is MacroNode macro)
                        {
                            this.RunMacro(macro);
                        }
                        else
                        {
                            this.plugin.ChatManager.PrintError($"Fail to find macro with name: {args}");
                        }

                        break;
                    }

                default:
                    this.plugin.ChatManager.PrintError($"Fail to run macro: {this.State}");
                    break;
            }
        }

        private void OnEventFrameworkDetour(IntPtr dataPtr, byte dataSize)
        {
            if (dataSize >= 4)
            {
                var dataType = (ActionCategory)Marshal.ReadInt32(dataPtr);
                if (dataType == ActionCategory.Action || dataType == ActionCategory.CraftAction)
                {
                    this.CraftingData = Marshal.PtrToStructure<CraftingState>(dataPtr);
                }
            }
        }

        private IntPtr EventFrameworkDetour(IntPtr a1, IntPtr a2, uint a3, ushort a4, IntPtr a5, IntPtr dataPtr, byte dataSize)
        {
            try
            {
                this.OnEventFrameworkDetour(dataPtr, dataSize);
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Don't crash the game.");
            }

            return this.eventFrameworkHook.Original(a1, a2, a3, a4, a5, dataPtr, dataSize);
        }

        private void OnLogin(object? sender, EventArgs e)
        {
            this.isLoggedIn = true;
            this.State = LoopState.Waiting;
        }

        private void OnLogout(object? sender, EventArgs e)
        {
            this.isLoggedIn = false;
            this.State = LoopState.NotLoggedIn;
        }

        private async Task EventLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    while (!this.isLoggedIn)
                    {
                        this.State = LoopState.NotLoggedIn;
                        await Task.Delay(100, token);
                    }

                    while (this.isPaused)
                    {
                        this.State = this.runningMacros.Count == 0 ? LoopState.Waiting : LoopState.Paused;
                        await Task.Delay(100, token);
                    }

                    var macro = this.runningMacros.FirstOrDefault();
                    if (macro == default(ActiveMacro))
                    {
                        this.isPaused = true;
                        continue;
                    }

                    if (this.State != LoopState.Cancel)
                    {
                        this.State = LoopState.Running;
                    }

                    using (var macroToken = CancellationTokenSource.CreateLinkedTokenSource(this.eventLoopTokenSource.Token))
                    {
                        this.stepTokenSource = macroToken;
                        if (await this.ProcessMacro(macro, this.stepTokenSource.Token))
                        {
                            this.runningMacros.Remove(macro);
                        }

                        this.stepTokenSource = null;
                    }
                }
                catch (OperationCanceledException)
                {
                    PluginLog.Verbose("Event loop has stopped");
                    this.State = LoopState.Stopped;
                    break;
                }
                catch (ObjectDisposedException)
                {
                    PluginLog.Verbose($"Event loop has stopped");
                    this.State = LoopState.Stopped;
                    break;
                }
                catch (Exception ex)
                {
                    PluginLog.Error(ex, "Unhandled exception occurred");
                    this.plugin.ChatManager.PrintError($"[Athavar.Macro] Worker has died unexpectedly.");
                    this.runningMacros.Clear();
                }
            }
        }

        private async Task<bool> ProcessMacro(ActiveMacro macro, CancellationToken token)
        {
            var step = macro.GetCurrentStep();

            if (step == null)
            {
                return true;
            }

            var wait = BaseCommand.ExtractWait(ref step);

            try
            {
                var command = step.ToLower().Split(' ').First();

                if (command.StartsWith('/') && this.commands.TryGetValue(command.Substring(1), out var commandImp))
                {
                    wait = await commandImp.Execute(step, macro, wait, token).ConfigureAwait(false);
                }
                else
                {
                    this.plugin.ChatManager.SendMessage(step);
                }

                if (wait.TotalSeconds > 0)
                {
                    await Task.Delay(wait, token).ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex is EffectNotPresentError || ex is ConditionNotFulfilledError)
            {
                this.plugin.ChatManager.PrintError($"{ex.Message}: Failure while running {step} (step {macro.StepIndex + 1})");
                this.isPaused = true;
                return false;
            }
            catch (EventFrameworkTimeoutError ex)
            {
                this.plugin.ChatManager.PrintError($"{ex.Message}: Failure while running {step} (step {macro.StepIndex + 1})");
                this.isPaused = true;
                return false;
            }
            catch (InvalidMacroOperationException ex)
            {
                this.plugin.ChatManager.PrintError($"{ex.Message}: Failure while running {step} (step {macro.StepIndex + 1})");
                this.isPaused = true;
                return true;
            }
            catch (OperationCanceledException)
            {
                this.isPaused = true;
                return true;
            }

            macro.StepIndex++;

            return false;
        }

        private void PopulateCommands()
        {
            var baseType = typeof(BaseCommand);
            var commandTypes = baseType.Assembly.GetTypes().Where(t => t.IsSubclassOf(baseType));

            foreach (var commandType in commandTypes)
            {
                var instance = Activator.CreateInstance(commandType);

                if (instance is BaseCommand commandInstace)
                {
                    commandInstace.Init(this);

                    foreach (var commandName in commandInstace.CommandAliase)
                    {
                        this.commands[commandName] = commandInstace;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Public API.
    /// </summary>
    internal sealed partial class MacroManager
    {
        /// <summary>
        /// Gets the amount of macros currently executing.
        /// </summary>
        public int MacroCount => this.runningMacros.Count;

        /// <summary>
        /// Gets the name and currently executing line of each active macro.
        /// </summary>
        public (string Name, int StepIndex)[] MacroStatus
            => this.runningMacros.Select(macro => (macro.Node.Name, macro.StepIndex + 1)).ToArray();

        /// <summary>
        /// Run a macro.
        /// </summary>
        /// <param name="node">Macro to run.</param>
        /// <param name="index">index to insert.</param>
        public void RunMacro(MacroNode node, int? index = null)
        {
            if (index.HasValue)
            {
                this.runningMacros.Insert(index.Value, new ActiveMacro(node));
            }
            else
            {
                this.runningMacros.Add(new ActiveMacro(node));
            }

            this.isPaused = false;
        }

        /// <summary>
        /// Pause macro execution.
        /// </summary>
        public void Pause()
        {
            this.isPaused = true;
        }

        /// <summary>
        /// Resume macro execution.
        /// </summary>
        public void Resume()
        {
            this.isPaused = false;
        }

        /// <summary>
        /// Cancel macro execution.
        /// </summary>
        public void Cancel()
        {
            if (this.State == LoopState.Running)
            {
                this.State = LoopState.Cancel;
            }
        }

        /// <summary>
        /// Clear the executing macro list.
        /// </summary>
        public void Clear()
        {
            this.runningMacros.Clear();
            if ((!this.stepTokenSource?.IsCancellationRequested) ?? false)
            {
                this.stepTokenSource?.Cancel();
            }
        }

        /// <summary>
        /// Gets the contents of the current macro.
        /// </summary>
        /// <returns>Macro contents.</returns>
        public string[] CurrentMacroContent()
        {
            if (this.runningMacros.Count == 0)
            {
                return Array.Empty<string>();
            }

            return this.runningMacros.First().Steps.ToArray();
        }

        /// <summary>
        /// Gets the executing line number of the current macro.
        /// </summary>
        /// <returns>Macro line number.</returns>
        public int CurrentMacroStep()
        {
            if (this.runningMacros.Count == 0)
            {
                return 0;
            }

            return this.runningMacros.First().StepIndex;
        }
    }
}
