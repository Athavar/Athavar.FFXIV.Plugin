// <copyright file="EventData.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Structures;

/// <summary>
///     Event data.
/// </summary>
public sealed unsafe class EventData : SharedBuffer
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EventData"/> class.
    /// </summary>
    private EventData()
    {
        const int size = 0x18;
        this.Data = (void**)Buffer.Add(new byte[size]);
        /*
        var d = stackalloc byte[size];
        this.Data = (void**)Buffer.Add(d, size);
        */

        if (this.Data == null)
        {
            throw new ArgumentNullException(null, "EventData could not be created, null");
        }

        this.Data[0] = null;
        this.Data[1] = null;
        this.Data[2] = null;
    }

    /// <summary>
    ///     Gets the data pointer.
    /// </summary>
    public void** Data { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EventData"/> class.
    /// </summary>
    /// <param name="target">Target.</param>
    /// <param name="listener">Event listener.</param>
    /// <returns>Event data.</returns>
    public static EventData ForNormalTarget(void* target, void* listener)
    {
        var data = new EventData();
        data.Data[1] = target;
        data.Data[2] = listener;
        return data;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EventData"/> class.
    /// </summary>
    /// <param name="target">Target.</param>
    /// <param name="listener">Event listener.</param>
    /// <returns>Event data.</returns>
    public static EventData ForNormalRightTarget(void* target, void* listener)
    {
        var data = ForNormalTarget(target, listener);
        data.Data[5] = (byte*)0x184003;
        data.Data[7] = target;
        return data;
    }
}