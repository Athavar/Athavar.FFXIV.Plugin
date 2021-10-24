// <copyright file="DalamudBinding.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin
{
    using Dalamud.Data;
    using Dalamud.Game;
    using Dalamud.Game.ClientState;
    using Dalamud.Game.ClientState.Conditions;
    using Dalamud.Game.ClientState.Fates;
    using Dalamud.Game.ClientState.Keys;
    using Dalamud.Game.ClientState.Objects;
    using Dalamud.Game.ClientState.Party;
    using Dalamud.Game.Command;
    using Dalamud.Game.Gui;
    using Dalamud.Game.Gui.Toast;
    using Dalamud.Game.Network;
    using Dalamud.IoC;
    using Dalamud.Plugin;

    public class DalamudBinding
    {
        public static void Initialize(DalamudPluginInterface pluginInterface)
            => pluginInterface.Create<DalamudBinding>();

        // @formatter:off
        [PluginService] [RequiredVersion("1.0")] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;

        [PluginService] [RequiredVersion("1.0")] public static CommandManager CommandManager { get; private set; } = null!;

        [PluginService] [RequiredVersion("1.0")] public static SigScanner SigScanner { get; private set; } = null!;

        [PluginService] [RequiredVersion("1.0")] public static DataManager DataManager { get; private set; } = null!;

        [PluginService] [RequiredVersion("1.0")] public static ClientState ClientState { get; private set; } = null!;

        [PluginService] [RequiredVersion("1.0")] public static ChatGui ChatGui { get; private set; } = null!;

        // [PluginService][RequiredVersion("1.0")] public static SeStringManager        SeStrings       { get; private set; } = null!; obsolete, use static method on SeString class.
        // [PluginService][RequiredVersion("1.0")] public static ChatHandlers           ChatHandlers    { get; private set; } = null!;
        [PluginService] [RequiredVersion("1.0")] public static Framework Framework { get; private set; } = null!;

        [PluginService] [RequiredVersion("1.0")] public static GameNetwork GameNetwork { get; private set; } = null!;

        [PluginService] [RequiredVersion("1.0")] public static Condition Condition { get; private set; } = null!;

        [PluginService] [RequiredVersion("1.0")] public static KeyState KeyState { get; private set; } = null!;

        [PluginService] [RequiredVersion("1.0")] public static GameGui GameGui { get; private set; } = null!;

        // [PluginService][RequiredVersion("1.0")] public static FlyTextGui             FlyTexts        { get; private set; } = null!;
        [PluginService] [RequiredVersion("1.0")] public static ToastGui ToastGui { get; private set; } = null!;

        // [PluginService][RequiredVersion("1.0")] public static JobGauges              Gauges          { get; private set; } = null!;
        // [PluginService][RequiredVersion("1.0")] public static PartyFinderGui         PartyFinder     { get; private set; } = null!;
        // [PluginService][RequiredVersion("1.0")] public static BuddyList              Buddies         { get; private set; } = null!;
        [PluginService] [RequiredVersion("1.0")] public static PartyList PartyList { get; private set; } = null!;

        [PluginService] [RequiredVersion("1.0")] public static TargetManager TargetManager { get; private set; } = null!;

        [PluginService] [RequiredVersion("1.0")] public static ObjectTable ObjectTable { get; private set; } = null!;

        [PluginService] [RequiredVersion("1.0")] public static FateTable FateTable { get; private set; } = null!;
        // [PluginService][RequiredVersion("1.0")] public static LibcFunction           LibC            { get; private set; } = null!;
        // @formatter:on
    }
}
