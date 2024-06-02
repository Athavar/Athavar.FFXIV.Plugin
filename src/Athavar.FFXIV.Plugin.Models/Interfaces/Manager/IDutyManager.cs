// <copyright file="IDutyManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Interfaces.Manager;

using Athavar.FFXIV.Plugin.Models.Duty;

public interface IDutyManager
{
    public delegate void DutyStartedDelegate(DutyStartedEventArgs eventArgs);

    public delegate void DutyWipedDelegate();

    public delegate void DutyRecommencedDelegate();

    public delegate void DutyEndedDelegate(DutyEndedEventArgs eventArgs);

    event DutyStartedDelegate? DutyStarted;

    event DutyWipedDelegate? DutyWiped;

    event DutyRecommencedDelegate? DutyRecommenced;

    event DutyEndedDelegate? DutyEnded;
}