// <copyright file="Exceptions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro
{
    using System;

    /// <summary>
    /// Error thrown when an effect is not present.
    /// </summary>
    internal class EffectNotPresentError : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EffectNotPresentError"/> class.
        /// </summary>
        /// <param name="message">Message to show.</param>
        public EffectNotPresentError(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Error thrown when an condition is not present.
    /// </summary>
    internal class ConditionNotFulfilledError : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionNotFulfilledError"/> class.
        /// </summary>
        /// <param name="message">Message to show.</param>
        public ConditionNotFulfilledError(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Error thrown when a timeout occurs.
    /// </summary>
    internal class EventFrameworkTimeoutError : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventFrameworkTimeoutError"/> class.
        /// </summary>
        /// <param name="message">Message to show.</param>
        public EventFrameworkTimeoutError(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Error thrown when an invalid macro operation occurs.
    /// </summary>
    internal class InvalidMacroOperationException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMacroOperationException"/> class.
        /// </summary>
        /// <param name="message">Message to show.</param>
        public InvalidMacroOperationException(string message)
            : base(message)
        {
        }
    }
}
