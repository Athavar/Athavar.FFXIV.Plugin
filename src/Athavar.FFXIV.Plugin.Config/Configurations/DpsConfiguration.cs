// <copyright file="DpsConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

public sealed class DpsConfiguration : BasicModuleConfig
{
    public List<MeterConfig> Meters { get; set; } = new();

    public PartyType PartyFilter { get; set; } = PartyType.Alliance;

    public int TextRefreshInterval { get; set; } = 200;
}