// <copyright file="BarColorsConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI.Config;

using System.Numerics;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using ImGuiNET;

internal class BarColorsConfigPage : IConfigPage
{
    private readonly MeterWindow window;

    public BarColorsConfigPage(MeterWindow window) => this.window = window;

    public string Name => "Colors";

    private BarColorsConfig Config => this.window.Config.BarColorsConfig;

    public IConfig GetDefault() => new BarColorsConfig();

    public IConfig GetConfig() => this.Config;

    public ConfigColor GetColor(Job job)
        => job switch
           {
               Job.Gladiator => this.Config.GLAColor,
               Job.Marauder => this.Config.MRDColor,
               Job.Paladin => this.Config.PLDColor,
               Job.Warrior => this.Config.WARColor,
               Job.DarkKnight => this.Config.DRKColor,
               Job.Gunbreaker => this.Config.GNBColor,

               Job.Conjurer => this.Config.CNJColor,
               Job.WhiteMage => this.Config.WHMColor,
               Job.Scholar => this.Config.SCHColor,
               Job.Astrologian => this.Config.ASTColor,
               Job.Sage => this.Config.SGEColor,

               Job.Pugilist => this.Config.PGLColor,
               Job.Lancer => this.Config.LNCColor,
               Job.Rogue => this.Config.ROGColor,
               Job.Monk => this.Config.MNKColor,
               Job.Dragoon => this.Config.DRGColor,
               Job.Ninja => this.Config.NINColor,
               Job.Samurai => this.Config.SAMColor,
               Job.Reaper => this.Config.RPRColor,

               Job.Archer => this.Config.ARCColor,
               Job.Bard => this.Config.BRDColor,
               Job.Machinist => this.Config.MCHColor,
               Job.Dancer => this.Config.DNCColor,

               Job.Thaumaturge => this.Config.THMColor,
               Job.Arcanist => this.Config.ACNColor,
               Job.BlackMage => this.Config.BLMColor,
               Job.Summoner => this.Config.SMNColor,
               Job.RedMage => this.Config.RDMColor,
               Job.BlueMage => this.Config.BLUColor,

               _ => this.Config.UKNColor,
           };

    public void DrawConfig(Vector2 size, float padX, float padY)
    {
        var change = false;
        if (ImGui.BeginChild($"##{this.Name}", new Vector2(size.X, size.Y), true))
        {
            if (this.ColorPick("PLD", this.Config.PLDColor.Vector, x => this.Config.PLDColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("WAR", this.Config.WARColor.Vector, x => this.Config.WARColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("DRK", this.Config.DRKColor.Vector, x => this.Config.DRKColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("GNB", this.Config.GNBColor.Vector, x => this.Config.GNBColor.Vector = x))
            {
                change = true;
            }

            ImGui.NewLine();

            if (this.ColorPick("SCH", this.Config.SCHColor.Vector, x => this.Config.SCHColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("WHM", this.Config.WHMColor.Vector, x => this.Config.WHMColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("AST", this.Config.ASTColor.Vector, x => this.Config.ASTColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("SGE", this.Config.SGEColor.Vector, x => this.Config.SGEColor.Vector = x))
            {
                change = true;
            }

            ImGui.NewLine();

            if (this.ColorPick("MNK", this.Config.MNKColor.Vector, x => this.Config.MNKColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("NIN", this.Config.NINColor.Vector, x => this.Config.NINColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("DRG", this.Config.DRGColor.Vector, x => this.Config.DRGColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("SAM", this.Config.SAMColor.Vector, x => this.Config.SAMColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("RPR", this.Config.RPRColor.Vector, x => this.Config.RPRColor.Vector = x))
            {
                change = true;
            }

            ImGui.NewLine();

            if (this.ColorPick("BRD", this.Config.BRDColor.Vector, x => this.Config.BRDColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("MCH", this.Config.MCHColor.Vector, x => this.Config.MCHColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("DNC", this.Config.DNCColor.Vector, x => this.Config.DNCColor.Vector = x))
            {
                change = true;
            }

            ImGui.NewLine();

            if (this.ColorPick("BLM", this.Config.BLMColor.Vector, x => this.Config.BLMColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("SMN", this.Config.SMNColor.Vector, x => this.Config.SMNColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("RDM", this.Config.RDMColor.Vector, x => this.Config.RDMColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("BLU", this.Config.BLUColor.Vector, x => this.Config.BLUColor.Vector = x))
            {
                change = true;
            }

            ImGui.NewLine();

            if (this.ColorPick("GLA", this.Config.GLAColor.Vector, x => this.Config.GLAColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("MRD", this.Config.MRDColor.Vector, x => this.Config.MRDColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("CNJ", this.Config.CNJColor.Vector, x => this.Config.CNJColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("PGL", this.Config.PGLColor.Vector, x => this.Config.PGLColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("ROG", this.Config.ROGColor.Vector, x => this.Config.ROGColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("LNC", this.Config.LNCColor.Vector, x => this.Config.LNCColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("ARC", this.Config.ARCColor.Vector, x => this.Config.ARCColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("THM", this.Config.THMColor.Vector, x => this.Config.THMColor.Vector = x))
            {
                change = true;
            }

            if (this.ColorPick("ACN", this.Config.ACNColor.Vector, x => this.Config.ACNColor.Vector = x))
            {
                change = true;
            }

            ImGui.NewLine();
            if (this.ColorPick("Other", this.Config.UKNColor.Vector, x => this.Config.UKNColor.Vector = x))
            {
                change = true;
            }

            // Save if changed
            if (change)
            {
                this.window.Save();
            }

            ImGui.EndChild();
        }
    }

    private bool ColorPick(string label, Vector4 current, Action<Vector4> setter) => ImGuiEx.ColorEdit4(label, current, setter, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
}