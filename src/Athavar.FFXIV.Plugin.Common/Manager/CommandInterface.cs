// <copyright file="CommandInterface.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Numerics;
using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Config;
using Dalamud;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;

/// <summary>
///     Miscellaneous functions that commands/scripts can use.
/// </summary>
internal sealed partial class CommandInterface : ICommandInterface
{
    private readonly IDalamudServices dalamudServices;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandInterface"/> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="dalamudServices"/> added by DI.</param>
    public CommandInterface(IDalamudServices dalamudServices)
    {
        this.dalamudServices = dalamudServices;
        var mainSheet = dalamudServices.DataManager.GetExcelSheet<MainCommand>(ClientLanguage.English)!;
        this.logOutId = mainSheet.FirstOrDefault(c => c.Name?.RawString == "Log Out")?.RowId ?? 0u;
    }

    /// <inheritdoc/>
    public unsafe bool CanUseAction(uint actionId)
    {
        if (!this.IsLoggedIn())
        {
            return false;
        }

        var actionType = actionId >= 100000U ? ActionType.CraftAction : ActionType.Spell;
        return ActionManager.Instance()->GetActionStatus(actionType, actionId, Constants.PlayerId, true, true, null) == 0U;
    }

    /// <inheritdoc/>
    public unsafe bool UseAction(uint actionId)
    {
        if (!this.IsLoggedIn())
        {
            return false;
        }

        var actionType = actionId >= 100000U ? ActionType.CraftAction : ActionType.Spell;
        return this.CanUseAction(actionId) && ActionManager.Instance()->UseAction(actionType, actionId, Constants.PlayerId, 0U, 0U, 0U, null);
    }

    /// <inheritdoc/>
    public unsafe bool CanUseGeneralAction(uint actionId)
    {
        if (!this.IsLoggedIn())
        {
            return false;
        }

        return ActionManager.Instance()->GetActionStatus(ActionType.General, actionId, Constants.PlayerId, true, true, null) == 0U;
    }

    /// <inheritdoc/>
    public unsafe bool UseGeneralAction(uint actionId)
    {
        if (!this.IsLoggedIn())
        {
            return false;
        }

        return this.CanUseGeneralAction(actionId) && ActionManager.Instance()->UseAction(ActionType.General, actionId, Constants.PlayerId, 0U, 0U, 0U, null);
    }

    public bool IsTargetInReach(string targetName) => this.IsTargetInReach(targetName, null);

    public bool IsTargetInReach(ObjectKind objectKind, string targetName) => this.IsTargetInReach(targetName, objectKind);

    public bool InteractWithTarget(string targetName) => this.InteractWithTarget(targetName, null);

    public bool InteractWithTarget(ObjectKind objectKind, string targetName) => this.InteractWithTarget(targetName, objectKind);

    private bool IsTargetInReach(string targetName, ObjectKind? objectKind)
    {
        var gameObject = this.FindNearestGameObject(targetName, objectKind);
        if (gameObject is null)
        {
            return false;
        }

        return this.IsObjectInReach(gameObject);
    }

    private unsafe bool InteractWithTarget(string targetName, ObjectKind? objectKind)
    {
        var target = this.FindNearestGameObject(targetName, objectKind);

        if (target is null)
        {
            return false;
        }

        if (this.dalamudServices.TargetManager.Target != target)
        {
            this.dalamudServices.TargetManager.Target = target;
        }

        var targetAddress = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)target.Address;
        var ts = TargetSystem.Instance();

        if (!this.IsObjectInReach(target))
        {
            return false;
        }

        var res = ts->InteractWithObject(targetAddress);
        PluginLog.LogInformation("Interaction Result: {0}", res);
        return true;
    }

    private bool IsObjectInReach(GameObject gameObject, Vector3? playerPosition = null)
    {
        playerPosition ??= this.dalamudServices.ClientState.LocalPlayer?.Position;
        if (playerPosition is null)
        {
            return false;
        }

        return Vector3.Distance(playerPosition.Value, gameObject.Position) < 10f;
    }

    private GameObject? FindNearestGameObject(string targetName, ObjectKind? objectKind)
    {
        if (!this.IsLoggedIn())
        {
            return null;
        }

        var playerPosition = this.dalamudServices.ClientState.LocalPlayer!.Position;

        IEnumerable<GameObject> objects = this.dalamudServices.ObjectTable;
        if (objectKind is not null)
        {
            objects = objects.Where(obj => obj.ObjectKind == objectKind);
        }

        return objects
           .Where(obj => targetName.Equals(obj.Name.TextValue, StringComparison.OrdinalIgnoreCase))
           .MinBy(o => Vector3.Distance(playerPosition, o.Position));
    }

    private unsafe int GetNodeTextAsInt(AtkTextNode* node, string error)
    {
        try
        {
            if (node == null)
            {
                throw new NullReferenceException("TextNode is null");
            }

            var text = node->NodeText.ToString();
            var value = int.Parse(text);
            return value;
        }
        catch (Exception ex)
        {
            throw new AthavarPluginException(error, ex);
        }
    }

    /// <summary>
    ///     Get a pointer to the Synthesis addon.
    /// </summary>
    /// <returns>A valid pointer or throw.</returns>
    private unsafe AddonSynthesis* GetSynthesisAddon()
    {
        var ptr = this.dalamudServices.GameGui.GetAddonByName("Synthesis");
        if (ptr == nint.Zero)
        {
            throw new AthavarPluginException("Could not find Synthesis addon");
        }

        return (AddonSynthesis*)ptr;
    }

    /// <summary>
    ///     Execute a main command.
    /// </summary>
    private unsafe bool ExecuteMainCommand(uint mainCommandId)
    {
        if (!this.IsLoggedIn())
        {
            return false;
        }

        var module = (UIModule*)this.dalamudServices.GameGui.GetUIModule();
        module->ExecuteMainCommand(mainCommandId);
        return true;
    }
}