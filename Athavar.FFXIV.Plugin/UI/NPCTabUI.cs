using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using Glamourer;
using Glamourer.Customization;
using ImGuiNET;
using Lumina;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Penumbra.GameData.Enums;
using Penumbra.GameData.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Athavar.FFXIV.Plugin.UI
{
    public class NPCTabUI
    {
        public const float SelectorWidth = 300;

        private string npcFilter = string.Empty;
        private string CurrentSearch = string.Empty;
        private Dictionary<uint, string> Selection = new();

        private uint CurrentNpc = 0;
        private string CurrentNpcName = string.Empty;
        private ENpcBase? eNpcBase;
        private CharacterCustomization character;
        private CharacterEquipment equipment;
        private string glamourerText = string.Empty;

        string debug;

        private ICustomizationManager customizationManager;

        public NPCTabUI()
        {
            UpdateList();
            customizationManager = CustomizationManager.Create(Dalamud.PluginInterface, Dalamud.GameData, Dalamud.ClientState.ClientLanguage);
            equipment = new CharacterEquipment();
        }

        public void Draw()
        {
            using var raii = new ImGuiRaii();
            if (!raii.Begin(() => ImGui.BeginTabItem("NPC"), ImGui.EndTabItem))
                return;

            DrawSelection();
            ImGui.SameLine();
            DrawPanel();
        }

        private void DrawSelection()
        {
            if (!ImGui.BeginChild("##npcSelector",
                new Vector2(SelectorWidth * ImGui.GetIO().FontGlobalScale, -ImGui.GetFrameHeight() - 1), true))
                return;


            ImGui.Spacing();
            if (ImGui.InputText("Search", ref npcFilter, 64))
            {
                if (!npcFilter.Equals(CurrentSearch, StringComparison.OrdinalIgnoreCase))
                {
                    CurrentSearch = npcFilter;
                    UpdateList();
                }
            }

            foreach(var npc in Selection)
            {
                if (ImGui.Selectable(npc.Value, CurrentNpc == npc.Key))
                {
                    CurrentNpc = npc.Key;
                    CurrentNpcName = npc.Value;
                    SelectNpc(npc.Key);
                }
            }

            using (var _ = new ImGuiRaii().PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero))
            {
                ImGui.EndChild();
            }
        }


        private void DrawPanel()
        {
            ImGui.BeginGroup();

            if (!ImGui.BeginChild("##npcData", -Vector2.One, true))
                return;

            if (eNpcBase is not null)
            {
                var gender = eNpcBase.Gender == 0;
                var race = eNpcBase.Race.Value;
                var tribe = eNpcBase.Tribe.Value;
                var raceText = (race is null ? "Unknown" : gender ? race.Masculine.RawString : race.Feminine.RawString);

                if (ImGui.Button("copy"))
                {
                    if (!string.IsNullOrEmpty(raceText))
                    {
                        var text = ToGlamorerText();
                        ImGui.SetClipboardText(text);
                    }
                }

                ImGui.Text($"Debug: {debug}");

                ImGui.Text($"Index: {CurrentNpc}");
                ImGui.Text($"Name: {CurrentNpcName}");
                ImGui.Text($"Race: {(character.Race == Penumbra.GameData.Enums.Race.Unknown ? "Unknown" :character.Race.ToName())}");
                ImGui.Text($"Gender: {character.Gender.ToName()}");
                ImGui.Text($"BodyType: {character.BodyType}");
                ImGui.Text($"Height: {character.Height}");
                ImGui.Text($"Clan: {(character.Race == Penumbra.GameData.Enums.Race.Unknown ? "Unknown" : character.Clan.ToName())}");
                ImGui.Text($"SkinColor: {character.SkinColor}");
                ImGui.Text($"Face: {character.Face}");
                ImGui.Text($"HairStyle: {character.Hairstyle}");
                ImGui.Text($"HairColor: {eNpcBase.HairColor}");
                ImGui.Text($"HighlightsOn: {(character.HighlightsOn ? "true" : "false")}");
                ImGui.Text($"HighlightsColor: {character.HighlightsColor}");
                ImGui.Text($"EyeColorRight: {character.EyeColorRight}");
                ImGui.Text($"EyeColorLeft: {character.EyeColorLeft}");
                ImGui.Text($"FacialFeature: {character.FacialFeatures}");
                ImGui.Text($"FacialFeatureColor: {character.TattooColor}");
                ImGui.Text($"Eyebrow: {character.Eyebrow}");
                ImGui.Text($"EyeSharp: {character.EyeShape}");
                ImGui.Text($"Nose: {character.Nose}");
                ImGui.Text($"Jaw: {character.Jaw}");
                ImGui.Text($"Mouth: {character.Mouth}");
                ImGui.Text($"LipColor: {character.LipColor}");
                ImGui.Text($"BustSize: {character.BustSize}");
                ImGui.Text($"MuscleToneOrTailEarLength : {character.MuscleMass}");
                ImGui.Text($"TailEarShape: {character.TailShape}");
                ImGui.Text($"FacePaint: {character.FacePaint}");
                ImGui.Text($"FacePaintColor: {character.FacePaintColor}");
            }
            
            ImGui.EndChild();
            ImGui.EndGroup();
        }

        public void UpdateList()
        {
            var npcNames = Dalamud.GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.ENpcResident>(Dalamud.ClientState.ClientLanguage);
            var maps = Dalamud.GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.Map>(Dalamud.ClientState.ClientLanguage);

            if (npcNames is null || maps is null)
            {
                return;
            }

            var tmp = npcNames.Where(n => !string.IsNullOrEmpty(n.Singular.RawString));

            if (!string.IsNullOrEmpty(CurrentSearch))
            {
                tmp = tmp.Where(s => s.Singular.RawString.Contains(this.CurrentSearch));
            }

            Selection = tmp.ToDictionary(e => e.RowId, e => $"{e.Singular.RawString} {(!string.IsNullOrEmpty(e.Title.RawString) ?"(" + e.Title + ")" : "" )} [{e.RowId}]");
        }

        public void SelectNpc(uint row)
        {
            var npcs = Dalamud.GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.ENpcBase>(Dalamud.ClientState.ClientLanguage);
            if (npcs is null)
            {
                return;
            }

            this.eNpcBase = npcs.GetRow(row);
            var data = this.eNpcBase;
            if (data is null)
            {
                return;
            }

            character.Race = (Penumbra.GameData.Enums.Race)data.Race.Row;
            character.Gender = data.Gender == 0 ? Penumbra.GameData.Enums.Gender.Male : Penumbra.GameData.Enums.Gender.Female;
            character.BodyType = data.BodyType;
            character.Height = data.Height;
            character.Clan = (Penumbra.GameData.Enums.SubRace)data.Tribe.Row;

            character.Face = (byte)(((uint)data.Face) % 100);
            character.Hairstyle = data.HairStyle;
            character.HairColor = data.HairColor;
            character.HighlightsOn = data.HairHighlight != 1;
            character.SkinColor = data.SkinColor;
            character.EyeColorRight = data.EyeColor;
            character.HighlightsColor = data.HairHighlightColor;
            character.FacialFeatures = data.FacialFeature;
            character.TattooColor = data.FacialFeatureColor;
            character.Eyebrow = data.Eyebrows;
            character.EyeColorLeft = data.EyeHeterochromia;
            character.Nose = data.Nose;
            character.Jaw = data.Jaw;
            character.Mouth = data.Mouth;
            character.LipColor = data.LipColor;
            character.TailShape = data.ExtraFeature1;

            var isMale = character.Gender == Penumbra.GameData.Enums.Gender.MaleNpc;
            // Bust & Muscles - flex fields.
            if (character.Race == Penumbra.GameData.Enums.Race.Roegadyn || character.Race == Penumbra.GameData.Enums.Race.Hyur)
            {
                // Roegadyn & Hyur
                character.MuscleMass = data.BustOrTone1;
                if (!isMale)
                    character.BustSize = data.ExtraFeature2OrBust;
            }
            else if (character.Race == Penumbra.GameData.Enums.Race.AuRa && isMale)
            {
                // Au Ra male muscles
                character.MuscleMass = data.BustOrTone1;
            }
            else if (!isMale)
            {
                // Other female bust sizes
                character.BustSize = data.BustOrTone1;
            }
            if (character.Race == Penumbra.GameData.Enums.Race.AuRa || character.Race == Penumbra.GameData.Enums.Race.Hyur)

                character.FacePaint = data.FacePaint;
            character.FacePaintColor = data.FacePaintColor;


            // fixes
            if (character.Race > Penumbra.GameData.Enums.Race.Viera)
            {
                character.Race = Penumbra.GameData.Enums.Race.Unknown;
            }

            // custom face fix for crashe in glamourer
            if (character.Face > 7)
            {
                character.Face = 1;
            }

            if (character.Race != Penumbra.GameData.Enums.Race.Unknown)
            {
                var set = customizationManager.GetList(character.Clan, character.Gender);
                int i = 0;
                while (i <= 100)
                {
                    var hair = set.HairColors.Where(s => s.Value == character.Hairstyle + i).FirstOrDefault();
                    if (hair.Id == CustomizationId.Hairstyle)
                    {
                        break;
                    }
                    i++;
                }

                // debug = string.Join(Environment.NewLine, set.HairColors.Select(e => $"{e.Id}:{e.CustomizeId}:{e.Value}:{e.Color}")); 
            }

            equipment.Clear();

            var npcEquip = data.NpcEquip.Value;

            if (npcEquip is null)
            {
                return;
            }

            unsafe CharacterWeapon CreateWeapon(ulong weapon, LazyRow<Lumina.Excel.GeneratedSheets.Stain> dye)
            {
                CharacterWeapon characterWeapon = new CharacterWeapon();
                IntPtr pointer = Marshal.AllocHGlobal(sizeof(CharacterWeapon));
                try
                {
                    Marshal.StructureToPtr<CharacterWeapon>(characterWeapon, pointer, true);

                    Marshal.Copy(BitConverter.GetBytes(weapon), 0, pointer, 6);
                    Marshal.Copy(BitConverter.GetBytes(dye.Value?.RowId ?? 0), 0, pointer + 6, 1);

                    return Marshal.PtrToStructure<CharacterWeapon>(pointer);
                }
                finally
                {
                    Marshal.FreeHGlobal(pointer);
                }
            }

            unsafe CharacterArmor CreateArmor( uint armor, LazyRow<Lumina.Excel.GeneratedSheets.Stain> dye)
            {
                CharacterArmor characterArmor = new CharacterArmor();
                IntPtr pointer = Marshal.AllocHGlobal(sizeof(CharacterArmor));
                try
                {
                    Marshal.StructureToPtr<CharacterArmor>(characterArmor, pointer, true);

                    Marshal.Copy(BitConverter.GetBytes(armor), 0, pointer, 3);
                    Marshal.Copy(BitConverter.GetBytes(dye.Value?.RowId ?? 0), 0, pointer + 3, 1);

                    return Marshal.PtrToStructure<CharacterArmor>(pointer);
                }
                finally
                {
                    Marshal.FreeHGlobal(pointer);
                }
            }

            equipment.MainHand = CreateWeapon(npcEquip.ModelMainHand, npcEquip.DyeMainHand);
            equipment.OffHand = CreateWeapon(npcEquip.ModelOffHand, npcEquip.DyeOffHand);
            equipment.Head = CreateArmor(npcEquip.ModelHead, npcEquip.DyeHead);
            equipment.Body = CreateArmor(npcEquip.ModelBody, npcEquip.DyeBody);
            equipment.Legs = CreateArmor(npcEquip.ModelLegs, npcEquip.DyeLegs);
            equipment.Feet = CreateArmor(npcEquip.ModelFeet, npcEquip.DyeFeet);
            equipment.Ears = CreateArmor(npcEquip.ModelEars, npcEquip.DyeEars);
            equipment.Neck = CreateArmor(npcEquip.ModelNeck, npcEquip.DyeNeck);
            equipment.Wrists = CreateArmor(npcEquip.ModelWrists, npcEquip.DyeWrists);
            equipment.RFinger = CreateArmor(npcEquip.ModelRightRing, npcEquip.DyeRightRing);
            equipment.LFinger = CreateArmor(npcEquip.ModelLeftRing, npcEquip.DyeLeftRing);
        }

        private string ToGlamorerText()
        {
            CharacterSave save = new CharacterSave();
            save.Load(character);
            save.Load(equipment);
            return save.ToBase64();
        }
    }



}
