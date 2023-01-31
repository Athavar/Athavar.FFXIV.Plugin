// <copyright file="Reflect.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Quality;

internal class Reflect : QualityAction
{
    private static readonly uint[] IdsValue = { 100387, 100388, 100389, 100390, 100391, 100392, 100393, 100394 };

    /// <inheritdoc />
    public override int Level => 69;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 6;

    /// <inheritdoc />
    public override void Execute(Simulation simulation)
    {
        base.Execute(simulation);
        simulation.AddInnerQuietStacks(1);
    }

    /// <inheritdoc />
    public override bool SkipOnFail() => true;

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation) => simulation.Steps.All(s => s.Skill.Action.IsSkipsBuffTicks());

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}