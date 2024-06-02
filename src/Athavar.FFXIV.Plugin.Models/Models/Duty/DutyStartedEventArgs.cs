// <copyright file="DutyStartedEventArgs.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Duty;

public sealed class DutyStartedEventArgs : EventArgs
{
    public required DateTimeOffset StartTime { get; init; }

    public required DutyInfo DutyInfo { get; init; }
}