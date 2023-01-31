// <copyright file="InstancinatorModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Instancinator;

using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Dalamud;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Command;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Implements the instancinator module.
/// </summary>
public class InstancinatorModule : IModule, IDisposable
{
    internal const string ModuleName = "Instancinator";
    private const string FolderName = "InstancinatorInternal";
    private const string MacroCommandName = "/instancinator";

    private static readonly string[] Instances = { "//", "//", "//" };

    private static readonly ushort[] Territories =
    {
        // 956, /* Labyrinthos */
        // 957, /* Thavnair */
        958, /* Garlemald */

        // 959, /* Mare Lamentorum */
        // 960, /* Ultima Thule */
        // 961, /* Elpis */
    };

    private readonly IDalamudServices dalamudServices;
    private readonly Configuration configuration;
    private readonly InstancinatorWindow window;
    private readonly string travelToInstancedArea;
    private readonly string aetheryteTarget;

    private long nextKeypress;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstancinatorModule" /> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="provider"><see cref="IServiceProvider" /> added by DI.</param>
    /// <param name="tab"><see cref="IInstancinatorTab" /> added by DI.</param>
    public InstancinatorModule(IDalamudServices dalamudServices, Configuration configuration, IServiceProvider provider, IInstancinatorTab tab)
    {
        this.dalamudServices = dalamudServices;
        this.configuration = configuration;
        this.window = provider.GetRequiredService<InstancinatorWindow>();
        this.Tab = tab;

        this.Configuration = configuration.Instancinator!;

        this.window.Setup(this);

        var aetheryteSheet = this.GetSubSheet<AetheryteString>("transport/Aetheryte") ?? throw new Exception("Sheet Aetheryte missing");
        var text = aetheryteSheet.GetRow(10)!.String.RawString;
        this.travelToInstancedArea = text[6..];
        this.aetheryteTarget = this.dalamudServices.DataManager.Excel.GetSheet<Aetheryte>()!.GetRow(0)!.Singular;

        this.dalamudServices.CommandManager.AddHandler(MacroCommandName, new CommandInfo(this.OnChatCommand)
        {
            HelpMessage = "Commands of the instancinator module.",
            ShowInHelp = false,
        });
        this.dalamudServices.Framework.Update += this.Tick;
        PluginLog.LogDebug("Module 'Instancinator' init");
    }

    /// <inheritdoc />
    public override string Name => ModuleName;

    /// <inheritdoc />
    public override bool Hidden => false;

    /// <inheritdoc />
    public override bool Enabled => this.Configuration.Enabled;

    public override IInstancinatorTab? Tab { get; }

    /// <summary>
    ///     Gets the configuration.
    /// </summary>
    internal InstancinatorConfiguration Configuration { get; }

    /// <summary>
    ///     Gets current selected Instance.
    /// </summary>
    internal int SelectedInstance { get; private set; }

    /// <inheritdoc />
    public override void Enable(bool state = true) => this.Configuration.Enabled = state;

