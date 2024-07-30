// <copyright file="EventData.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Structures;

using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

/// <summary>
///     Event data.
/// </summary>
public sealed unsafe class EventData : SharedBuffer
{
    public static AtkEvent ForNormalTarget(AtkComponentNode* target, PopupMenu* listener) => ForNormalTarget((AtkEventTarget*)target, (AtkEventListener*)listener);

    public static AtkEvent ForNormalTarget(AtkStage* target, AtkUnitBase* unitBase) => ForNormalTarget((AtkEventTarget*)target, (AtkEventListener*)unitBase);

    public static AtkEvent ForNormalTarget(AtkComponentNode* target, AtkUnitBase* unitBase) => ForNormalTarget((AtkEventTarget*)target, (AtkEventListener*)unitBase);

    public static AtkEvent ForNormalTarget(AtkComponentNode* target, AtkComponentNode* unitBase) => ForNormalTarget((AtkEventTarget*)target, (AtkEventListener*)unitBase);

    /// <summary>
    ///     Initializes a new instance of the <see cref="EventData"/> class.
    /// </summary>
    /// <param name="target">Target.</param>
    /// <param name="listener">Event listener.</param>
    /// <returns>Event data.</returns>
    public static AtkEvent ForNormalTarget(AtkEventTarget* target, AtkEventListener* listener)
        => new()
        {
            Listener = listener,
            Target = target,
        };

    /// <summary>
    ///     Initializes a new instance of the <see cref="EventData"/> class.
    /// </summary>
    /// <param name="target">Target.</param>
    /// <param name="listener">Event listener.</param>
    /// <returns>Event data.</returns>
    public static AtkEvent ForNormalRightTarget(AtkEventTarget* target, AtkEventListener* listener)
    {
        var data = ForNormalTarget(target, listener);
        /*
        data.Data[5] = (byte*)0x184003;
        data.Data[7] = target;*/
        return data;
    }
}