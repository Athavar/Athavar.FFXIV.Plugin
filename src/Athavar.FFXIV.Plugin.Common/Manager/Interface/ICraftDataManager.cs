// <copyright file="ICraftDataManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;
using Dalamud.Game;

public interface ICraftDataManager
{
    CraftSkillData GetCraftSkillData(CraftingSkill skill);

    string CreateTextMacro(CraftingMacro macro, ClientLanguage language);

    CraftingMacro ParseCraftingMacro(string text);

    public sealed record CraftSkillData(MultiString Name, ushort[] IconIds);
}