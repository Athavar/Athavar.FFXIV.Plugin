// <copyright file="ClickMaterialize.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;

public sealed class ClickMaterialize : ClickBase<ClickMaterialize>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickMaterialize"/> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickMaterialize(nint addon = default)
        : base("Materialize", addon)
    {
    }

    public static implicit operator ClickMaterialize(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickMaterialize Using(nint addon) => new(addon);

    /// <summary>
    ///     Click item to extract materia.
    /// </summary>
    /// <param name="index">Retainer index.</param>
    public void Item(uint index) => this.FireCallback(2, index);

    [ClickName("materia_extract0")]
    public void Item0() => this.Item(0);
}