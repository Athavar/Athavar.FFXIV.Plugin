// <copyright file="AutoTranslateWindow.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.UI;

using System.Numerics;
using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.Sheets;

internal sealed class AutoTranslateWindow : Window
{
    private readonly IDalamudServices dalamudServices;

    private (ushort GroupId, string GroupName, uint KeyId, string Text)[]? translations;

    private int[] translationsFilter = Array.Empty<int>();

    private string filterText = string.Empty;
    private string selectedId = string.Empty;

    public AutoTranslateWindow(IDalamudServices dalamudServices, WindowSystem windowSystem)
        : base("AutoTranslate")
    {
        this.dalamudServices = dalamudServices;
        this.Size = new Vector2(525, 600);
        this.SizeCondition = ImGuiCond.FirstUseEver;

        windowSystem.AddWindow(this);
    }

    public override void OnOpen()
    {
        base.OnOpen();
        this.Populate();
    }

    public override void OnClose()
    {
        base.OnClose();
        this.translations = null;
        this.translationsFilter = Array.Empty<int>();
    }

    /// <inheritdoc/>
    public override void Draw()
    {
        ImGui.LabelText("##Filter", "Filter");
        ImGui.SameLine();
        if (ImGui.InputText("##Filter", ref this.filterText, 254))
        {
            this.Filter(this.filterText);
        }

        ImGui.BeginChild("##TranslationList");

        if (ImGui.BeginTable("##TranslationTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("Group", ImGuiTableColumnFlags.DefaultSort);
            ImGui.TableSetupColumn("Key", ImGuiTableColumnFlags.DefaultSort);
            ImGui.TableSetupColumn("Text", ImGuiTableColumnFlags.DefaultSort);
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            if (this.translations is not null)
            {
                ImGuiListClipperPtr clipper;
                unsafe
                {
                    clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
                }

                clipper.Begin(this.translations.Length, ImGui.CalcTextSize("Test").Y + ImGui.GetStyle().ItemSpacing.Y);

                while (clipper.Step())
                {
                    for (var i = clipper.DisplayStart; i < Math.Min(clipper.DisplayEnd, this.translationsFilter.Length); i++)
                    {
                        var filterIndex = this.translationsFilter[i];
                        var (groupId, groupName, keyId, text) = this.translations[filterIndex];
                        var id = $"{{t:{groupId}:{keyId}}}";

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        var selected = this.selectedId.Equals(id);
                        if (ImGui.Selectable($"{groupName}[{groupId}]", ref selected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowDoubleClick))
                        {
                            if (selected)
                            {
                                ImGui.SetClipboardText(id);
                            }
                            else
                            {
                                this.selectedId = string.Empty;
                            }
                        }

                        if (ImGui.TableSetColumnIndex(1))
                        {
                            ImGui.Text($"{keyId}");
                        }

                        if (ImGui.TableSetColumnIndex(2))
                        {
                            ImGui.Text(text);
                        }
                    }
                }

                clipper.Destroy();
            }

            ImGui.EndTable();
        }

        ImGui.EndChild();
    }

    private void Populate()
    {
        var dataManager = this.dalamudServices.DataManager;
        var excelSheet = dataManager.GetExcelSheet<Completion>();
        if (excelSheet is null)
        {
            return;
        }

        var completionGroups = excelSheet.Where(c => c.Key == 0).ToList();

        var lookUpGroups = completionGroups.Select(g => (Id: g.Group, LookUp: g.LookupTable.ExtractText())).Where(l => l.LookUp.Length > 1).ToDictionary(l => l.Id, l => LookUpGroup.CreateLookUpGroup(this.dalamudServices.DataManager, l.LookUp));

        List<(ushort GroupId, string GroupName, uint KeyId, string Text)> tmpList = new();
        foreach (var completionGroup in completionGroups)
        {
            var groupId = completionGroup.Group;
            var groupName = completionGroup.GroupTitle.ExtractText().TrimEnd('.');

            if (lookUpGroups.TryGetValue(groupId, out var lookUpGroup))
            {
                tmpList.AddRange(from rowId in lookUpGroup.RowIds() let text = lookUpGroup.GetMessage(rowId) where text is not null select (groupId, groupName, rowId, text.TextValue));
            }
            else
            {
                tmpList.AddRange(from c in excelSheet where c.Group == groupId && c.Key != 0 select (groupId, groupName, c.RowId, c.Text.ExtractText()));
            }
        }

        this.translations = tmpList.ToArray();
        this.Filter(string.Empty);
    }

    private void Filter(string filterWord)
    {
        if (this.translations is null)
        {
            return;
        }

        List<int> filterList = new();
        for (var i = 0; i < this.translations.Length; i++)
        {
            var (groupId, groupName, keyId, text) = this.translations[i];

            if (groupName.Contains(filterWord) || text.Contains(filterWord))
            {
                filterList.Add(i);
            }
        }

        this.translationsFilter = filterList.ToArray();
    }

    private abstract class LookUpGroup
    {
        protected static readonly Regex LookUpRegex = new("(?<Sheet>\\w+)(?:\\[(?:(?<Colume>col\\-.*?),?)*((?:(?<RangeStart>\\d+)-(?<RangeEnd>\\d+),?)+)\\])?", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        protected static readonly Type RowInterfaceType = typeof(Item).GetInterfaces()[0];

        public abstract uint[] RowIds();

        public abstract SeString? GetMessage(uint key);

        private static Type? GetSheetType(string sheetName)
            => sheetName switch
            {
                "Action" => typeof(Action),
                "ActionComboRoute" => typeof(ActionComboRoute),
                "BuddyAction" => typeof(BuddyAction),
                "ClassJob" => typeof(ClassJob),
                "Companion" => typeof(Companion),
                "CraftAction" => typeof(CraftAction),
                "GeneralAction" => typeof(GeneralAction),
                "GuardianDeity" => typeof(GuardianDeity),
                "MainCommand" => typeof(MainCommand),
                "Mount" => typeof(Mount),
                "Pet" => typeof(Pet),
                "PetAction" => typeof(PetMirage),
                "PetMirage" => typeof(PetAction),
                "PlaceName" => typeof(PlaceName),
                "Race" => typeof(Race),
                "TextCommand" => typeof(TextCommand),
                "Tribe" => typeof(Tribe),
                "Weather" => typeof(Weather),
                _ => null,
            };

        public static LookUpGroup? CreateLookUpGroup(IDataManager dataManager, string lookUp)
        {
            var match = LookUpRegex.Match(lookUp);
            var sheet = match.Groups["Sheet"];

            var type = GetSheetType(sheet.Value);
            if (type is not null && type.IsSubclassOf(RowInterfaceType))
            {
                var methode = typeof(IDataManager).GetMethods().FirstOrDefault(m => m.Name == "GetExcelSheet" && m.GetParameters().Length == 0 && m.IsGenericMethod)?.MakeGenericMethod(type);
                var sheetObject = methode?.Invoke(dataManager, null);

                var t = typeof(LookUpGroup<>).MakeGenericType(type);
                return (LookUpGroup?)Activator.CreateInstance(t, sheetObject, match);
            }

            return null;
        }
    }

    private class LookUpGroup<T> : LookUpGroup
        where T : struct, IExcelRow<T>
    {
        private readonly ExcelSheet<T> sheet;
        private readonly List<(uint Index, int Length)> ranges = new();
        private readonly uint[] rowIds;

        public LookUpGroup(ExcelSheet<T> sheet, Match match)
        {
            this.sheet = sheet;
            var sRange = match.Groups["RangeStart"].Captures.Select(c => uint.TryParse(c.Value, out var v) ? v : 0).Where(v => v != 0).ToList();
            var eRange = match.Groups["RangeEnd"].Captures.Select(c => uint.TryParse(c.Value, out var v) ? v : 0).Where(v => v != 0).ToList();

            if (sRange.Count != eRange.Count)
            {
                throw new Exception();
            }

            for (var i = 0; i < sRange.Count; i++)
            {
                this.ranges.Add((sRange[i], (int)((eRange[i] - sRange[i]) + 1)));
            }

            IEnumerable<uint> GetRowIds()
            {
                if (this.ranges.Count == 0)
                {
                    foreach (var row in this.sheet)
                    {
                        yield return row.RowId;
                    }
                }
                else
                {
                    foreach (var range in this.ranges)
                    {
                        for (uint i = 0; i < range.Length; i++)
                        {
                            yield return range.Index + i;
                        }
                    }
                }
            }

            // get valid rows
            List<uint> ids = new();
            foreach (var rowId in GetRowIds())
            {
                var message = this.GetMessage(rowId);
                if (string.IsNullOrWhiteSpace(message?.TextValue))
                {
                    continue;
                }

                ids.Add(rowId);
            }

            this.rowIds = ids.ToArray();
        }

        public override SeString? GetMessage(uint key) => GetMessage(this.sheet, key);

        public override uint[] RowIds() => this.rowIds;

        private static SeString? GetMessage(IEnumerable<T> sheet, uint rowId)
            => sheet switch
            {
                ExcelSheet<Action> excelSheet => excelSheet.GetRow(rowId).Name.ToDalamudString(),
                ExcelSheet<ActionComboRoute> excelSheet => excelSheet.GetRow(rowId).Name.ToDalamudString(),
                ExcelSheet<BuddyAction> excelSheet => excelSheet.GetRow(rowId).Name.ToDalamudString(),
                ExcelSheet<ClassJob> excelSheet => excelSheet.GetRow(rowId).Name.ToDalamudString(),
                ExcelSheet<Companion> excelSheet => excelSheet.GetRow(rowId).Singular.ToDalamudString(),
                ExcelSheet<CraftAction> excelSheet => excelSheet.GetRow(rowId).Name.ToDalamudString(),
                ExcelSheet<GeneralAction> excelSheet => excelSheet.GetRow(rowId).Name.ToDalamudString(),
                ExcelSheet<GuardianDeity> excelSheet => excelSheet.GetRow(rowId).Name.ToDalamudString(),
                ExcelSheet<MainCommand> excelSheet => excelSheet.GetRow(rowId).Name.ToDalamudString(),
                ExcelSheet<Mount> excelSheet => excelSheet.GetRow(rowId).Singular.ToDalamudString(),
                ExcelSheet<Pet> excelSheet => excelSheet.GetRow(rowId).Name.ToDalamudString(),
                ExcelSheet<PetMirage> excelSheet => excelSheet.GetRow(rowId).Name.ToDalamudString(),
                ExcelSheet<PetAction> excelSheet => excelSheet.GetRow(rowId).Name.ToDalamudString(),
                ExcelSheet<PlaceName> excelSheet => excelSheet.GetRow(rowId).Name.ToDalamudString(),
                ExcelSheet<Race> excelSheet => excelSheet.GetRow(rowId).Masculine.ToDalamudString(),
                ExcelSheet<TextCommand> excelSheet => excelSheet.GetRow(rowId).Command.ToDalamudString(),
                ExcelSheet<Tribe> excelSheet => excelSheet.GetRow(rowId).Masculine.ToDalamudString(),
                ExcelSheet<Weather> excelSheet => excelSheet.GetRow(rowId).Name.ToDalamudString(),
                _ => throw new Exception($"Not mapped Sheet of type {sheet.GetType().FullName}"),
            };
    }
}