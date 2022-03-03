// <copyright file="Click.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Lib.ClickLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Bases;

/// <summary>
///     Main class for clicking by name.
/// </summary>
internal class Click : IClick
{
    private readonly Dictionary<string, PrecompiledDelegate> availableClicks = new();
    private bool initialized;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Click" /> class.
    /// </summary>
    public Click() => this.Initialize();

    private delegate void PrecompiledDelegate(IntPtr addon);

    /// <inheritdoc />
    public void SendClick(string name, IntPtr addon = default)
    {
        if (!this.initialized)
        {
            throw new InvalidClickException("Not initialized yet");
        }

        if (!this.availableClicks.TryGetValue(name, out var clickDelegate))
        {
            throw new ClickNotFoundError($"Click \"{name}\" does not exist");
        }

        clickDelegate!(addon);
    }

    /// <inheritdoc />
    public bool TrySendClick(string name, IntPtr addon = default)
    {
        try
        {
            this.SendClick(name, addon);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public IList<string> GetClickNames() => this.availableClicks.Keys.ToList();

    /// <summary>
    ///     Load the mapping of click names.
    /// </summary>
    private void Initialize()
    {
        if (this.initialized)
        {
            return;
        }

        this.initialized = true;

        // Get all parameterless methods, of types that inherit from ClickBase
        var clicks = typeof(ClickBase).Assembly.GetTypes()
                                      .Where(type => type.IsSubclassOf(typeof(ClickBase)) && !type.IsGenericType)
                                      .SelectMany(cls => cls.GetMethods())
                                      .Where(method => method.GetParameters().Length == 0)
                                      .Select(method => (method, method.GetCustomAttribute<ClickNameAttribute>()?.Name))
                                      .Where(tpl => tpl.Name != null);

        foreach (var click in clicks)
        {
            var method = click.method;
            var clickType = method.DeclaringType!;
            var ctor = clickType.GetConstructor(new[] { typeof(IntPtr) })!;

            var param = Expression.Parameter(typeof(IntPtr), "addon");
            var instantiate = Expression.New(ctor, param);
            var invoke = Expression.Call(instantiate, method);
            var blockExpr = Expression.Block(invoke);
            var lambdaExpr = Expression.Lambda<PrecompiledDelegate>(blockExpr, param);
            var compiled = lambdaExpr.Compile()!;

            this.availableClicks.Add(click.Name!, compiled);
        }
    }
}