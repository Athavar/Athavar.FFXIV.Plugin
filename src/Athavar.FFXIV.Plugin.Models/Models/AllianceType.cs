// <copyright file="AllianceType.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models;

[Flags]
public enum AllianceType : byte
{
    None = 0,
    ThreeParty = 1,
    SixParty = 3,
}