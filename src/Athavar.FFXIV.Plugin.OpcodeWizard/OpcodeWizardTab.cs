// <copyright file="OpcodeWizardTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.OpcodeWizard;

using System.Text;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.UI;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.OpcodeWizard.Models;
using Athavar.FFXIV.Plugin.OpcodeWizard.PacketDetection;
using Dalamud.Interface;
using ImGuiNET;

internal class OpcodeWizardTab : Tab, IOpcodeWizardTab
{
    private readonly ScannerRegistry scannerRegistry;
    private readonly DetectionProgram detectionProgram;
    private readonly IDefinitionManager definitionManager;
    private readonly IOpcodeManager opcodeManager;

    private Scanner? selectedScanner;

    private DateTime requestParameterModalReopenWaitTime = DateTime.MinValue;
    private TaskCompletionSource<(string parameter, bool skipRequested)>? requestParameterCompletionSource;

    private string requestParameterText = string.Empty;
    private string requestParameterValue = string.Empty;

    private int oldIndex = -1;

    private string resultAffix = string.Empty;

    private string resultContent = string.Empty;

    public OpcodeWizardTab(IDefinitionManager definitionManager, ScannerRegistry scannerRegistry, DetectionProgram detectionProgram, IOpcodeManager opcodeManager)
    {
        this.definitionManager = definitionManager;
        this.scannerRegistry = scannerRegistry;
        this.detectionProgram = detectionProgram;
        this.opcodeManager = opcodeManager;
    }

    public override string Name => OpcodeWizardModule.ModuleName;

    public override string Identifier => "opcodeWizard";

    public override void Draw()
    {
        var currentIndex = this.detectionProgram.ScannerIndex;
        if (this.oldIndex != currentIndex)
        {
            this.UpdateResult();
            this.selectedScanner = this.scannerRegistry.GetScanner(currentIndex);
            this.oldIndex = currentIndex;
        }

        this.DrawPopupModal();

        ImGui.Columns(3);

        if (ImGui.BeginChild("##scanner", ImGui.GetContentRegionAvail(), false))
        {
            this.DrawScannerList();
            ImGui.EndChild();
        }

        ImGui.NextColumn();
        this.DrawSelectedScannerDetail();
        ImGui.NextColumn();
        this.DrawResults();

        ImGui.Columns(1);
    }

    private async Task<(string parameter, bool skipRequested)> RequestParameter(Scanner scanner, int index)
    {
        this.requestParameterText = scanner.ParameterPrompts[index];
        this.requestParameterValue = string.Empty;
        this.requestParameterModalReopenWaitTime = DateTime.MinValue;
        this.requestParameterCompletionSource = new TaskCompletionSource<(string parameter, bool skipRequested)>();

        var result = await this.requestParameterCompletionSource.Task;
        this.requestParameterCompletionSource = null;
        return result;
    }

    private void DrawPopupModal()
    {
        var popupTitle = "Await Input";
        if (this.requestParameterCompletionSource is null)
        {
            if (ImGui.IsPopupOpen(popupTitle))
            {
                ImGui.CloseCurrentPopup();
            }

            return;
        }

        if (this.requestParameterModalReopenWaitTime > DateTime.UtcNow)
        {
            return;
        }

        var open = this.requestParameterCompletionSource is not null;
        if (open && !ImGui.IsPopupOpen(popupTitle))
        {
            ImGui.OpenPopup(popupTitle);
        }

        if (ImGui.BeginPopupModal(popupTitle, ref open))
        {
            ImGui.TextUnformatted(this.requestParameterText);
            ImGui.InputText("##imput", ref this.requestParameterValue, 256);

            if (ImGui.Button("Submit"))
            {
                this.requestParameterCompletionSource?.TrySetResult((this.requestParameterValue, false));
            }

            ImGui.SameLine();
            if (ImGui.Button("Wait"))
            {
                this.requestParameterModalReopenWaitTime = DateTime.UtcNow.AddSeconds(10);
            }

            ImGui.SameLine();
            if (ImGui.Button("Skip"))
            {
                this.requestParameterCompletionSource?.TrySetResult((string.Empty, true));
            }

            ImGui.EndPopup();
        }
    }

