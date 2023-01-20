// <copyright file="CraftingJobException.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.CraftQueue;

internal class CraftingJobException : AthavarPluginException
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