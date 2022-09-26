// <copyright file="InstancinatorConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

/// <summary>
///     Instancinator Module configuration.
/// </summary>
internal class InstancinatorConfiguration : BasicModuleConfig
{
    public string KeyCode = Native.KeyCode.NumPad0.ToString();

    public int ExtraDelay = 0;
}