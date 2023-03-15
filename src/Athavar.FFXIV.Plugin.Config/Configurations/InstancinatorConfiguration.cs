// <copyright file="InstancinatorConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

/// <summary>
///     Instancinator Module configuration.
/// </summary>
public sealed class InstancinatorConfiguration : BasicModuleConfig
{
    public string KeyCode { get; set; } = "NumPad0";

    public int ExtraDelay { get; set; } = 0;
}