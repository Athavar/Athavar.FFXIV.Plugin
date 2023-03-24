// <copyright file="IDalamudServices.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

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
using Dalamud.Plugin;

public interface IDalamudServices
{
    DalamudPluginInterface PluginInterface { get; }

    CommandManager CommandManager { get; }

    ChatGui ChatGui { get; }

    ChatHandlers ChatHandlers { get; }

    ClientState ClientState { get; }

    Condition Condition { get; }

    DataManager DataManager { get; }

    Framework Framework { get; }

    GameGui GameGui { get; }

    GameNetwork GameNetwork { get; }

    KeyState KeyState { get; }

    ObjectTable ObjectTable { get; }

    PartyList PartyList { get; }

    SigScanner SigScanner { get; }

    TargetManager TargetManager { get; }

    TitleScreenMenu TitleScreenMenu { get; }
}