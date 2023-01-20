// <copyright file="JobStatus.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.CraftQueue;

internal enum JobStatus
{
    Queued = 0,
    Running,
    Paused,
    Success,
    Failure,
    Canceled,
}