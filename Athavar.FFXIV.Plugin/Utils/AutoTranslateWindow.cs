namespace Athavar.FFXIV.Plugin.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Dalamud.Data;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Action = Lumina.Excel.GeneratedSheets.Action;

internal class AutoTranslateWindow : Window
{
    private readonly IDalamudServices dalamudServices;

    private (ushort GroupId, string GroupName, uint KeyId, string Text)[]? translations;

    private int[] translationsFilter = Array.Empty<int>();
    
    private string filterText = string.Empty;

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

    /// <inheritdoc />
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
                    for (int i = clipper.DisplayStart; i < Math.Min(clipper.DisplayEnd, this.translationsFilter.Length); i++)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        
                        var filterIndex = this.translationsFilter[i];
                        var (groupId, groupName, keyId, text) = this.translations[filterIndex];

                        ImGui.Text($"{groupName}[{groupId}]");
                        ImGui.TableNextColumn();
                        ImGui.Text($"{keyId}");
                        ImGui.TableNextColumn();
                        ImGui.Text(text);
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

        var lookUpGroups = completionGroups.Select(g => (Id: g.Group, LookUp: g.LookupTable.RawString)).Where(l => l.LookUp.Length > 1).ToDictionary(l => l.Id, l => new LookUpGroup(this.dalamudServices.DataManager, l.Id, l.LookUp));

        List<(ushort GroupId, string GroupName, uint KeyId, string Text)> tmpList = new();
        foreach (var completionGroup in completionGroups)
        {
            var groupId = completionGroup.Group;
            var groupName = completionGroup.GroupTitle.RawString.TrimEnd('.');

            if (lookUpGroups.TryGetValue(groupId, out var lookUpGroup))
            {
                tmpList.AddRange(from rowId in lookUpGroup.RowIds() let text = lookUpGroup.GetMessage(this.dalamudServices.DataManager, rowId) where text is not null select (groupId, groupName, rowId, text.TextValue));
            }
            else
            {
                tmpList.AddRange(from c in excelSheet where c.Group == groupId && c.Key != 0 select (groupId, groupName, (uint)c.Key, c.Text.RawString));
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

    private class LookUpGroup
    {
        private static readonly Regex LookUpRegex = new("(?<Sheet>\\w+)(?:\\[(?:(?<Colume>col\\-.*?),?)*((?:(?<RangeStart>\\d+)-(?<RangeEnd>\\d+),?)+)\\])?", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private readonly IEnumerable<ExcelRow>? sheet = null;
        private readonly List<(uint Index, int Length)> ranges = new();
        private readonly uint[] rowIds;

        public LookUpGroup(DataManager dataManager, uint group, string lookUp)
        {
            var match = LookUpRegex.Match(lookUp);
            var sheet = match.Groups["Sheet"];

            var type = GetSheetType(sheet.Value);
            if (type is not null && type.IsSubclassOf(typeof(ExcelRow)))
            {
                var methode = typeof(DataManager).GetMethods().FirstOrDefault(m => m.Name == "GetExcelSheet" && m.GetParameters().Length == 0 && m.IsGenericMethod)?.MakeGenericMethod(type);
                var sheetObject = methode?.Invoke(dataManager, null);

                this.sheet = sheetObject as IEnumerable<ExcelRow>;
            }

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
                if (this.ranges.Count == 0 && this.sheet is not null)
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
                var message = this.GetMessage(dataManager, rowId);
                if (string.IsNullOrWhiteSpace(message?.TextValue))
                {
                    continue;
                }

                ids.Add(rowId);
            }

            this.rowIds = ids.ToArray();
        }

        public SeString? GetMessage(DataManager manager, uint key) => this.sheet is not null ? GetMessage(this.sheet, key) : null;

        public uint[] RowIds() => this.rowIds;

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

        private static SeString? GetMessage<T>(IEnumerable<T> sheet, uint rowId)
            where T : ExcelRow =>
            sheet switch
            {
                ExcelSheet<Action> excelSheet => excelSheet.GetRow(rowId)?.Name.ToDalamudString(),
                ExcelSheet<ActionComboRoute> excelSheet => excelSheet.GetRow(rowId)?.Name.ToDalamudString(),
                ExcelSheet<BuddyAction> excelSheet => excelSheet.GetRow(rowId)?.Name.ToDalamudString(),
                ExcelSheet<ClassJob> excelSheet => excelSheet.GetRow(rowId)?.Name.ToDalamudString(),
                ExcelSheet<Companion> excelSheet => excelSheet.GetRow(rowId)?.Singular.ToDalamudString(),
                ExcelSheet<CraftAction> excelSheet => excelSheet.GetRow(rowId)?.Name.ToDalamudString(),
                ExcelSheet<GeneralAction> excelSheet => excelSheet.GetRow(rowId)?.Name.ToDalamudString(),
                ExcelSheet<GuardianDeity> excelSheet => excelSheet.GetRow(rowId)?.Name.ToDalamudString(),
                ExcelSheet<MainCommand> excelSheet => excelSheet.GetRow(rowId)?.Name.ToDalamudString(),
                ExcelSheet<Mount> excelSheet => excelSheet.GetRow(rowId)?.Singular.ToDalamudString(),
                ExcelSheet<Pet> excelSheet => excelSheet.GetRow(rowId)?.Name.ToDalamudString(),
                ExcelSheet<PetMirage> excelSheet => excelSheet.GetRow(rowId)?.Name.ToDalamudString(),
                ExcelSheet<PetAction> excelSheet => excelSheet.GetRow(rowId)?.Name.ToDalamudString(),
                ExcelSheet<PlaceName> excelSheet => excelSheet.GetRow(rowId)?.Name.ToDalamudString(),
                ExcelSheet<Race> excelSheet => excelSheet.GetRow(rowId)?.Masculine.ToDalamudString(),
                ExcelSheet<TextCommand> excelSheet => excelSheet.GetRow(rowId)?.Command.ToDalamudString(),
                ExcelSheet<Tribe> excelSheet => excelSheet.GetRow(rowId)?.Masculine.ToDalamudString(),
                ExcelSheet<Weather> excelSheet => excelSheet.GetRow(rowId)?.Name.ToDalamudString(),
                _ => throw new Exception($"Not mapped Sheet of type {sheet.GetType().FullName}"),
            };
    }
}