    private void DrawScannerList()
    {
        if (ImGuiEx.IconButton(FontAwesomeIcon.Recycle, "Reset", disabled: this.detectionProgram.IsRunning))
        {
            foreach (var scanner in this.scannerRegistry.AsList())
            {
                scanner.Opcode = 0;
            }
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.PlayCircle, "Run", disabled: this.detectionProgram.IsRunning))
        {
            Task.Run(() => this.detectionProgram.Run(0, this.RequestParameter));
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.StopCircle, "Stop", disabled: !this.detectionProgram.IsRunning))
        {
            this.detectionProgram.Stop();
            this.requestParameterCompletionSource?.TrySetResult((string.Empty, true));
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.Forward, "Skip", disabled: !this.detectionProgram.IsRunning))
        {
            this.detectionProgram.Skip();
        }

        ImGui.SameLine();
        ImGui.Checkbox("Debug", ref this.detectionProgram.Debug);

        if (ImGui.BeginListBox("##list", ImGui.GetContentRegionAvail()))
        {
            var list = this.scannerRegistry.AsList();
            for (var index = 0; index < list.Count; index++)
            {
                var scanner = list[index];
                DrawScanner(scanner, index);
            }

            ImGui.EndListBox();
        }

        void DrawScanner(Scanner scanner, int index)
        {
            ImGuiEx.Icon(scanner.Opcode == 0 ? FontAwesomeIcon.TimesCircle : FontAwesomeIcon.CheckCircle, width: 32);
            ImGui.SameLine();
            ImGui.Selectable(scanner.PacketName, this.detectionProgram.ScannerIndex == index, ImGuiSelectableFlags.SpanAllColumns);
            if (ImGui.IsItemClicked())
            {
                this.detectionProgram.SetIndex(index);
            }

            if (!this.detectionProgram.IsRunning && ImGui.BeginPopupContextItem($"scanner_{index}"))
            {
                if (scanner.DependentScanner is null)
                {
                    if (ImGui.Selectable("Run this"))
                    {
                        this.detectionProgram.SetIndex(index);
                        Task.Run(() => this.detectionProgram.RunOne(this.RequestParameter));
                    }

                    if (ImGui.Selectable("Run from here"))
                    {
                        Task.Run(() => this.detectionProgram.Run(index, this.RequestParameter));
                    }
                }
                else
                {
                    ImGui.TextUnformatted($"Run from \"{scanner.DependentScanner.Value.AsText()}\"");
                }

                if (ImGui.Selectable("Reset"))
                {
                    scanner.Opcode = 0;
                    this.opcodeManager.Remove(scanner.OpcodeType);
                }

                ImGui.EndPopup();
            }
        }
    }

    private void DrawSelectedScannerDetail()
    {
        ImGui.TextUnformatted("Package Name: ");
        ImGui.SameLine();
        ImGui.TextUnformatted(this.selectedScanner?.PacketName);

        ImGui.TextUnformatted("Opcode: ");
        if (this.selectedScanner?.Opcode > 0)
        {
            ImGui.SameLine();
            ImGui.TextUnformatted($"{Util.NumberToString(this.selectedScanner.Opcode, NumberDisplayFormat.Decimal)} | {Util.NumberToString(this.selectedScanner.Opcode, NumberDisplayFormat.HexadecimalUppercase)}");
        }

        ImGui.TextUnformatted("Packet source: ");
        ImGui.SameLine();
        ImGui.TextUnformatted($"{this.selectedScanner?.PacketSource.ToString()}");

        ImGui.Spacing();
        ImGui.TextUnformatted(this.selectedScanner?.Tutorial);
    }

    private void DrawResults()
    {
        ImGui.TextUnformatted("Results");
        ImGui.InputText("Affix", ref this.resultAffix, 256);

        ImGui.BeginDisabled();
        ImGui.InputTextMultiline("##result-content", ref this.resultContent, 100_000, ImGui.GetContentRegionAvail());
        ImGui.EndDisabled();
    }

    private void UpdateResult() => this.resultContent = this.GetScannerResult();

    private string GetScannerResult()
    {
        var serverSb = new StringBuilder();
        var clientSb = new StringBuilder();

        var format = NumberDisplayFormat.Decimal;
        foreach (var scanner in this.scannerRegistry.AsList())
        {
            if (scanner.Opcode == 0)
            {
                continue;
            }

            var sb = scanner.PacketSource == PacketSource.Client ? clientSb : serverSb;

            sb.Append(scanner.PacketName).Append(" = ")
               .Append(Util.NumberToString(scanner.Opcode, format)).Append(",");
            if (scanner.Comment.Text != null)
            {
                sb.Append(" (").Append(scanner.Comment).Append(")");
            }

            sb.AppendLine();
        }

        var resultSb = new StringBuilder();
        resultSb.Append("// GameVersion: ");
        resultSb.Append(this.definitionManager.StartInfo.GameVersion);
        resultSb.AppendLine();
        resultSb.Append("// Server Zone");
        resultSb.AppendLine();
        resultSb.Append(serverSb);
        resultSb.AppendLine();
        resultSb.Append("// Client Zone");
        resultSb.AppendLine();
        resultSb.Append(clientSb);
        resultSb.AppendLine();
        return resultSb.ToString();
    }
}