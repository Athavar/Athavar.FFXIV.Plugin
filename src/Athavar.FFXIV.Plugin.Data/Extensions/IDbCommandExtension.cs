// <copyright file="IDbCommandExtension.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Data.Extensions;

using System.Data;

internal static class IDbCommandExtension
{
    public static void ExecutePragmaCommand(this IDbCommand cmd, string pragma)
    {
        cmd.CommandText = $"PRAGMA {pragma};";
        cmd.ExecuteNonQuery();
    }
}