// <copyright file="VisibilityConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Models;

public sealed class VisibilityConfig : BaseConfig
{
    [JsonInclude]
    [JsonPropertyName("AlwaysHide")]
    public bool AlwaysHide { get; set; }

    [JsonInclude]
    [JsonPropertyName("HideInCombat")]
    public bool HideInCombat { get; set; }

    [JsonInclude]
    [JsonPropertyName("HideOutsideCombat")]
    public bool HideOutsideCombat { get; set; }

    [JsonInclude]
    [JsonPropertyName("HideOutsideDuty")]
    public bool HideOutsideDuty { get; set; }

    [JsonInclude]
    [JsonPropertyName("HideWhilePerforming")]
    public bool HideWhilePerforming { get; set; }

    [JsonInclude]
    [JsonPropertyName("HideInGoldenSaucer")]
    public bool HideInGoldenSaucer { get; set; }

    [JsonInclude]
    [JsonPropertyName("ShowForJobTypes")]
    public JobType ShowForJobTypes { get; set; } = JobType.All;

    [JsonInclude]
    [JsonPropertyName("CustomJobString")]
    public string CustomJobString { get; set; } = string.Empty;

    [JsonInclude]
    [JsonPropertyName("CustomJobList")]
    public List<Job> CustomJobList { get; set; } = new();
}