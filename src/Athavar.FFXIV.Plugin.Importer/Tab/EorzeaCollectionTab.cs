// <copyright file="EorzeaCollectionTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Importer.Tab;

using System.Numerics;
using System.Text.RegularExpressions;
using System.Web;
using Athavar.FFXIV.Plugin.Common.UI;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Importer.Models.Glamourer;
using Athavar.FFXIV.Plugin.Models.Constants;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using HtmlAgilityPack;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Item = Lumina.Excel.GeneratedSheets.Item;

internal sealed class EorzeaCollectionTab : Tab
{
    private const string UrlStart = "https://ffxiv.eorzeacollection.com/glamour/";
    private const string UrlHint = $"{UrlStart}{{id}}...";

    private readonly IDalamudServices dalamudServices;
    private readonly IIconManager iconManager;
    private readonly IIpcManager ipcManager;

    private readonly ExcelSheet<Item> englishItemSheet;
    private readonly ExcelSheet<Item>? itemSheet;
    private readonly ExcelSheet<Stain> englishStainSheet;
    private readonly ExcelSheet<Stain>? stainSheet;

    private bool loading;

    private string glamourUrl = string.Empty;
    private string lastLoadedGlamourId = string.Empty;

    private string glamourTitle = string.Empty;
    private string glamourAuthor = string.Empty;
    private string[] glamourTags = [];
    private List<EquipmentSlotItem> currentEquipmentSlots = [];

    public EorzeaCollectionTab(IDalamudServices dalamudServices, IIconManager iconManager, IIpcManager ipcManager)
    {
        this.dalamudServices = dalamudServices;
        this.iconManager = iconManager;
        this.ipcManager = ipcManager;

        this.englishItemSheet = this.dalamudServices.DataManager.GetExcelSheet<Item>(ClientLanguage.English)!;
        this.englishStainSheet = this.dalamudServices.DataManager.GetExcelSheet<Stain>(ClientLanguage.English)!;

        if (this.dalamudServices.DataManager.Language != ClientLanguage.English)
        {
            this.itemSheet = this.dalamudServices.DataManager.GetExcelSheet<Item>()!;
            this.stainSheet = this.dalamudServices.DataManager.GetExcelSheet<Stain>()!;
        }
    }

    public override string Name => "eorzeacollection";

    public override string Identifier => "eorzeacollection";

    public override void Draw()
    {
        if (ImGui.InputTextWithHint("##url", UrlHint, ref this.glamourUrl, 512, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            _ = Task.Run(this.LoadWebPageData);
        }

        if (this.loading)
        {
            ImGui.SameLine();
            ImGuiEx.Icon(FontAwesomeIcon.Sync);
        }

        ImGui.Separator();

        if (this.currentEquipmentSlots.Count > 0)
        {
            if (ImGuiEx.IconButton(FontAwesomeIcon.Copy, "Copy the design to your clipboard."))
            {
                this.ExportToClipboard();
            }

            if (this.ipcManager.GlamourerEnabled)
            {
                ImGui.SameLine();
                if (ImGui.Button("Apply to Yourself", Vector2.Zero))
                {
                    var localPlayer = this.dalamudServices.ClientState.LocalPlayer;
                    if (localPlayer != null)
                    {
                        this.ApplyTo(localPlayer);
                    }
                }

                ImGuiEx.TextTooltip("Apply the design to your character.");

                ImGui.SameLine();
                if (ImGui.Button("Apply to Target", Vector2.Zero))
                {
                    var target = this.dalamudServices.ClientState.IsGPosing ? this.dalamudServices.TargetManager.GPoseTarget : this.dalamudServices.TargetManager.Target;
                    if (target is Character character)
                    {
                        this.ApplyTo(character);
                    }
                }

                ImGuiEx.TextTooltip("Apply the design to your current target.");
            }

            ImGui.Separator();

            ImGui.TextUnformatted("Name: ");
            ImGui.SameLine();
            ImGui.TextUnformatted(this.glamourTitle);

            ImGui.TextUnformatted("Author: ");
            ImGui.SameLine();
            ImGui.TextUnformatted(this.glamourAuthor);

            var lineHeight = ImGui.GetTextLineHeight();
            var iconSize = new Vector2(lineHeight * 2, lineHeight * 2);
            foreach (var equipmentSlot in this.currentEquipmentSlots)
            {
                using (_ = ImRaii.Group())
                {
                    var tex = this.iconManager.GetIcon(equipmentSlot.ItemIcon);
                    if (tex is null)
                    {
                        continue;
                    }

                    ImGui.Separator();
                    using (_ = ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 2 * ImGuiHelpers.GlobalScale))
                    {
                        ImGui.Image(tex.ImGuiHandle, iconSize);
                    }

                    ImGui.SameLine();

                    using (_ = ImRaii.Group())
                    {
                        ImGui.TextUnformatted(equipmentSlot.ItemName);

                        if (equipmentSlot.StrainId is not null && equipmentSlot.RgbaColor is not null)
                        {
                            using (var c = new ImRaii.Color())
                            {
                                c.Push(ImGuiCol.Button, equipmentSlot.RgbaColor.Value);
                                ImGui.BeginDisabled();
                                ImGui.Button("  ");
                                ImGui.EndDisabled();
                            }

                            ImGui.SameLine();
                            ImGui.TextUnformatted(equipmentSlot.StainName);
                        }
                        else
                        {
                            ImGui.NewLine();
                        }
                    }
                }

                if (ImGui.IsItemClicked())
                {
                    AgentTryon.TryOn(0, equipmentSlot.ItemId, (byte)(equipmentSlot.StrainId ?? 0), 0, 0);
                }
            }
        }
    }

