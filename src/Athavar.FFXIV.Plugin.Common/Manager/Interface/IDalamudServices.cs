// <copyright file="IDalamudServices.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

public interface IDalamudServices
{
    DalamudPluginInterface PluginInterface { get; }

    ICommandManager CommandManager { get; }

    IChatGui ChatGui { get; }

    IClientState ClientState { get; }

    ICondition Condition { get; }

    IDataManager DataManager { get; }

    IFramework Framework { get; }

    IGameGui GameGui { get; }

    IGameInteropProvider GameInteropProvider { get; }

    IGameLifecycle GameLifecycle { get; }

    IGameNetwork GameNetwork { get; }

    IKeyState KeyState { get; }

    IObjectTable ObjectTable { get; }

    IPartyList PartyList { get; }

    ISigScanner SigScanner { get; }

    ITargetManager TargetManager { get; }

    ITextureProvider TextureProvider { get; }

    ITitleScreenMenu TitleScreenMenu { get; }

    object? GetInternalService(Type serviceType);
}