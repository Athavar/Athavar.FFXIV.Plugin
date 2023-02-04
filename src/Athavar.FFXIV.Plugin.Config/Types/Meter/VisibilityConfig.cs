// <copyright file="VisibilityConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

public class VisibilityConfig : BaseConfig
{
    public bool AlwaysHide { get; set; }

    public bool HideInCombat { get; set; }

    public bool HideOutsideCombat { get; set; }

    public bool HideOutsideDuty { get; set; }

    public bool HideWhilePerforming { get; set; }

    public bool HideInGoldenSaucer { get; set; }

    public JobType ShowForJobTypes { get; set; } = JobType.All;

    public string CustomJobString { get; set; } = string.Empty;

    public List<Job> CustomJobList { get; set; } = new();
}