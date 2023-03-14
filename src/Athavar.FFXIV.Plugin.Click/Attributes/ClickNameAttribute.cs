// <copyright file="ClickNameAttribute.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Attributes;

/// <summary>
///     The callable name of a click.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class ClickNameAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickNameAttribute" /> class.
    /// </summary>
    /// <param name="name">Name of the click.</param>
    public ClickNameAttribute(string name) => this.Name = name;

    /// <summary>
    ///     Gets the name of the click.
    /// </summary>
    public string Name { get; init; }
}