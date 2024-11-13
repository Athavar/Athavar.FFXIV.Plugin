// <copyright file="GroupStateManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Runtime.InteropServices;
using Athavar.FFXIV.Plugin.Models;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using Lumina.Excel.Sheets;

/// <summary>
///     Currently unused.
/// </summary>
public class GroupStateManager : IDisposable
{
    private static readonly uint[] AllianceTextSheetRows = [1115, 1116, 1118, 1160, 1161, 1162];

    private static string[] allianceTexts;
    private readonly IFrameworkManager frameworkManager;

    public GroupStateManager(IFrameworkManager frameworkManager, IDalamudServices dalamudServices)
    {
        this.frameworkManager = frameworkManager;
        var addonSheet = dalamudServices.DataManager.GetExcelSheet<Addon>();
        allianceTexts = AllianceTextSheetRows.Select(rowId => addonSheet.GetRow(rowId).Text.ExtractText()).ToArray();

        frameworkManager.Subscribe(this.OnFrameworkUpdate);
    }

    public bool InParty { get; }

    public bool InAlliance => this.CurrentAllianceType != AllianceType.None;

    public unsafe AllianceType CurrentAllianceType => (AllianceType)this.GetGroupManager->MainGroup.AllianceFlags;

    public Alliance CurrentAlliance { get; }

    private unsafe GroupManager* GetGroupManager => GroupManager.Instance();

    public void Dispose() => this.frameworkManager.Unsubscribe(this.OnFrameworkUpdate);

    private static Alliance ToAlliance(string? allianceString)
    {
        if (string.IsNullOrWhiteSpace(allianceString))
        {
            return Alliance.None;
        }

        var index = Array.FindIndex(allianceTexts, s => s.Equals(allianceString));
        if (index >= 0)
        {
            return (Alliance)index;
        }

        return Alliance.None;
    }

    private static unsafe Alliance ToAlliance(byte* allianceString) => ToAlliance(Marshal.PtrToStringUTF8(new IntPtr(allianceString)));

    private void OnFrameworkUpdate(IFramework framework)
    {
    }
}