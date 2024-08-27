// <copyright file="DalamudServices.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Networking.Http;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

/// <summary>
///     Contains services from dalamud.
/// </summary>
internal sealed class DalamudServices : IDalamudServices, IDisposable
{
    private readonly Assembly? dalamudAssembly;
    private readonly Type? serviceGenericType;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DalamudServices"/> class.
    /// </summary>
    /// <param name="pluginInterface"><see cref="DalamudPluginInterface"/> used to inject the other values.</param>
    public DalamudServices(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Inject(this);
        this.PluginLogger = new Logger(this.PluginLog);
        using var happyEyeballsCallback = new HappyEyeballsCallback();
        this.HttpClient = new HttpClient(
            new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                ConnectCallback = happyEyeballsCallback.ConnectCallback,
            });
        this.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
        this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

        try
        {
            this.dalamudAssembly = typeof(IDalamudPluginInterface).Assembly;
            this.serviceGenericType = this.dalamudAssembly.GetType("Dalamud.Service`1") ?? throw new Exception("Fail to get type of Dalamud.Service<>");
        }
        catch (Exception e)
        {
            this.PluginLogger.Error($"{e.Message}\n{e.StackTrace ?? string.Empty}");
        }
    }

    /*[PluginService]
    public IToastGui ToastGui { get; private set; } = null!;*/

    /// <inheritdoc/>
    public IPluginLogger PluginLogger { get; }

    /// <inheritdoc/>
    public HttpClient HttpClient { get; }

    /// <inheritdoc/>
    [PluginService]
    public IDalamudPluginInterface PluginInterface { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    public IAddonEventManager AddonEventManager { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    public IAddonLifecycle AddonLifecycle { get; init; } = null!;

    /*
    /// <inheritdoc/>
    [PluginService]
    public IAetheryteList AetheryteList { get; init; } = null!;
    */

    /*
    /// <inheritdoc/>
    [PluginService]
    public IBuddyList Buddies { get; private set; } = null!;
    */

    /// <inheritdoc/>
    [PluginService]
    public IChatGui ChatGui { get; init; } = null!;

    /*
    /// <inheritdoc/>
    [PluginService]
    public IChatManager ChatHandlers { get; init; } = null!;
    */

    /// <inheritdoc/>
    [PluginService]
    public IClientState ClientState { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    public ICondition Condition { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    public ICommandManager CommandManager { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    public IDataManager DataManager { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    public IDutyState DutyState { get; init; } = null!;

    /*
    /// <inheritdoc/>
    [PluginService]
    public IDtrBar DtrBar { get; init; } = null!;
    */

    /*
    /// <inheritdoc/>
    [PluginService]
    public IFateTable FateTable { get; private set; } = null!;
    */

    /*
    /// <inheritdoc/>
    [PluginService]
    public IFlyTextGui FlyTexts { get; private set; } = null!;
    */

    /// <inheritdoc/>
    [PluginService]
    public IFramework Framework { get; init; } = null!;

    /*
    /// <inheritdoc/>
    [PluginService]
    public IGameConfig GameConfig { get; init; } = null!;
    */

    /// <inheritdoc/>
    [PluginService]
    public IGameGui GameGui { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    public IGameInteropProvider GameInteropProvider { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    public IGameLifecycle GameLifecycle { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    public IGameNetwork GameNetwork { get; init; } = null!;

    /*
    /// <inheritdoc/>
    [PluginService]
    public IGamepadState GamepadState { get; init; } = null!;
    */

    /*
    /// <inheritdoc/>
    [PluginService]
    public IJobGauges Gauges { get; private set; } = null!;
    */

    /// <inheritdoc/>
    [PluginService]
    public IKeyState KeyState { get; init; } = null!;

    /*[PluginService]
    public ILibcFunction LibC { get; private set; } = null!;*/

    /// <inheritdoc/>
    [PluginService]
    public INotificationManager NotificationManager { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    public IObjectTable ObjectTable { get; init; } = null!;

    /*[PluginService]
    public IPartyFinderGui PartyFinder { get; private set; } = null!;*/

    [PluginService]
    public IPartyList PartyList { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    public ISigScanner SigScanner { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    public ITargetManager TargetManager { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    public ITextureProvider TextureProvider { get; init; } = null!;

    /// <inheritdoc/>
    [PluginService]
    public ITitleScreenMenu TitleScreenMenu { get; init; } = null!;

    [PluginService]
    internal IPluginLog PluginLog { get; init; } = null!;

    /// <inheritdoc/>
    public object? GetInternalService(Type serviceType) => this.serviceGenericType?.MakeGenericType(serviceType).GetMethod("Get")?.Invoke(null, BindingFlags.Default, null, [], null);

    /// <summary>
    ///     Creates and enable a hook. Hooking address is inferred by calling to GetProcAddress() function.
    ///     The hook is not activated until Enable() method is called.
    ///     Please do not use MinHook unless you have thoroughly troubleshot why Reloaded does not work.
    /// </summary>
    /// <param name="hookName">Name of the hook. Used in Logging..</param>
    /// <param name="procAddress">A memory address to install a hook.</param>
    /// <param name="detour">Callback function. Delegate must have a same original function prototype.</param>
    /// <param name="setHook">Callback function to give the hook back.</param>
    /// <typeparam name="TDelegate">Delegate of detour.</typeparam>
    public void SafeEnableHookFromAddress<TDelegate>(string hookName, nint procAddress, TDelegate detour, Action<Hook<TDelegate>> setHook)
        where TDelegate : Delegate
    {
        Hook<TDelegate>? hook = null;
        Task.Run(
            () =>
            {
                this.PluginLog.Verbose("Create Hook {0}", hookName);
                hook = this.GameInteropProvider.HookFromAddress(procAddress, detour);
                this.PluginLog.Verbose("Enable Hook {0}", hookName);
                hook.Enable();
                setHook(hook);
                this.PluginLog.Verbose("Finish Enabling Hook {0}", hookName);
            });
        Task.Run(
            async () =>
            {
                await Task.Delay(7000);
                if (hook is null)
                {
                    this.PluginLog.Warning("Hook {0} was not created in time", hookName);
                }
            });
    }

    /// <inheritdoc/>
    public void Dispose() => this.HttpClient.Dispose();
}