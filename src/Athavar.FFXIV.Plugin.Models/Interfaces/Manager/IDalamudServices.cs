// <copyright file="IDalamudServices.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Interfaces;

using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Hooking;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

public interface IDalamudServices
{
    DalamudPluginInterface PluginInterface { get; }

    IAddonLifecycle AddonLifecycle { get; }

    ICommandManager CommandManager { get; }

    IChatGui ChatGui { get; }

    IClientState ClientState { get; }

    ICondition Condition { get; }

    IDataManager DataManager { get; }

    IDutyState DutyState { get; }

    IFramework Framework { get; }

    IGameGui GameGui { get; }

    IGameInteropProvider GameInteropProvider { get; }

    IGameLifecycle GameLifecycle { get; }

    IGameNetwork GameNetwork { get; }

    IKeyState KeyState { get; }

    IObjectTable ObjectTable { get; }

    IPartyList PartyList { get; }

    IPluginLogger PluginLogger { get; }

    ISigScanner SigScanner { get; }

    ITargetManager TargetManager { get; }

    ITextureProvider TextureProvider { get; }

    ITitleScreenMenu TitleScreenMenu { get; }

    object? GetInternalService(Type serviceType);

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
    void SafeEnableHookFromAddress<TDelegate>(string hookName, nint procAddress, TDelegate detour, Action<Hook<TDelegate>> setHook)
        where TDelegate : Delegate;
}