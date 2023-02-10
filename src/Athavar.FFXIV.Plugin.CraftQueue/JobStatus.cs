// <copyright file="JobStatus.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue;

internal enum JobStatus
{
    Queued = 0,
    Running,
    Paused,
    Success,
    Failure,
    Canceled,
}