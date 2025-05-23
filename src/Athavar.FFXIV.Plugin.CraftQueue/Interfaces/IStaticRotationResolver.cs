// <copyright file="IStaticRotationResolver.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue.Interfaces;

using Athavar.FFXIV.Plugin.CraftSimulator.Models;

internal interface IStaticRotationResolver : IRotationResolver
{
    public CraftingSkills[] Rotation { get; }
}