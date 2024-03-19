// <copyright file="BaseRepository.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Data.Repositories;

using System.Data;
using Athavar.FFXIV.Plugin.Data.Dto;

public abstract class BaseRepository(IDbConnection connection)
{
    protected IDbConnection Connection { get; } = connection;

    /// <summary>
    ///     Sets the creation and last-updated timestamps for the provided DTO.
    /// </summary>
    /// <param name="dto">Data Transfer Object.</param>
    protected static void SetCreateTimestamp(BaseDto dto)
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        dto.Created = currentTime;
        dto.Updated = currentTime;
    }

    /// <summary>
    ///     Sets the last-updated timestamp for the provided DTO.
    /// </summary>
    /// <param name="dto">Data Transfer Object.</param>
    protected static void SetUpdateTimestamp(BaseDto dto) => dto.Updated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}