// <copyright file="ActionEventModifier.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.ActionSummary;

[Flags]
public enum ActionEventModifier : byte
{
    Normal = 0x0,
    CritHit = 0x1,
    DirectHit = 0x2,
    CritDirectHit = CritHit | DirectHit,
}