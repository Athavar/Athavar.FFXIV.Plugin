// <copyright file="ActionTypeSummary.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.ActionSummary;

using System.Runtime.InteropServices;

internal class ActionTypeSummary
{
    private readonly List<ActionEvent> Events = new();
    private bool change;

    public int Hits => this.Events.Count;

    public ulong TotalAmount { get; set; }

    public ulong OverAmount { get; set; }

    public void AddEvent(ActionEvent actionEvent)
    {
        this.Events.Add(actionEvent);
        this.change = true;
    }

    public virtual void Calc()
    {
        if (!this.change)
        {
            return;
        }

        this.change = false;

        ulong sum = 0;
        ulong overSum = 0;
        foreach (var actionEvent in CollectionsMarshal.AsSpan(this.Events))
        {
            sum += actionEvent.Amount;
            overSum += actionEvent.OverAmount;
        }

        this.TotalAmount = sum;
        this.OverAmount = overSum;
    }
}