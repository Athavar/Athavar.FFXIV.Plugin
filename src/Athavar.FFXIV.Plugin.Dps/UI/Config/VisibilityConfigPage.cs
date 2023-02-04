// <copyright file="VisibilityConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI.Config;

using System.Numerics;
using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using ImGuiNET;

internal class VisibilityConfigPage : IConfigPage
{
    private readonly MeterConfig config;

    private readonly ICommandInterface ci;

    [JsonIgnore]
    private string customJobInput = string.Empty;

    public VisibilityConfigPage(MeterConfig config, ICommandInterface ci)
    {
        this.config = config;
        this.ci = ci;
    }

    public string Name => "Visibility";

    private VisibilityConfig Config => this.config.VisibilityConfig;

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
            var c = this.Config;
            ImGuiEx.Checkbox("Always Hide", c.AlwaysHide, x => c.AlwaysHide = x);
            ImGuiEx.Checkbox("Hide In Combat", c.HideInCombat, x => c.HideInCombat = x);
            ImGuiEx.Checkbox("Hide Outside Combat", c.HideOutsideCombat, x => c.HideOutsideCombat = x);
            ImGuiEx.Checkbox("Hide Outside Duty", c.HideOutsideDuty, x => c.HideOutsideDuty = x);
            ImGuiEx.Checkbox("Hide While Performing", c.HideWhilePerforming, x => c.HideWhilePerforming = x);
            ImGuiEx.Checkbox("Hide In Golden Saucer", c.HideInGoldenSaucer, x => c.HideInGoldenSaucer = x);

            ImGuiEx.Spacing(1);
            var jobTypeOptions = Enum.GetNames(typeof(JobType));
            ImGuiEx.Combo("Show for Jobs", c.ShowForJobTypes.AsText(), x => c.ShowForJobTypes = Enum.Parse<JobType>(jobTypeOptions[x]), jobTypeOptions);

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
                }
            }
        }

        ImGui.EndChild();
    }
}