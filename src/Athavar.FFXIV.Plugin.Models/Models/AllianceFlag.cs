// <copyright file="AllianceFlag.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models;

public enum AllianceFlag : byte
{
    /// <summary>
    ///     The is no additional alliance.
    /// </summary>
    NoAlliance = 0x00,

    /// <summary>
    ///     alliance with 2 8-man group.
    /// </summary>
    Alliance = 0x01,

    /// <summary>
    ///     alliance with 5 4-man group.
    /// </summary>
    LightAlliance = 0x02,
}