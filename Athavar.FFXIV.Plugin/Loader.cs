// <copyright file="Loader.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin;

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Loader;

public static class Loader
{
    // [ModuleInitializer]
    internal static void ModuleInit()
    {
        File.WriteAllText("/home/athavar/.xlcore/ffxivload.test", "Init success");
        var loadContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
        if (loadContext != null)
        {
            loadContext.Resolving += ResolveAssembly;
        }
        else
        {
            Debugger.Break();
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }
    }

    /*
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static void MakeCosturaStopComplainingAboutNotCallingCosturaUtilityInitialize() => AssemblyLoader.Attach();
    */
    private static Assembly? ResolveAssembly(object? sender, ResolveEventArgs args)
    {
        var name = args.Name;
        File.WriteAllText("/home/athavar/.xlcore/ffxivload.test2", name);
        Debugger.Break();
        if (name == null)
        {
            return null;
        }

        var manifestResourceBytes = Assembly.GetExecutingAssembly().GetUncompressedManifestResourceBytes("costura." + name.ToLowerInvariant() + ".dll.compressed");
        if (manifestResourceBytes == null)
        {
            return null;
        }

        try
        {
            return Assembly.Load(manifestResourceBytes);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static Assembly? ResolveAssembly(AssemblyLoadContext context, AssemblyName name)
    {
        var name1 = name.Name;
        if (name1 == null)
        {
            return null;
        }

        var manifestResourceStream = Assembly.GetExecutingAssembly().GetUncompressedManifestResourceStream("costura." + name1.ToLowerInvariant() + ".dll.compressed");
        if (manifestResourceStream == null)
        {
            return null;
        }

        try
        {
            return context.LoadFromStream(manifestResourceStream);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static Stream? GetUncompressedManifestResourceStream(
        this Assembly assembly,
        string name)
    {
        var manifestResourceStream = assembly.GetManifestResourceStream(name);
        if (manifestResourceStream == null)
        {
            return null;
        }

        using var deflateStream = new DeflateStream(manifestResourceStream, CompressionMode.Decompress, false);
        var destination = new MemoryStream();
        deflateStream.CopyTo(destination);
        destination.Position = 0L;
        return destination;
    }

    private static byte[]? GetUncompressedManifestResourceBytes(this Assembly assembly, string name)
    {
        using var manifestResourceStream = assembly.GetManifestResourceStream(name);
        if (manifestResourceStream == null)
        {
            return null;
        }

        var buffer = new byte[manifestResourceStream.Length];
        manifestResourceStream.Read((Span<byte>)buffer);
        return buffer;
    }
}