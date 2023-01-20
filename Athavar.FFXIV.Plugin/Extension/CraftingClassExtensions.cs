// <copyright file="JobExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Extension;

using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;
using Athavar.FFXIV.Plugin.Utils.Constants;

internal static class CraftingClassExtensions
{
    public static uint GetRowId(this CraftingClass job) => (uint)((int)job + 8);

    public static Job GetJob(this CraftingClass job) => (Job)job.GetRowId();
}