// <copyright file="Click.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click;

using System.Linq.Expressions;
using System.Reflection;
using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
using Athavar.FFXIV.Plugin.Click.Exceptions;

/// <summary>
///     Main class for clicking by name.
/// </summary>
public class Click : IClick
{
    private readonly Dictionary<string, PrecompiledDelegate> availableClicks = new();
    private bool initialized;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Click" /> class.
    /// </summary>
    public Click() => this.Initialize();

    private delegate void PrecompiledDelegate(nint addon);

    /// <summary>
    ///     Load the mapping of click names.
    /// </summary>
    public void Initialize()
    {
        if (this.initialized)
        {
            return;
        }

        this.initialized = true;

        // Get all parameterless methods, of types that inherit from ClickBase
        var clicks = typeof(IClickable).Assembly.GetTypes()
           .Where(type => type.IsAssignableTo(typeof(IClickable)) && !type.IsGenericType)
           .SelectMany(cls => cls.GetMethods())
           .Where(method => method.GetParameters().Length == 0)
           .Select(method => (method, method.GetCustomAttribute<ClickNameAttribute>()?.Name))
           .Where(tpl => tpl.Name != null);

        foreach (var click in clicks)
        {
            var method = click.method;
            var clickType = method.DeclaringType!;
            var ctor = clickType.GetConstructor(new[] { typeof(nint) })!;

            var param = Expression.Parameter(typeof(nint), "addon");
            var instantiate = Expression.New(ctor, param);
            var invoke = Expression.Call(instantiate, method);
            var blockExpr = Expression.Block(invoke);
            var lambdaExpr = Expression.Lambda<PrecompiledDelegate>(blockExpr, param);
            var compiled = lambdaExpr.Compile()!;

            this.availableClicks.Add(click.Name!, compiled);
        }
    }

    /// <summary>
    ///     Get a list of available click strings that can be used with SendClick.
    /// </summary>
    /// <returns>A list of click names.</returns>
    public IList<string> GetClickNames() => this.availableClicks.Keys.ToList();

    /// <summary>
    ///     Send a click by the name of the individual click.
    /// </summary>
    /// <param name="name">Click name.</param>
    /// <param name="addon">Pointer to an existing addon.</param>
    public void SendClick(string name, nint addon = default)
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

    /// <summary>
    ///     Send a click by the name of the individual click.
    /// </summary>
    /// <param name="name">Click name.</param>
    /// <param name="addon">Pointer to an existing addon.</param>
    /// <returns>A value indicating whether the delegate was successfully called.</returns>
    public bool TrySendClick(string name, nint addon = default)
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
}