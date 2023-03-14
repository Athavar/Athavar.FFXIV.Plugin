// <copyright file="VisibilityConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

public sealed class VisibilityConfig : BaseConfig
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