    /// <summary> Square stores its colors as BGR values so R and B need to be shuffled and Alpha set to max. </summary>
    private static uint SeColorToRgba(uint color) => (color & 0xFF) << 16 | color >> 16 & 0xFF | color & 0xFF00 | 0xFF000000;

    private void ApplyTo(Character character)
    {
        foreach (var equipmentSlot in this.currentEquipmentSlots)
        {
            var x = this.ipcManager.SetItem(character, equipmentSlot.EquipSlot, equipmentSlot.ItemId, (byte)equipmentSlot.StrainId.GetValueOrDefault(), 0);
            this.dalamudServices.PluginLogger.Information("{Item} -> {Result}", equipmentSlot.ItemName, x);
        }
    }

    private async Task LoadWebPageData()
    {
        if (this.loading)
        {
            return;
        }

        this.loading = true;
        try
        {
            if (long.TryParse(this.glamourUrl, out var glamourId))
            {
                this.glamourUrl = $"{UrlStart}{glamourId}";
            }

            if (!this.glamourUrl.StartsWith(UrlStart))
            {
                return;
            }

            var m = Regex.Match(this.glamourUrl, "\\d+");
            if (m.Success && m.Value != this.lastLoadedGlamourId && Uri.TryCreate(this.glamourUrl, UriKind.RelativeOrAbsolute, out var uri))
            {
                var siteData = await this.dalamudServices.HttpClient.GetStringAsync(uri);
                this.ipcManager.UpdateActivePluginState();

                var equipmentSlots = new List<EquipmentSlotItem>();

                HtmlDocument document = new();
                document.LoadHtml(siteData);
                var titleNode = document.DocumentNode.SelectSingleNode("//b[contains(concat(' ', normalize-space(@class), ' '), 'b-title-text-bold')]");
                this.glamourTitle = titleNode.InnerText;
                var authorNode = document.DocumentNode.SelectSingleNode("//span[contains(concat(' ', normalize-space(@class), ' '), 'b-user-info-text-name')]");
                this.glamourAuthor = authorNode.InnerText;

                var tagNodes = document.DocumentNode.SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), 'c-tag')]");
                this.glamourTags = tagNodes.Select(tn => tn.InnerText).ToArray();

