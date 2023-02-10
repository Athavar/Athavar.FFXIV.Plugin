// <copyright file="MeterConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

public class MeterConfig : BaseConfig
{
    public string Name { get; set; } = string.Empty;

    public GeneralConfig GeneralConfig { get; set; } = new();

    public HeaderConfig HeaderConfig { get; set; } = new();

    public BarConfig BarConfig { get; set; } = new();

    public BarColorsConfig BarColorsConfig { get; set; } = new();

    public VisibilityConfig VisibilityConfig { get; set; } = new();
}