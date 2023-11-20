// <copyright file="DutyEndedEventArgs.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Duty;

public sealed class DutyEndedEventArgs : EventArgs
{
    public required bool Completed { get; init; }

    public required DateTimeOffset StartTime { get; init; }

    public required DateTimeOffset EndTime { get; init; }

    public required TimeSpan Duration { get; init; }

    public required int Wipes { get; init; }

    public required DutyInfo DutyInfo { get; init; }
}