// <copyright file="ActiveMacro.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro;

using System.Collections.Generic;
using System.Linq;
using Athavar.FFXIV.Plugin.Module.Macro.Grammar;
using Athavar.FFXIV.Plugin.Module.Macro.Grammar.Commands;

/// <summary>
///     Reprecent a active running macro.
/// </summary>
internal class ActiveMacro
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ActiveMacro" /> class.
    /// </summary>
    /// <param name="node">The node containing the macro code.</param>
    public ActiveMacro(MacroNode node, MacroConfiguration configuration)
    {
        this.Node = node;
        this.Steps = MacroParser.Parse(node.Contents).ToList();

        if (node.CraftingLoop)
        {
            var maxwait = configuration.CraftLoopMaxWait;
            var maxwaitModifier = maxwait > 0 ? $" <maxwait.{maxwait}>" : string.Empty;

            var steps = new MacroCommand[]
                        {
                            WaitAddonCommand.Parse($@"/waitaddon ""RecipeNote""{maxwaitModifier}"),
                            ClickCommand.Parse(@"/click ""synthesize"""),
                            WaitAddonCommand.Parse($@"/waitaddon ""Synthesis""{maxwaitModifier}"),
                        };

            if (configuration.CraftLoopFromRecipeNote)
            {
                this.Steps.InsertRange(0, steps);
            }
            else
            {
                // No sense in looping afterwards, if no loops are necessary.
                if (this.Node.CraftLoopCount != 0)
                {
                    this.Steps.AddRange(steps);
                }
            }

            var loops = this.Node.CraftLoopCount;
            if (loops > 0 || loops == -1)
            {
                var loopCount = loops > 0 ? $" {loops}" : string.Empty;

                var echo = configuration.CraftLoopEcho;
                var echoModifier = echo ? " <echo>" : string.Empty;

                var loopStep = LoopCommand.Parse($@"/loop{loopCount}{echoModifier}");
                this.Steps.Add(loopStep);
            }
        }
    }

    /// <summary>
    ///     Gets the <see cref="MacroNode" /> containing the macro code.
    /// </summary>
    public MacroNode Node { get; }

    /// <summary>
    ///     Gets all steps of the <see cref="ActiveMacro" />.
    /// </summary>
    public List<MacroCommand> Steps { get; }

    /// <summary>
    ///     Gets or sets index of current executing step.
    /// </summary>
    public int StepIndex { get; set; }

    /// <summary>
    ///     Increase the step count.
    /// </summary>
    public void NextStep() => this.StepIndex++;

    /// <summary>
    ///     Loop to the start of the macro.
    /// </summary>
    public void Loop() => this.StepIndex = -1;

    /// <summary>
    ///     Gets the current command step.
    /// </summary>
    /// <returns>returns the current command step.</returns>
    public MacroCommand? GetCurrentStep()
    {
        if (this.StepIndex < 0 || this.StepIndex >= this.Steps.Count)
        {
            return null;
        }

        return this.Steps[this.StepIndex];
    }
}