// <copyright file="DalamudServices.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Network;
using Dalamud.Interface;
using Dalamud.IoC;
using Dalamud.Plugin;

/// <summary>
///     Contains services from dalamud.
/// </summary>
internal sealed class DalamudServices : IDalamudServices
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DalamudServices"/> class.
    /// </summary>
    /// <param name="pluginInterface"><see cref="DalamudPluginInterface"/> used to inject the other values.</param>
    public DalamudServices(DalamudPluginInterface pluginInterface) => pluginInterface.Inject(this);

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public DalamudPluginInterface PluginInterface { get; init; } = null!;

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public BuddyList Buddies { get; private set; } = null!;
    */

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public AetheryteList AetheryteList { get; init; } = null!;
    */

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public ChatGui ChatGui { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public ChatHandlers ChatHandlers { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public ClientState ClientState { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public Condition Condition { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public CommandManager CommandManager { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public DataManager DataManager { get; init; } = null!;

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public DutyState DutyState { get; init; } = null!;
    */

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public DtrBar DtrBar { get; init; } = null!;
    */

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public FateTable FateTable { get; private set; } = null!;
    */

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public FlyTextGui FlyTexts { get; private set; } = null!;
    */

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public Framework Framework { get; init; } = null!;

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public GameConfig GameConfig { get; init; } = null!;
    */

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public GameGui GameGui { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public GameLifecycle GameLifecycle { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public GameNetwork GameNetwork { get; init; } = null!;

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public GamepadState GamepadState { get; init; } = null!;
    */

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public JobGauges Gauges { get; private set; } = null!;
    */

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public KeyState KeyState { get; init; } = null!;

    /*[PluginService]
    [RequiredVersion("1.0")]
    public LibcFunction LibC { get; private set; } = null!;*/

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public ObjectTable ObjectTable { get; init; } = null!;

    /*[PluginService]
    [RequiredVersion("1.0")]
    public PartyFinderGui PartyFinder { get; private set; } = null!;*/

    [PluginService]
    [RequiredVersion("1.0")]
    public PartyList PartyList { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public SigScanner SigScanner { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public TargetManager TargetManager { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public TitleScreenMenu TitleScreenMenu { get; init; } = null!;

    /*[PluginService]
    [RequiredVersion("1.0")]
    public ToastGui ToastGui { get; private set; } = null!;*/
}