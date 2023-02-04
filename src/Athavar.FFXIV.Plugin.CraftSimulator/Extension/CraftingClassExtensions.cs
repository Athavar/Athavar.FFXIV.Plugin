// <copyright file="CraftingClassExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Extension;

using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;

public static class CraftingClassExtensions
{
    public static uint GetRowId(this CraftingClass job) => (uint)((int)job + 8);

    public static Job GetJob(this CraftingClass job) => (Job)job.GetRowId();
}