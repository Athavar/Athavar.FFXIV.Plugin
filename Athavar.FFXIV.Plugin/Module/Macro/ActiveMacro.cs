// <copyright file="ActiveMacro.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro
{
    using System;
    using System.Linq;

    /// <summary>
    /// Reprecent a active running macro.
    /// </summary>
    internal class ActiveMacro
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveMacro"/> class.
        /// </summary>
        /// <param name="node">The node containing the macro code.</param>
        public ActiveMacro(MacroNode node)
            : this(node, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveMacro"/> class.
        /// </summary>
        /// <param name="node">The node containing the macro code.</param>
        /// <param name="parent">The parent macro.</param>
        public ActiveMacro(MacroNode node, ActiveMacro? parent)
        {
            this.Node = node;
            this.Parent = parent;
            this.Steps = node.Contents.Split(new[] { "\n", "\r", "\n\r" }, StringSplitOptions.RemoveEmptyEntries).Where(line => !line.StartsWith("#")).ToArray();
        }

        /// <summary>
        /// Gets the <see cref="MacroNode"/> containing the macro code.
        /// </summary>
        public MacroNode Node { get; private set; }

        /// <summary>
        /// Gets the parent <see cref="MacroNode"/>.
        /// </summary>
        public ActiveMacro? Parent { get; private set; }

        /// <summary>
        /// Gets all steps of the <see cref="ActiveMacro"/>.
        /// </summary>
        public string[] Steps { get; private set; }

        /// <summary>
        /// Gets or sets index of current executing step.
        /// </summary>
        public int StepIndex { get; set; }

        public int LoopCount { get; set; }

        public string? GetCurrentStep()
        {
            if (this.StepIndex >= this.Steps.Length)
            {
                return null;
            }

            return this.Steps[this.StepIndex];
        }
    }
}
