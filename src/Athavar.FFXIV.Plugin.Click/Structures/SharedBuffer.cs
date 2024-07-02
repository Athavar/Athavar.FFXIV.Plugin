// <copyright file="SharedBuffer.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Structures;

using Reloaded.Memory.Sources;
using Reloaded.Memory.Utilities;

/// <summary>
///     A shared buffer for structures.
/// </summary>
public abstract class SharedBuffer
{
    static SharedBuffer()
        =>
            /*
                Allocation = Memory.Instance.Allocate(0x2048);
                Buffer = new CircularBuffer(Allocation.Address, 0x2048);
            */
            Buffer = new CircularBuffer(0x2048, Memory.Instance);

    /// <summary>
    ///     Gets the shared buffer.
    /// </summary>
    internal static CircularBuffer Buffer { get; }

    // private static MemoryAllocation Allocation { get; }

    /// <summary>
    ///     Dispose.
    /// </summary>
    public static void Dispose() => /*Memory.Instance.Free(Allocation);*/ Buffer.Dispose();
}