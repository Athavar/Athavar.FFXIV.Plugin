// <copyright file="Observe.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Other;

internal class Observe : CraftingAction
{
    private static readonly uint[] IdsValue = { 100010, 100023, 100040, 100082, 100053, 100070, 100099, 100113 };

    /// <inheritdoc />
    public override ActionType ActionType => ActionType.Other;

    /// <inheritdoc />
    public override int Level => 13;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetDurabilityCost(Simulation simulation) => 0;

    /// <inheritdoc />
    public override void Execute(Simulation simulation)
    {
    }

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 7;

    /// <inheritdoc />
    public override bool SkipOnFail() => true;

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation) => true;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;
}