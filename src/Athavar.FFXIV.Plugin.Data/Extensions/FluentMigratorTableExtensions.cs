// <copyright file="FluentMigratorTableExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Data.Extensions;

using Athavar.FFXIV.Plugin.Data.Dto;
using FluentMigrator.Builders.Create.Table;

/// <summary>
///     Provides extension methods for FluentMigrator's ICreateTableWithColumnSyntax interface to simplify table creation.
/// </summary>
public static class FluentMigratorTableExtensions
{
    /// <summary>
    ///     Adds an 'id' column configured as a primary key with auto-incrementing values.
    /// </summary>
    /// <param name="tableWithColumnSyntax">The ICreateTableWithColumnSyntax instance.</param>
    /// <returns>The modified ICreateTableColumnOptionOrWithColumnSyntax instance.</returns>
    public static ICreateTableColumnOptionOrWithColumnSyntax WithId(
        this ICreateTableWithColumnSyntax tableWithColumnSyntax)
        => tableWithColumnSyntax.WithColumn(nameof(BaseDto.Id)).AsInt64().NotNullable().PrimaryKey().Identity();

    /// <summary>
    ///     Adds 'created' and 'updated' columns configured as Int64 and not nullable.
    /// </summary>
    /// <param name="tableWithColumnSyntax">The ICreateTableWithColumnSyntax instance.</param>
    /// <returns>The modified ICreateTableWithColumnSyntax instance.</returns>
    public static ICreateTableWithColumnSyntax WithTimeStamp(
        this ICreateTableWithColumnSyntax tableWithColumnSyntax)
        => tableWithColumnSyntax
           .WithColumn(nameof(BaseDto.Created)).AsInt64().NotNullable()
           .WithColumn(nameof(BaseDto.Updated)).AsInt64().NotNullable();
}