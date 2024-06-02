// <copyright file="BaseDto.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Data.Dto;

/// <summary>
///     Represents a Data Transfer Object with common identifiers and timestamps.
/// </summary>
public class BaseDto
{
    public long Id { get; set; }

    public long Created { get; set; }

    public long Updated { get; set; }
}