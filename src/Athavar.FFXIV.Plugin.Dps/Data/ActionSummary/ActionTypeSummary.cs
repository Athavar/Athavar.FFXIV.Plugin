// <copyright file="ActionTypeSummary.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.ActionSummary;

using System.Runtime.InteropServices;

internal class ActionTypeSummary
{
    private readonly List<ActionEvent> events = new();
    private bool change;

    public int Hits => this.events.Count;

    public ulong TotalAmount { get; set; }

    public ulong OverAmount { get; set; }

    public ulong CritHits { get; set; }

    public ulong DirectHits { get; set; }

    public ulong CritDirectHits { get; set; }

    public void AddEvent(ActionEvent actionEvent)
    {
        this.events.Add(actionEvent);
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
        ulong crit = 0;
        ulong direct = 0;
        ulong critDirect = 0;
        foreach (var actionEvent in CollectionsMarshal.AsSpan(this.events))
        {
            sum += actionEvent.Amount;
            overSum += actionEvent.OverAmount;
            switch (actionEvent.Mod)
            {
                case ActionEventModifier.CritHit:
                    crit++;
                    break;
                case ActionEventModifier.DirectHit:
                    direct++;
                    break;
                case ActionEventModifier.CritDirectHit:
                    critDirect++;
                    crit++;
                    direct++;
                    break;
                case ActionEventModifier.Normal:
                default:
                    break;
            }
        }

        this.TotalAmount = sum;
        this.OverAmount = overSum;
        this.CritHits = crit;
        this.DirectHits = direct;
        this.CritDirectHits = critDirect;
    }
}