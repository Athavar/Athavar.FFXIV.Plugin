// <copyright file="InstancinatorModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Instancinator;

using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Implements the instancinator module.
/// </summary>
[Module(ModuleName, ModuleConfigurationType = typeof(InstancinatorConfiguration), HasTab = true)]
internal sealed class InstancinatorModule : Module<InstancinatorTab, InstancinatorConfiguration>
{
    internal const string ModuleName = "Instancinator";
    private const string FolderName = "InstancinatorInternal";
    private const string MacroCommandName = "/instancinator";

    private static readonly string[] Instances = ["/\ue0b1/", "/\ue0b2/", "/\ue0b3/", "/\ue0b4/", "/\ue0b5/", "/\ue0b6/"];

    private readonly YesConfiguration yesConfiguration;
    private readonly IServiceProvider provider;
    private readonly IDalamudServices dalamudServices;
    private readonly ICommandInterface ci;
    private readonly IFrameworkManager frameworkManager;
    private readonly string travelToInstancedArea;
    private readonly string aetheryteTarget;

    private InstancinatorWindow? window;

    private long nextKeypress;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstancinatorModule"/> class.
    /// </summary>
    /// <param name="configuration"><see cref="InstancinatorConfiguration"/> added by DI.</param>
    /// <param name="yesConfiguration"><see cref="YesConfiguration"/> added by DI.</param>
    /// <param name="provider"><see cref="IServiceProvider"/> added by DI.</param>
    /// <param name="dalamudServices"><see cref="IDalamudServices"/> added by DI.</param>
    /// <param name="ci"><see cref="ICommandInterface"/> added by DI.</param>
    public InstancinatorModule(InstancinatorConfiguration configuration, YesConfiguration yesConfiguration, IServiceProvider provider, IDalamudServices dalamudServices, ICommandInterface ci, IFrameworkManager frameworkManager)
        : base(configuration)
    {
        this.yesConfiguration = yesConfiguration;
        this.provider = provider;
        this.dalamudServices = dalamudServices;
        this.ci = ci;
        this.frameworkManager = frameworkManager;

        var aetheryteSheet = this.dalamudServices.DataManager.Excel.GetSheet<AetheryteString>() ?? throw new Exception("Sheet transport/Aetheryte missing");
        var text = aetheryteSheet.GetRow(12).String.ExtractText();
        this.travelToInstancedArea = text.Trim();
        this.aetheryteTarget = this.dalamudServices.DataManager.Excel.GetSheet<Aetheryte>().GetRow(0).Singular.ExtractText();

        this.dalamudServices.CommandManager.AddHandler(
            MacroCommandName,
            new CommandInfo(this.OnChatCommand)
            {
                HelpMessage = "Commands of the instancinator module.",
                ShowInHelp = false,
            });
        frameworkManager.Subscribe(this.Tick);
        this.dalamudServices.PluginLogger.Debug("Module 'Instancinator' init");
    }

    /// <inheritdoc/>
    public override string Name => ModuleName;

    /// <summary>
    ///     Gets current selected Instance.
    /// </summary>
    internal int SelectedInstance { get; private set; }

    /// <summary>
    ///     Enable instance for switch for selected instance.
    /// </summary>
    /// <param name="instance">the instance number.</param>
    public void EnableInstance(int instance)
    {
        this.DisableAllAndCreateIfNotExists();
        this.SelectedInstance = instance;

        foreach (var e in this.yesConfiguration.ListRootFolder.Children)
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

    /// <inheritdoc/>
    public override void Dispose()
    {
        this.dalamudServices.CommandManager.RemoveHandler(MacroCommandName);
        this.frameworkManager.Unsubscribe(this.Tick);
        base.Dispose();
        this.window?.Dispose();
    }

    internal int GetNumberOfInstances()
    {
        switch (this.dalamudServices.ClientState.TerritoryType)
        {
            case 1185: // Tuliyollal
            case 1186: // Solution 9
                return 2;
            /*case 1187: // Urqopacha
            case 1188: // Kozama'uka
                return 6;*/
            default:
                return 3;
        }
    }

    internal void DisableAllAndCreateIfNotExists()
    {
        void AddOrDisable(List<Node> f, string text)
        {
            var node = f.OfType<ListEntryNode>().FirstOrDefault(n => n.Text == text && n.TargetText == this.aetheryteTarget);

            if (node is null)
            {
                f.Add(this.CreateListEntryNode(this.aetheryteTarget, text));
            }
            else
            {
                node.Enabled = false;
            }
        }

        var folder = this.yesConfiguration.ListRootFolder.Children.OfType<TextFolderNode>().FirstOrDefault(f => f.Name == FolderName);
        if (folder is null)
        {
            folder = new TextFolderNode
            {
                Name = FolderName,
            };
            this.yesConfiguration.ListRootFolder.Children.Add(folder);
        }

        this.SelectedInstance = 0;
        var children = folder.Children;
        AddOrDisable(children, this.travelToInstancedArea);
        foreach (var numberText in Instances)
        {
            AddOrDisable(children, numberText);
        }
    }

    /// <inheritdoc/>
    protected override InstancinatorTab InitTab() => new(this.ModuleConfig);

    private InstancinatorWindow CreateWindow() => new(this.provider.GetRequiredService<WindowSystem>(), this);

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
        else if (int.TryParse(arguments, out var number) && number > 0 && number < Instances.Length)
        {
            this.EnableInstance(number);
        }
    }

    private unsafe bool IsInstanced() => UIState.Instance()->PublicInstance.IsInstancedArea();

    private void Tick(IFramework framework)
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

        var config = this.ModuleConfig;
        if (!config.Enabled || this.dalamudServices.ClientState.LocalPlayer == null || this.dalamudServices.Condition[ConditionFlag.BoundByDuty] || !this.IsInstanced())
        {
            if (this.window != null)
            {
                this.window.IsOpen = false;
            }

            return;
        }

        if (!this.ci.IsTargetInReach(ObjectKind.Aetheryte, this.aetheryteTarget))
        {
            if (this.window != null)
            {
                this.window.IsOpen = false;
            }

            return;
        }

        (this.window ??= this.CreateWindow()).IsOpen = true;

        if (this.SelectedInstance == 0 || Environment.TickCount64 <= this.nextKeypress)
        {
            return;
        }

        if (this.dalamudServices.Condition[ConditionFlag.OccupiedInQuestEvent])
        {
            this.nextKeypress = Environment.TickCount64 + 1000 + config.ExtraDelay;
        }

        this.ci.InteractWithTarget(ObjectKind.Aetheryte, this.aetheryteTarget);

        this.nextKeypress = Environment.TickCount64 + 500;
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
        foreach (var e in this.yesConfiguration.ListRootFolder.Children)
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
    [Sheet("transport/Aetheryte")]
    public struct AetheryteString(ExcelPage page, uint offset, uint row)
        : IExcelRow<AetheryteString>
    {
        public uint RowId => row;

        public readonly ReadOnlySeString Identifier => page.ReadString(offset, offset);

        public readonly ReadOnlySeString String => page.ReadString(offset + 4, offset);

        static AetheryteString IExcelRow<AetheryteString>.Create(ExcelPage page, uint offset, uint row) => new(page, offset, row);
    }
}