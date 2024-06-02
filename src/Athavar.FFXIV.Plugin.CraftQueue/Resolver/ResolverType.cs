// <copyright file="ResolverType.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue.Resolver;

internal enum ResolverType
{
    /// <summary>
    ///     Static Resolver. Rotation is fixed.
    /// </summary>
    Static,

    /// <summary>
    ///     Dynamic Resolver. Rotation is dynamic.
    /// </summary>
    Dynamic,
}