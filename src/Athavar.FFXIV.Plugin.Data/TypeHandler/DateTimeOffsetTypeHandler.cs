// <copyright file="DateTimeOffsetTypeHandler.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Data.TypeHandler;

using System.Data;
using Dapper;

internal sealed class DateTimeOffsetTypeHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
    {
        switch (parameter.DbType)
        {
            case DbType.DateTime:
            case DbType.DateTime2:
            case DbType.AnsiString: // Seems to be some MySQL type mapping here
                parameter.Value = value.UtcDateTime;
                break;
            case DbType.DateTimeOffset:
                parameter.Value = value;
                break;
            default:
                throw new InvalidOperationException("DateTimeOffset must be assigned to a DbType.DateTime SQL field.");
        }
    }

    public override DateTimeOffset Parse(object value)
    {
        switch (value)
        {
            case DateTime time:
                return new DateTimeOffset(DateTime.SpecifyKind(time, DateTimeKind.Utc), TimeSpan.Zero);
            case DateTimeOffset dto:
                return dto;
            default:
                throw new InvalidOperationException("Must be DateTime or DateTimeOffset object to be mapped.");
        }
    }
}