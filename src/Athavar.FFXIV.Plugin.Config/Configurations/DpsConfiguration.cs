// <copyright file="DpsConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

public class DpsConfiguration : BasicModuleConfig
{
    public List<MeterConfig> Meters { get; set; } = new();

    public PartyType PartyFilter { get; set; }
}