                var items = document.DocumentNode.SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), 'b-info-box-item-wrapper')]");
                foreach (var itemNode in items)
                {
                    var urlNode = itemNode.ChildNodes.Last(n => n.Name == "a");
                    var path = urlNode.GetAttributeValue("href", string.Empty);
                    var match = Regex.Match(path, "\\/glamours\\/(?<type>\\w+)\\/(?<itemId>\\d+)");
                    if (match.Success)
                    {
                        var slotType = match.Groups["type"].Value;

                        var slot = this.GetSlot(slotType);
                        if (slot is null)
                        {
                            continue;
                        }

                        if (slot is EquipSlot.RFinger && equipmentSlots.Any(e => e.EquipSlot == EquipSlot.RFinger))
                        {
                            slot = EquipSlot.LFinger;
                        }

                        var itemNameNode = itemNode.SelectSingleNode(".//span[contains(concat(' ', normalize-space(@class), ' '), 'c-gear-slot-item-name')]");
                        var itemName = HttpUtility.HtmlDecode(itemNameNode.InnerText);
                        var item = this.englishItemSheet.FirstOrDefault(i => i.Name.RawString.Equals(itemName));
                        if (item is null)
                        {
                            this.dalamudServices.PluginLogger.Warning("Item {Item} not found", itemName);
                            continue;
                        }

                        if (this.itemSheet is not null)
                        {
                            // use item from a sheet with the client localisation.
                            item = this.itemSheet.GetRow(item.RowId)!;
                        }

                        Stain? stain = null;
                        var colorNode = itemNode.SelectSingleNode(".//span[contains(concat(' ', normalize-space(@class), ' '), 'c-gear-slot-item-info-color')]")?.LastChild;
                        if (colorNode is not null)
                        {
                            var colorText = colorNode.InnerText.Trim();
                            stain = this.englishStainSheet.FirstOrDefault(i => i.Name.RawString.Equals(colorText));
                            if (stain is not null)
                            {
                                if (stain.RowId == 0)
                                {
                                    stain = null;
                                }
                                else if (this.stainSheet is not null)
                                {
                                    // use item from a sheet with the client localisation.
                                    stain = this.stainSheet.GetRow(stain.RowId)!;
                                }
                            }
                        }

                        var rgbaColor = stain is not null ? SeColorToRgba(stain.Color) : 0;
                        equipmentSlots.Add(new EquipmentSlotItem(slot.Value, item.RowId, item.Name.RawString, item.Icon, stain?.RowId, stain?.Name.RawString ?? string.Empty, rgbaColor));
                    }
                }

                this.lastLoadedGlamourId = m.Value;
                this.currentEquipmentSlots = equipmentSlots;
            }
        }
        catch (Exception ex)
        {
            this.dalamudServices.PluginLogger.Error(ex, "Error");
        }
        finally
        {
            this.loading = false;
        }
    }

    private void ExportToClipboard()
    {
        var design = this.CreateDesign();
        var text = design.ToShareText();
        ImGui.SetClipboardText(text);
    }

    private Design CreateDesign()
    {
        var design = new Design
        {
            Name = this.glamourTitle,
            Tags = this.glamourTags,
        };
        foreach (var equipmentSlot in this.currentEquipmentSlots)
        {
            var slot = design.Equipment.GetSlot(equipmentSlot.EquipSlot);
            if (slot is null)
            {
                this.dalamudServices.PluginLogger.Warning("slot {Slot} not found", equipmentSlot.EquipSlot);
                continue;
            }

            slot.ItemId = equipmentSlot.ItemId;
            slot.Apply = true;
            slot.ApplyStain = true;

            if (ReferenceEquals(slot, design.Equipment.MainHand))
            {
                design.Equipment.Weapon.Show = true;
                design.Equipment.Weapon.Apply = true;

                if (design.Equipment.OffHand.ItemId == 0)
                {
                    design.Equipment.OffHand.ItemId = equipmentSlot.ItemId;
                    design.Equipment.OffHand.Apply = true;
                    design.Equipment.OffHand.ApplyStain = true;
                }
            }

            if (ReferenceEquals(slot, design.Equipment.Head))
            {
                design.Equipment.Hat.Show = true;
                design.Equipment.Hat.Apply = true;
            }

            if (equipmentSlot.StrainId > 0)
            {
                slot.Stain = equipmentSlot.StrainId.Value;
                slot.ApplyStain = true;
            }
        }

        return design;
    }

    private EquipSlot? GetSlot(string type, int variant = 0)
        => type switch
        {
            "head" => EquipSlot.Head,
            "body" => EquipSlot.Body,
            "hands" => EquipSlot.Hands,
            "legs" => EquipSlot.Legs,
            "feet" => EquipSlot.Feet,
            "weapon" => EquipSlot.MainHand,
            "offhand" => EquipSlot.OffHand,
            "earrings" => EquipSlot.Ears,
            "necklace" => EquipSlot.Neck,
            "bracelet" => EquipSlot.Wrists,
            "ring" when variant == 0 => EquipSlot.RFinger,
            "ring" => EquipSlot.LFinger,
            _ => null,
        };

    private sealed record EquipmentSlotItem(EquipSlot EquipSlot, uint ItemId, string ItemName, ushort ItemIcon, uint? StrainId, string StainName, uint? RgbaColor);
}