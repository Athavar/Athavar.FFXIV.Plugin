// <copyright file="ActionEffects.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.ActionEffect;

using System.Collections;

internal unsafe struct ActionEffects : IEnumerable<ActionEffect>
{
    private fixed ulong effects[8];

    public ulong this[int index]
    {
        get => this.effects[index];
        set => this.effects[index] = value;
    }

    public IEnumerator<ActionEffect> GetEnumerator()
    {
        for (var i = 0; i < 8; ++i)
        {
            var eff = this.Build(i);
            if (eff.Type != ActionEffectType.Nothing)
            {
                yield return eff;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    private ActionEffect Build(int index)
    {
        fixed (ulong* p = this.effects)
        {
            return *(ActionEffect*)(p + index);
        }
    }
}