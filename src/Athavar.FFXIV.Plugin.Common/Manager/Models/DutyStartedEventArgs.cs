// <copyright file="DutyStartedEventArgs.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager.Models;

public sealed class DutyStartedEventArgs : EventArgs
{
    public DateTimeOffset StartTime { get; init; }

    public DutyInfo DutyInfo { get; init; }
}