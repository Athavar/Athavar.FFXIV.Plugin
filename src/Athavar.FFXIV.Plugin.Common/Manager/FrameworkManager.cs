// <copyright file="FrameworkManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Diagnostics;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Plugin.Services;

internal class FrameworkManager : IDisposable, IFrameworkManager
{
    private readonly IDalamudServices dalamudServices;
    private readonly List<RegisteredDelegation> onUpdateDelegates;
    private readonly Stopwatch stopwatch = new();

    private RegisteredDelegation[] onUpdateDelegatesArray;

    public FrameworkManager(IDalamudServices dalamudServices)
    {
        this.dalamudServices = dalamudServices;

        this.onUpdateDelegates = new List<RegisteredDelegation>();
        this.onUpdateDelegatesArray = Array.Empty<RegisteredDelegation>();
        this.dalamudServices.Framework.Update += this.FrameworkOnUpdate;
    }

    public IReadOnlyList<IFrameworkManager.IRegisteredDelegation> RegisteredDelegations => this.onUpdateDelegates.AsReadOnly();

    public void Subscribe(IFramework.OnUpdateDelegate updateDelegate)
    {
        var methode = updateDelegate.Method;
        this.onUpdateDelegates.Add(
            new RegisteredDelegation
            {
                Name = $"{methode.DeclaringType?.FullName}::{methode.Name}",
                UpdateDelegate = updateDelegate,
            });
        this.onUpdateDelegatesArray = this.onUpdateDelegates.ToArray();
    }

    public void Unsubscribe(IFramework.OnUpdateDelegate updateDelegate)
    {
        var match = this.onUpdateDelegates.Find(o => o.UpdateDelegate == updateDelegate);
        if (match is not null)
        {
            this.onUpdateDelegates.Remove(match);
            this.onUpdateDelegatesArray = this.onUpdateDelegates.ToArray();
        }
    }

    public void Dispose() => this.dalamudServices.Framework.Update -= this.FrameworkOnUpdate;

    private void FrameworkOnUpdate(IFramework framework)
    {
        foreach (var updateDelegate in this.onUpdateDelegatesArray)
        {
            this.stopwatch.Restart();
            updateDelegate.UpdateDelegate(framework);
            this.stopwatch.Stop();
            updateDelegate.Times.Add(this.stopwatch.Elapsed);
        }
    }

    private sealed class RegisteredDelegation : IFrameworkManager.IRegisteredDelegation
    {
        public IReadOnlyList<TimeSpan> Duration => new List<TimeSpan>(this.Times);

        public required string Name { get; init; }

        public required IFramework.OnUpdateDelegate UpdateDelegate { get; init; }

        internal RollingList<TimeSpan> Times { get; } = new(60);
    }
}