// <copyright file="ActionResultFlags.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.CraftingData;

/// <summary>
///     Event action result types.
/// </summary>
[Flags]
internal enum ActionResultFlags : ushort
{
    // Unk0 = 1 << 0,
    // NotStep1 = 1 << 1,
    // CraftingSuccess = 1 << 2,
    // CraftingFailure = 1 << 3,
    // ActionSuccess = 1 << 4,
}