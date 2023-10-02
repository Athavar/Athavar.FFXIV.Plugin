// <copyright file="DalamudServices.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Reflection;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

/// <summary>
///     Contains services from dalamud.
/// </summary>
internal sealed class DalamudServices : IDalamudServices
{
    private readonly Assembly? dalamudAssembly;
    private readonly Type? serviceGenericType;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DalamudServices"/> class.
    /// </summary>
    /// <param name="pluginInterface"><see cref="DalamudPluginInterface"/> used to inject the other values.</param>
    public DalamudServices(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Inject(this);

        try
        {
            this.dalamudAssembly = typeof(DalamudPluginInterface).Assembly;
            this.serviceGenericType = this.dalamudAssembly.GetType("Dalamud.Service`1") ?? throw new Exception("Fail to get type of Dalamud.Service<>");
        }
        catch (Exception e)
        {
            this.PluginLogger.Error($"{e.Message}\n{e.StackTrace ?? string.Empty}");
        }
    }

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public DalamudPluginInterface PluginInterface { get; init; } = null!;

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IBuddyList Buddies { get; private set; } = null!;
    */

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IAetheryteList AetheryteList { get; init; } = null!;
    */

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IChatGui ChatGui { get; init; } = null!;

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IChatManager ChatHandlers { get; init; } = null!;
    */

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IClientState ClientState { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public ICondition Condition { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public ICommandManager CommandManager { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IDataManager DataManager { get; init; } = null!;

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IDutyState DutyState { get; init; } = null!;
    */

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IDtrBar DtrBar { get; init; } = null!;
    */

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IFateTable FateTable { get; private set; } = null!;
    */

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IFlyTextGui FlyTexts { get; private set; } = null!;
    */

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IFramework Framework { get; init; } = null!;

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IGameConfig GameConfig { get; init; } = null!;
    */

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IGameGui GameGui { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IGameInteropProvider GameInteropProvider { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IGameLifecycle GameLifecycle { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IGameNetwork GameNetwork { get; init; } = null!;

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IGamepadState GamepadState { get; init; } = null!;
    */

    /*
    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IJobGauges Gauges { get; private set; } = null!;
    */

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IKeyState KeyState { get; init; } = null!;

    /*[PluginService]
    [RequiredVersion("1.0")]
    public ILibcFunction LibC { get; private set; } = null!;*/

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public IObjectTable ObjectTable { get; init; } = null!;

    /*[PluginService]
    [RequiredVersion("1.0")]
    public IPartyFinderGui PartyFinder { get; private set; } = null!;*/

    [PluginService]
    [RequiredVersion("1.0")]
    public IPartyList PartyList { get; init; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public IPluginLog PluginLogger { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public ISigScanner SigScanner { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public ITargetManager TargetManager { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public ITextureProvider TextureProvider { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    [RequiredVersion("1.0")]
    public ITitleScreenMenu TitleScreenMenu { get; init; } = null!;

    /*[PluginService]
    [RequiredVersion("1.0")]
    public IToastGui ToastGui { get; private set; } = null!;*/

    /// <inheritdoc/>
    public object? GetInternalService(Type serviceType) => this.serviceGenericType?.MakeGenericType(serviceType).GetMethod("Get")?.Invoke(null, BindingFlags.Default, null, Array.Empty<object>(), null);
}