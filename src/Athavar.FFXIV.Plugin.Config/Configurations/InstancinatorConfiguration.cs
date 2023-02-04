// <copyright file="InstancinatorConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

/// <summary>
///     Instancinator Module configuration.
/// </summary>
public class InstancinatorConfiguration : BasicModuleConfig
{
    public string KeyCode { get; set; } = "NumPad0";

    public int ExtraDelay { get; set; } = 0;
}