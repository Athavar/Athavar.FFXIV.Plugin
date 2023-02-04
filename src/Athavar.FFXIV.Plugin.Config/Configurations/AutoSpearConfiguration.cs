// <copyright file="AutoSpearConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

public class AutoSpearConfiguration : BasicModuleConfig
{
    public string? FishMatchText { get; set; } = string.Empty;
}