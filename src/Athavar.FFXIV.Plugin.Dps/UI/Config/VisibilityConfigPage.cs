// <copyright file="VisibilityConfigPage.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI.Config;

using System.Numerics;
using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Models;
using ImGuiNET;

internal sealed class VisibilityConfigPage : IConfigPage
{
    private readonly MeterWindow window;

    private readonly ICommandInterface ci;

    [JsonIgnore]
    private string customJobInput = string.Empty;

    public VisibilityConfigPage(MeterWindow window, ICommandInterface ci)
    {
        this.window = window;
        this.ci = ci;
    }

    public string Name => "Visibility";

    private VisibilityConfig Config => this.window.Config.VisibilityConfig;

    public IConfig GetDefault() => new VisibilityConfig();

    public IConfig GetConfig() => this.Config;

    public bool IsVisible()
    {
        var c = this.Config;
        if (c.AlwaysHide)
        {
            return false;
        }

        if (c.HideInCombat && this.ci.IsInCombat())
        {
            return false;
        }

        if (c.HideOutsideCombat && !this.ci.IsInCombat())
        {
            return false;
        }

        if (c.HideOutsideDuty && !this.ci.IsInDuty())
        {
            return false;
        }

        if (c.HideWhilePerforming && this.ci.IsPerforming())
        {
            return false;
        }

        if (c.HideInGoldenSaucer && this.ci.IsInGoldenSaucer())
        {
            return false;
        }

        return Utils.IsJobType((Job)this.ci.GetCurrentJob(), c.ShowForJobTypes, c.CustomJobList);
    }

    public void DrawConfig(Vector2 size, float padX, float padY)
    {
        if (ImGui.BeginChild($"##{this.Name}", new Vector2(size.X, size.Y), true))
        {
            var change = false;
            var c = this.Config;
            if (ImGuiEx.Checkbox("Always Hide", c.AlwaysHide, x => c.AlwaysHide = x))
            {
                change = true;
            }

            if (ImGuiEx.Checkbox("Hide In Combat", c.HideInCombat, x => c.HideInCombat = x))
            {
                change = true;
            }

            if (ImGuiEx.Checkbox("Hide Outside Combat", c.HideOutsideCombat, x => c.HideOutsideCombat = x))
            {
                change = true;
            }

            if (ImGuiEx.Checkbox("Hide Outside Duty", c.HideOutsideDuty, x => c.HideOutsideDuty = x))
            {
                change = true;
            }

            if (ImGuiEx.Checkbox("Hide While Performing", c.HideWhilePerforming, x => c.HideWhilePerforming = x))
            {
                change = true;
            }

            if (ImGuiEx.Checkbox("Hide In Golden Saucer", c.HideInGoldenSaucer, x => c.HideInGoldenSaucer = x))
            {
                change = true;
            }

            ImGuiEx.Spacing(1);
            var jobTypeOptions = Enum.GetNames(typeof(JobType));
            if (ImGuiEx.Combo("Show for Jobs", c.ShowForJobTypes.AsText(), x => c.ShowForJobTypes = Enum.Parse<JobType>(jobTypeOptions[x]), jobTypeOptions))
            {
                change = true;
            }

            if (c.ShowForJobTypes == JobType.Custom)
            {
                if (string.IsNullOrEmpty(this.customJobInput))
                {
                    this.customJobInput = c.CustomJobString.ToUpper();
                }

                if (ImGui.InputTextWithHint("Custom Job List", "Comma Separated List (ex: WAR, SAM, BLM)", ref this.customJobInput, 100, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    var jobStrings = this.customJobInput.Split(',').Select(j => j.Trim());
                    List<Job> jobList = new();
                    foreach (var j in jobStrings)
                    {
                        if (Enum.TryParse(j, true, out Job parsed))
                        {
                            jobList.Add(parsed);
                        }
                        else
                        {
                            jobList.Clear();
                            this.customJobInput = string.Empty;
                            break;
                        }
                    }

                    this.customJobInput = this.customJobInput.ToUpper();
                    c.CustomJobString = this.customJobInput;
                    c.CustomJobList = jobList;
                    change = true;
                }
            }

            // Save if changed
            if (change)
            {
                this.window.Save();
            }

            ImGui.EndChild();
        }
    }
}