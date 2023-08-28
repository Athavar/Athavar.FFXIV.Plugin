// <copyright file="SQLCustomColumnTypes.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Data.Extensions;

using FluentMigrator.Builders.Create.Table;

/// https://github.com/kalilistic/FluentDapperLite/blob/0d629c4bc2fa12c9b829e62172b28e697a847db2/FluentDapperLite/Extension/SQLCustomColumnTypes.cs
/// <summary>
///     Provides custom column type extensions for FluentMigrator's table column creation syntax.
/// </summary>
/// <remarks>
///     This class extends FluentMigrator's ICreateTableColumnAsTypeSyntax to add support for unsigned integer column
///     types.
/// </remarks>
public static class SqlCustomColumnTypes
{
    /// <summary>
    ///     Adds a column of type SMALLINT with a constraint to mimic UInt16.
    /// </summary>
    /// <param name="column">The ICreateTableColumnAsTypeSyntax instance to extend.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>An ICreateTableColumnOptionOrWithColumnSyntax instance with the added constraint.</returns>
    public static ICreateTableColumnOptionOrWithColumnSyntax AsUInt16(this ICreateTableColumnAsTypeSyntax column, string columnName) => column.AsCustom($"SMALLINT CHECK({columnName} >= 0 AND {columnName} <= 65535)");

    /// <summary>
    ///     Adds a column of type INTEGER with a constraint to mimic UInt32.
    /// </summary>
    /// <param name="column">The ICreateTableColumnAsTypeSyntax instance to extend.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>An ICreateTableColumnOptionOrWithColumnSyntax instance with the added constraint.</returns>
    public static ICreateTableColumnOptionOrWithColumnSyntax AsUInt32(this ICreateTableColumnAsTypeSyntax column, string columnName) => column.AsCustom($"INTEGER CHECK({columnName} >= 0 AND {columnName} <= 4294967295)");

    /// <summary>
    ///     Adds a column of type INTEGER with a constraint to mimic UInt64.
    ///     Since SQLite uses dynamic typing, INTEGER can accommodate large integers, but care must be taken with values larger
    ///     than 2^63 - 1.
    /// </summary>
    /// <param name="column">The ICreateTableColumnAsTypeSyntax instance to extend.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <returns>An ICreateTableColumnOptionOrWithColumnSyntax instance with the added constraint.</returns>
    public static ICreateTableColumnOptionOrWithColumnSyntax AsUInt64(this ICreateTableColumnAsTypeSyntax column, string columnName) => column.AsCustom($"INTEGER CHECK({columnName} >= 0)");
}