// <copyright file="ClickBase{TImpl}.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Bases;

using Athavar.FFXIV.Plugin.Click.Exceptions;
using Athavar.FFXIV.Plugin.Click.Structures;
using FFXIVClientStructs.FFXIV.Component.GUI;

/// <summary>
///     Click base class.
/// </summary>
/// <typeparam name="TImpl">The implementing type.</typeparam>
public abstract unsafe class ClickBase<TImpl> : IClickable
    where TImpl : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickBase{TImpl}" /> class.
    /// </summary>
    /// <param name="name">Addon name.</param>
    /// <param name="addon">Addon address.</param>
    public ClickBase(string name, nint addon)
    {
        this.AddonName = name;

        if (addon == default)
        {
            addon = this.GetAddonByName(this.AddonName);
        }

        this.AddonAddress = addon;
        this.UnitBase = (AtkUnitBase*)addon;
    }

    /// <summary>
    ///     Gets a pointer to the underlying AtkUnitBase.
    /// </summary>
    protected AtkUnitBase* UnitBase { get; }

    /// <summary>
    ///     Gets the associated addon name.
    /// </summary>
    protected string AddonName { get; init; }

    /// <summary>
    ///     Gets a pointer to the addon.
    /// </summary>
    protected nint AddonAddress { get; init; }

    public static implicit operator TImpl(ClickBase<TImpl> cb) => (cb as TImpl)!;

    /// <summary>
    ///     Fire an addon callback.
    /// </summary>
    /// <param name="values">AtkValue values.</param>
    /// <returns>Itself.</returns>
    protected TImpl FireCallback(params object[] values)
    {
        var atkValues = new AtkValueArray(values);
        this.UnitBase->FireCallback(atkValues.Length, atkValues);
        atkValues.Dispose();

        return this;
    }

    /// <summary>
    ///     Fire an addon callback.
    /// </summary>
    /// <param name="a4">A parameter.</param>
    /// <returns>Itself.</returns>
    protected TImpl FireNullCallback(ulong a4)
    {
        this.UnitBase->FireCallback(0, null, (void*)a4);

        return this;
    }

    /// <summary>
    ///     Hide the addon.
    /// </summary>
    /// <returns>Itself.</returns>
    protected TImpl HideAddon()
    {
        this.UnitBase->Hide(false, false, 0);

        return this;
    }

    private nint GetAddonByName(string name, int index = 1)
    {
        var atkStage = AtkStage.GetSingleton();
        if (atkStage == null)
        {
            throw new InvalidClickException("AtkStage is not available");
        }

        var unitMgr = atkStage->RaptureAtkUnitManager;
        if (unitMgr == null)
        {
            throw new InvalidClickException("UnitMgr is not available");
        }

        var addon = unitMgr->GetAddonByName(name, index);
        if (addon == null)
        {
            throw new InvalidClickException("Addon is not available");
        }

        return (nint)addon;
    }
}