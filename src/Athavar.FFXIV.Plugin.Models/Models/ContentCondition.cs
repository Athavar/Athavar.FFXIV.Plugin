// <copyright file="ContentCondition.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models;

[Flags]
public enum ContentCondition
{
    JoinInProgress = 0x01,
    UnrestrictedParty = 0x02,
    MinimalIL = 0x04,
    LevelSync = 0x08,
    SilenceEcho = 0x10,
    ExplorerMode = 0x20,
    LimitedLevelingRoulette = 0x40,

    /// <summary>
    ///     Selection only. No valid save-able value.
    /// </summary>
    SelectNone = 0x8000000,
}