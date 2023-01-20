// <copyright file="CommandInterface.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Manager;

using System;
using System.Linq;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Module.Macro.Exceptions;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Status = Lumina.Excel.GeneratedSheets.Status;

/// <summary>
///     Miscellaneous functions that commands/scripts can use.
/// </summary>
internal partial class CommandInterface : ICommandInterface
{
    private readonly IDalamudServices dalamudServices;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandInterface" /> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="dalamudServices" /> added by DI.</param>
    public CommandInterface(IDalamudServices dalamudServices) => this.dalamudServices = dalamudServices;

    /// <inheritdoc />
    public bool HasStatus(string statusName)
    {
        statusName = statusName.ToLowerInvariant();
        var sheet = this.dalamudServices.DataManager.GetExcelSheet<Status>()!;
        var statusIDs = sheet
           .Where(row => row.Name.RawString.ToLowerInvariant() == statusName)
           .Select(row => row.RowId)
           .ToArray()!;

        return this.HasStatusId(statusIDs);
    }

    /// <inheritdoc />
    public bool HasStatusId(params uint[] statusIDs)
    {
        var statusID = this.dalamudServices.ClientState.LocalPlayer!.StatusList
           .Select(se => se.StatusId)
           .ToList().Intersect(statusIDs)
           .FirstOrDefault();

        return statusID != default;
    }

    /// <inheritdoc />
    public string? GetCurrentTarget() => this.dalamudServices.TargetManager.Target?.Name.ToString();

    /// <inheritdoc />
    public string? GetCurrentFocusTarget() => this.dalamudServices.TargetManager.FocusTarget?.Name.ToString();

    /// <inheritdoc />
    public unsafe bool CanUseAction(uint actionId)
    {
        var actionType = actionId >= 100000U ? ActionType.CraftAction : ActionType.Spell;
        return ActionManager.Instance()->GetActionStatus(actionType, actionId, 3758096384L, true, true, null) == 0U;
    }

    /// <inheritdoc />
    public unsafe bool UseAction(uint actionId)
    {
        var actionType = actionId >= 100000U ? ActionType.CraftAction : ActionType.Spell;
        return this.CanUseAction(actionId) && ActionManager.Instance()->UseAction(actionType, actionId, 3758096384L, 0U, 0U, 0U, null);
    }

    /// <inheritdoc />
    public unsafe bool CanUseGeneralAction(uint actionId) => ActionManager.Instance()->GetActionStatus(ActionType.General, actionId, 3758096384L, true, true, null) == 0U;

    /// <inheritdoc />
    public unsafe bool UseGeneralAction(uint actionId) => this.CanUseAction(actionId) && ActionManager.Instance()->UseAction(ActionType.General, actionId, 3758096384L, 0U, 0U, 0U, null);

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
            throw new MacroCommandError(error, ex);
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
            throw new MacroCommandError("Could not find Synthesis addon");
        }

        return (AddonSynthesis*)ptr;
    }
}