namespace Athavar.FFXIV.Plugin.Module.ItemInspector;

using System.Text;
using Athavar.FFXIV.Plugin.Extension;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Utils;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

internal class ItemInspectorTab
{
    private readonly EquipmentScanner scanner;
    private readonly IDalamudServices dalamudServices;

    public ItemInspectorTab(IDalamudServices dalamudServices, EquipmentScanner scanner)
    {
        this.scanner = scanner;
        this.dalamudServices = dalamudServices;
    }

    public unsafe void DrawTab()
    {
        using var raii = new ImGuiRaii();
        if (!raii.Begin(() => ImGui.BeginTabItem("ItemInspector"), ImGui.EndTabItem))
        {
            return;
        }

        var itemSheet = this.dalamudServices.DataManager.GetExcelSheet<Item>()!;
        var materiaSheet = this.dalamudServices.DataManager.GetExcelSheet<Materia>()!;

        foreach (var item in this.scanner.GetEquippedItems())
        {
            var dataRow = itemSheet.GetRow(item.ItemID);

            if (item.ItemID == 0 || dataRow is null)
            {
                continue;
            }

            if (ImGui.CollapsingHeader($"{dataRow.Name} ({item.Slot})"))
            {
                var materiaSlotCount = dataRow.MateriaSlotCount;
                ImGui.Text($"ItemId: {item.ItemID}");

                ImGui.Text($"Name: {dataRow.Name}");
                ImGui.Text($"Spiritbond: {item.Spiritbond}");
                ImGui.Text($"Condition: {item.Condition}");
                ImGui.Text($"Stain: {item.Stain}");
                ImGui.Text($"Quantity: {item.Quantity}");

                if (materiaSlotCount > 0)
                {
                    var materia = IntPtrExtension.CreateArray<ushort>(item.Materia, materiaSlotCount);
                    var materiaGrade = IntPtrExtension.CreateArray<byte>(item.MateriaGrade, materiaSlotCount);
                    StringBuilder strBuilder = new();
                    for (var i = 0; i < materiaSlotCount; i++)
                    {
                        var materiaRowId = materia[i];
                        var materiaGradeIndex = materiaGrade[i];
                        if (materiaRowId == 0)
                        {
                            continue;
                        }

                        var materiaData = materiaSheet.GetRow(materia[i]);
                        if (materiaData is null)
                        {
                            continue;
                        }

                        strBuilder.Append(materiaData.BaseParam.Value?.Name ?? string.Empty);
                        strBuilder.Append(": +");
                        strBuilder.Append(materiaData.Value[materiaGradeIndex]);
                        strBuilder.Append(' ');
                    }

                    ImGui.Text($"MateriaStats: {strBuilder}");
                }
            }
        }
    }
}