    /// <summary>
    ///     Enable instance for switch for selected instance.
    /// </summary>
    /// <param name="instance">the instance number.</param>
    public void EnableInstance(int instance)
    {
        this.DisableAllAndCreateIfNotExists();
        this.SelectedInstance = instance;

        foreach (var e in this.configuration.Yes!.ListRootFolder.Children)
        {
            if (e is not TextFolderNode { Name: FolderName } folder)
            {
                continue;
            }

            foreach (var i in folder.Children)
            {
                if (i is ListEntryNode listEntry && (listEntry.Text == Instances[instance - 1] || listEntry.Text == this.travelToInstancedArea))
                {
                    listEntry.Enabled = true;
                }
            }

            return;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.dalamudServices.CommandManager.RemoveHandler(MacroCommandName);
        this.dalamudServices.Framework.Update -= this.Tick;
    }

    internal int GetNumberOfInstances()
    {
        switch (this.dalamudServices.ClientState.TerritoryType)
        {
            case 963:
                return 2;
            default:
                return 3;
        }
    }

    internal void DisableAllAndCreateIfNotExists()
    {
        if (!this.DisableAllEntries())
        {
            var rootChildren = this.configuration.Yes!.ListRootFolder.Children;
            var instance = new TextFolderNode
            {
                Name = FolderName,
            };
            var children = instance.Children;
            children.Add(this.CreateListEntryNode(this.aetheryteTarget, this.travelToInstancedArea));
            children.Add(this.CreateListEntryNode(this.aetheryteTarget, Instances[0]));
            children.Add(this.CreateListEntryNode(this.aetheryteTarget, Instances[1]));
            children.Add(this.CreateListEntryNode(this.aetheryteTarget, Instances[2]));
            rootChildren.Add(instance);
        }
    }

    private ExcelSheet<T>? GetSubSheet<T>(string path)
        where T : ExcelRow
        => this.dalamudServices.DataManager.Excel.GetType().GetMethod("GetSheet", BindingFlags.Instance | BindingFlags.NonPublic)!
           .MakeGenericMethod(typeof(T)).Invoke(this.dalamudServices.DataManager.Excel, new object?[]
            {
                path,
                this.dalamudServices.ClientState.ClientLanguage.ToLumina(),
                null,
            }) as ExcelSheet<T>;

    private void OnChatCommand(string command, string arguments)
    {
        arguments = arguments.Trim();
        if (int.TryParse(arguments, out var instanceNumber))
        {
            this.SelectedInstance = instanceNumber;
        }

        if (arguments == "disableall")
        {
            this.DisableAllAndCreateIfNotExists();
        }
        else if (arguments == "1")
        {
            this.EnableInstance(1);
        }
        else if (arguments == "2")
        {
            this.EnableInstance(2);
        }
        else if (arguments == "3")
        {
            this.EnableInstance(3);
        }
    }

    private unsafe bool IsInstanced() => UIState.Instance()->AreaInstance.IsInstancedArea();

    private void Tick(Framework framework)
    {
        if (this.SelectedInstance != 0)
        {
            if (this.dalamudServices.Condition[ConditionFlag.BetweenAreas]
             || this.dalamudServices.Condition[ConditionFlag.BetweenAreas51])
            {
                this.DisableAllEntries();
                return;
            }
        }

        if (!this.Configuration.Enabled || this.dalamudServices.ClientState.LocalPlayer == null || this.dalamudServices.Condition[ConditionFlag.BoundByDuty] || !this.IsInstanced())
        {
            this.window.IsOpen = false;
            return;
        }

        var aetheryte = this.dalamudServices.ObjectTable.FirstOrDefault(o => o.ObjectKind == ObjectKind.Aetheryte && Vector3.Distance(o.Position, this.dalamudServices.ClientState.LocalPlayer.Position) < 10f);

        if (aetheryte == null)
        {
            this.window.IsOpen = false;
            return;
        }

        this.window.IsOpen = true;

        if (this.SelectedInstance == 0 || Environment.TickCount64 <= this.nextKeypress)
        {
            return;
        }

        if (this.dalamudServices.Condition[ConditionFlag.OccupiedInQuestEvent])
        {
            this.nextKeypress = Environment.TickCount64 + 1000 + this.Configuration.ExtraDelay;
        }

        var target = this.dalamudServices.TargetManager.Target;

        if (target is null || target.Name.ToString() != this.aetheryteTarget)
        {
            this.dalamudServices.TargetManager.SetTarget(aetheryte);
            this.nextKeypress = Environment.TickCount64 + 100;
        }
        else
        {
            Task.Run(async () =>
            {
                List<Native.KeyCode> vkCodes = new();
                foreach (var nameValue in this.Configuration.KeyCode.Split(' ', ','))
                {
                    if (!Enum.TryParse<Native.KeyCode>(nameValue, true, out var vkCode))
                    {
                        throw new AthavarPluginException($"Invalid key code '{nameValue}'");
                    }

                    vkCodes.Add(vkCode);
                }

                var mWnd = Process.GetCurrentProcess().MainWindowHandle;

                foreach (var keyCode in vkCodes)
                {
                    Native.KeyDown(mWnd, keyCode);
                }

                await Task.Delay(15);

                foreach (var keyCode in vkCodes)
                {
                    Native.KeyUp(mWnd, keyCode);
                }
            });

            this.nextKeypress = Environment.TickCount64 + 500;
        }
    }

    private ListEntryNode CreateListEntryNode(string target, string text)
    {
        var instance = new ListEntryNode
        {
            Enabled = false,
            TargetRestricted = true,
            Text = text,
            TargetText = target,
        };
        return instance;
    }

    private bool DisableAllEntries()
    {
        this.SelectedInstance = 0;
        foreach (var e in this.configuration.Yes!.ListRootFolder.Children)
        {
            if (e is not TextFolderNode { Name: FolderName } folder)
            {
                continue;
            }

            foreach (var i in folder.Children)
            {
                if (i is ListEntryNode listEntry)
                {
                    listEntry.Enabled = false;
                }
            }

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Row of transport/Aetheryte Table.
    /// </summary>
    [Sheet("Aetheryte")]
    internal class AetheryteString : ExcelRow
    {
        /// <summary>
        ///     Gets or sets the key.
        /// </summary>
        public SeString Identifier { get; set; } = null!;

        /// <summary>
        ///     Gets or sets the string value.
        /// </summary>
        public SeString String { get; set; } = null!;

        /// <inheritdoc />
        public override void PopulateData(RowParser parser, GameData gameData, Language language)
        {
            base.PopulateData(parser, gameData, language);

            this.Identifier = parser.ReadColumn<SeString>(0)!;
            this.String = parser.ReadColumn<SeString>(1)!;
        }
    }
}