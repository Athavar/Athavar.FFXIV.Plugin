// <copyright file="CraftDataManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;
using Dalamud;
using Dalamud.Data;
using Lumina.Excel.GeneratedSheets;

internal sealed class CraftDataManager : ICraftDataManager
{
    private static readonly Regex ActionRegex = new(@"^(/(?:ac|action)\s+)?(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex WaitRegex = new(@"(?<modifier><wait\.(?<wait>\d+(?:\.\d+)?)(?:-(?<until>\d+(?:\.\d+)?))?>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly CraftingClass?[] AllCraftingClass =
    {
        CraftingClass.CRP,
        CraftingClass.BSM,
        CraftingClass.ARM,
        CraftingClass.GSM,
        CraftingClass.LTW,
        CraftingClass.WVR,
        CraftingClass.ALC,
        CraftingClass.CUL,
    };

    private readonly Dictionary<CraftingSkills, ICraftDataManager.CraftSkillData> craftSkillDatas = new();

    private readonly DataManager dataManager;

    public CraftDataManager(IDalamudServices dalamudServices)
    {
        this.dataManager = dalamudServices.DataManager;

        this.Populate();
    }

    public ICraftDataManager.CraftSkillData GetCraftSkillData(CraftingSkill skill) => this.GetCraftSkillData(skill.Skill);

    public ICraftDataManager.CraftSkillData GetCraftSkillData(CraftingSkills skill) => this.craftSkillDatas[skill];

    public string CreateTextMacro(CraftingMacro macro, ClientLanguage language)
    {
        string Process(CraftingSkill skill) => $"/ac \"{this.GetCraftSkillData(skill).Name[language]}\" <wait.{skill.Action.GetWaitDuration()}>";

        return string.Join("\r\n", macro.Rotation.Select(Process));
    }

    public CraftingMacro ParseCraftingMacro(string text) => CraftingSkill.ParseMacro(this.Parse(text));

    public CraftingSkills[] Parse(string text)
    {
        string? line;
        using var reader = new StringReader(text);

        List<CraftingSkills> skillsList = new();
        while ((line = reader.ReadLine()) != null)
        {
            var match = WaitRegex.Match(line);

            if (match.Success)
            {
                var group = match.Groups["modifier"];
                line = line.Remove(group.Index, group.Length);
            }

            match = ActionRegex.Match(line);
            if (match.Success)
            {
                var name = match.ExtractAndUnquote("name");
                var skill = this.FindAction(name);
                if (skill is not null)
                {
                    skillsList.Add(skill.Skill);
                }
            }
        }

        return skillsList.ToArray();
    }

    private CraftingSkill? FindAction(string name)
    {
        var skill = this.craftSkillDatas.FirstOrDefault(a => a.Value.Name.Equals(name)).Key;
        if (skill is not 0)
        {
            return CraftingSkill.FindAction(skill);
        }

        return null;
    }

    private void Populate()
    {
        foreach (var skill in CraftingSkill.AllSkills())
        {
            var value = skill.Action;
            CraftingClass?[] loopClasses;
            if (value.Class == CraftingClass.ANY)
            {
                loopClasses = AllCraftingClass;
            }
            else
            {
                loopClasses = new CraftingClass?[8];
                loopClasses[(int)value.Class] = value.Class;
            }

            uint actionId = 0;
            var useCraftActionSheet = false;
            var iconIds = new ushort[8];
            for (var index = 0; index < loopClasses.Length; index++)
            {
                var loopClass = loopClasses[index];
                if (loopClass is null)
                {
                    continue;
                }

                actionId = value.GetId(loopClass.Value);
                useCraftActionSheet = actionId > 100000;

                if (useCraftActionSheet)
                {
                    var row = this.dataManager.GetExcelSheet<CraftAction>()!.GetRow(actionId);
                    if (actionId != 0 && row is not null)
                    {
                        iconIds[index] = row.Icon;
                    }
                }
                else
                {
                    var row = this.dataManager.GetExcelSheet<Action>()!.GetRow(actionId);
                    if (actionId != 0 && row is not null)
                    {
                        iconIds[index] = row.Icon;
                    }
                }
            }

            this.craftSkillDatas.Add(skill.Skill, new ICraftDataManager.CraftSkillData(useCraftActionSheet ? MultiString.FromCraftAction(this.dataManager, actionId) : MultiString.FromAction(this.dataManager, actionId), iconIds));
        }
    }
}