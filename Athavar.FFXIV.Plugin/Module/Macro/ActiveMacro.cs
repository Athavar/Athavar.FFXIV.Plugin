// <copyright file="ActiveMacro.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro;

using System.Linq;
using Athavar.FFXIV.Plugin.Module.Macro.Commands;
using Athavar.FFXIV.Plugin.Module.Macro.Grammar;

/// <summary>
///     Reprecent a active running macro.
/// </summary>
internal class ActiveMacro
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ActiveMacro" /> class.
    /// </summary>
    /// <param name="node">The node containing the macro code.</param>
    public ActiveMacro(MacroNode node)
    {
        this.Node = node;
        this.Steps = MacroParser.Parse(node.Contents).ToArray();
    }

    /// <summary>
    ///     Gets the <see cref="MacroNode" /> containing the macro code.
    /// </summary>
    public MacroNode Node { get; }

    /// <summary>
    ///     Gets all steps of the <see cref="ActiveMacro" />.
    /// </summary>
    public MacroCommand[] Steps { get; }

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
        if (this.StepIndex < 0 || this.StepIndex >= this.Steps.Length)
        {
            return null;
        }

        return this.Steps[this.StepIndex];
    }
}