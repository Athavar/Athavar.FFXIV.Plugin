// <copyright file="IntPtrExtension.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Extension;

using System;
using System.Runtime.InteropServices;

public static class IntPtrExtension
{
    public static unsafe T?[] CreateArray<T>(void* source, int length)
    {
        var type = typeof(T);
        var sizeInBytes = Marshal.SizeOf(typeof(T));

        var output = new T?[length];

        if (type.IsPrimitive)
        {
            // Make sure the array won't be moved around by the GC 
            var handle = GCHandle.Alloc(output, GCHandleType.Pinned);

            var destination = (byte*)handle.AddrOfPinnedObject().ToPointer();
            var byteLength = length * sizeInBytes;

            // There are faster ways to do this, particularly by using wider types or by 
            // handling special lengths.
            for (var i = 0; i < byteLength; i++)
            {
                destination[i] = ((byte*)source)[i];
            }

            handle.Free();
        }
        else if (type.IsValueType)
        {
            if (!type.IsLayoutSequential && !type.IsExplicitLayout)
            {
                throw new InvalidOperationException($"{type} does not define a StructLayout attribute");
            }

            var sourcePtr = new IntPtr(source);

            for (var i = 0; i < length; i++)
            {
                var p = new IntPtr((byte*)source + (i * sizeInBytes));

                output[i] = (T?)Marshal.PtrToStructure(p, typeof(T));
            }
        }
        else
        {
            throw new InvalidOperationException($"{type} is not supported");
        }

        return output;
    }
}