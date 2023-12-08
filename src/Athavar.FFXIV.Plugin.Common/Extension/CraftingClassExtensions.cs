// <copyright file="CraftingClassExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Extension;

using Athavar.FFXIV.Plugin.CraftSimulator.Models;
using Athavar.FFXIV.Plugin.Models;

public static class CraftingClassExtensions
{
    public static uint GetRowId(this CraftingClass job) => (uint)((int)job + 8);

    public static Job GetJob(this CraftingClass job) => (Job)job.GetRowId();
}