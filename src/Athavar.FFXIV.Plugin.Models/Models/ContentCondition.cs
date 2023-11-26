// <copyright file="ContentCondition.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models;

[Flags]
public enum ContentCondition
{
    None = 0x0,
    JoinInProgress = 0x1,
    UnrestrictedParty = 0x2,
    MinimalIL = 0x4,
    LevelSync = 0x8,
    SilenceEcho = 0x10,
    ExplorerMode = 0x20,
}