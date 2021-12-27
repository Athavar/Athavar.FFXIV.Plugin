// <copyright file="IDalamudServices.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Manager.Interface;

using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Plugin;

internal interface IDalamudServices
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

    KeyState KeyState { get; }

    ObjectTable ObjectTable { get; }

    SigScanner SigScanner { get; }

    TargetManager TargetManager { get; }
}