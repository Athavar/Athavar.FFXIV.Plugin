// <copyright file="TerritoryTypeExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Extension;

using Athavar.FFXIV.Plugin.Models.Duty;
using Lumina.Excel.GeneratedSheets;

public static class TerritoryTypeExtensions
{
    public static TerritoryIntendedUse GetTerritoryIntendedUse(this TerritoryType territoryType) => (TerritoryIntendedUse)territoryType.TerritoryIntendedUse;
}