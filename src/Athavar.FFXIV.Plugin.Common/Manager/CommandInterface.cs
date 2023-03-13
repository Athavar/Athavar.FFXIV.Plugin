// <copyright file="CommandInterface.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Config;
using Dalamud;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;

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
    public CommandInterface(IDalamudServices dalamudServices)
    {
        this.dalamudServices = dalamudServices;
        var mainSheet = dalamudServices.DataManager.GetExcelSheet<MainCommand>(ClientLanguage.English)!;
        this.logOutId = mainSheet.FirstOrDefault(c => c.Name?.RawString == "Log Out")?.RowId ?? 0u;
    }

    /// <inheritdoc />
    public unsafe bool CanUseAction(uint actionId)
    {
        if (!this.IsLoggedIn())
        {
            return false;
        }

        var actionType = actionId >= 100000U ? ActionType.CraftAction : ActionType.Spell;
        return ActionManager.Instance()->GetActionStatus(actionType, actionId, Constants.PlayerId, true, true, null) == 0U;
    }

    /// <inheritdoc />
    public unsafe bool UseAction(uint actionId)
    {
        if (!this.IsLoggedIn())
        {
            return false;
        }

        var actionType = actionId >= 100000U ? ActionType.CraftAction : ActionType.Spell;
        return this.CanUseAction(actionId) && ActionManager.Instance()->UseAction(actionType, actionId, Constants.PlayerId, 0U, 0U, 0U, null);
    }

    /// <inheritdoc />
    public unsafe bool CanUseGeneralAction(uint actionId)
    {
        if (!this.IsLoggedIn())
        {
            return false;
        }

        return ActionManager.Instance()->GetActionStatus(ActionType.General, actionId, Constants.PlayerId, true, true, null) == 0U;
    }

    /// <inheritdoc />
    public unsafe bool UseGeneralAction(uint actionId)
    {
        if (!this.IsLoggedIn())
        {
            return false;
        }

        return this.CanUseGeneralAction(actionId) && ActionManager.Instance()->UseAction(ActionType.General, actionId, Constants.PlayerId, 0U, 0U, 0U, null);
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