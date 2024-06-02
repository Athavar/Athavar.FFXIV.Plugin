// <copyright file="ActionSummary.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.ActionSummary;

internal sealed class ActionSummary
{
    public ActionSummary(uint id, bool isStatus = false)
    {
        this.Id = id;
        this.IsStatus = isStatus;
    }

    public bool IsStatus { get; }

    public uint Id { get; }

    public uint Casts { get; private set; }

    public ActionTypeSummary? HealingDone { get; private set; }

    public ActionTypeSummary? HealingTaken { get; private set; }

    public ActionTypeSummary? DamageDone { get; private set; }

    public ActionTypeSummary? DamageTaken { get; private set; }

    public void OnCast() => this.Casts++;

    public void OnHealingDone(ActionEvent actionEvent) => (this.HealingDone ??= new ActionTypeSummary()).AddEvent(actionEvent);

    public void OnHealingTake(ActionEvent actionEvent) => (this.HealingTaken ??= new ActionTypeSummary()).AddEvent(actionEvent);

    public void OnDamageDone(ActionEvent actionEvent) => (this.DamageDone ??= new ActionTypeSummary()).AddEvent(actionEvent);

    public void OnDamageTake(ActionEvent actionEvent) => (this.DamageTaken ??= new ActionTypeSummary()).AddEvent(actionEvent);

    public void Calc()
    {
        this.HealingDone?.Calc();
        this.HealingTaken?.Calc();
        this.DamageDone?.Calc();
        this.DamageTaken?.Calc();
    }
}