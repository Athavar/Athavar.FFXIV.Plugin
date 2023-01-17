// <copyright file="FinalAppraisal.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Buff;

internal class FinalAppraisal : BuffAction
{
    private static readonly int[] IdsValue = { 19012, 19013, 19014, 19015, 19016, 19017, 19018, 19019 };

    /// <inheritdoc />
    public override ActionType ActionType => ActionType.Buff;

    /// <inheritdoc />
    public override int Level => 42;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override int[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 1;

    /// <inheritdoc />
    public override int GetDuration(Simulation simulation) => 5;

    /// <inheritdoc />
    public override Buffs GetBuff() => Buffs.FINAL_APPRAISAL;

    /// <inheritdoc />
    public override int GetInitialStacks() => 0;

    /// <inheritdoc />
    protected override OnTick? GetOnTick() => null;

    /// <inheritdoc />
    protected override bool CanBeClipped() => true;
}