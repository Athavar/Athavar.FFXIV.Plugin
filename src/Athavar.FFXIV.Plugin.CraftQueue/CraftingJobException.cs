// <copyright file="CraftingJobException.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue;

using Athavar.FFXIV.Plugin.Common.Exceptions;

internal sealed class CraftingJobException : AthavarPluginException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CraftingJobException" /> class.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public CraftingJobException(string message)
        : base(message)
    {
    }
}