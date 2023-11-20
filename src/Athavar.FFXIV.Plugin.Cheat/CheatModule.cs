// <copyright file="CheatModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Cheat;

using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Models;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Implements cheats.
/// </summary>
[Module(ModuleName, Hidden = true)]
internal sealed class CheatModule : Module, IDisposable
{
    private const string ModuleName = "CheatModule";

    private readonly IDalamudServices dalamudServices;
    private readonly IFrameworkManager frameworkManager;
    private readonly Dictionary<Cheat, bool> cheats;

    private DateTimeOffset nextTick = DateTimeOffset.MinValue;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CheatModule"/> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices"/> added by DI.</param>
    /// <param name="provider"><see cref="IServiceProvider"/> added by DI.</param>
    /// <param name="frameworkManager"><see cref="IFrameworkManager"/> added by DI.</param>
    public CheatModule(IDalamudServices dalamudServices, IServiceProvider provider, IFrameworkManager frameworkManager)
    {
        this.dalamudServices = dalamudServices;
        this.frameworkManager = frameworkManager;

        this.cheats = typeof(Cheat).Assembly.GetTypes()
           .Where(type => type.IsAssignableTo(typeof(Cheat)) && type is { IsGenericType: false, IsInterface: false, IsAbstract: false })
           .Select(t => (Cheat)ActivatorUtilities.CreateInstance(provider, t)).ToDictionary(cheat => cheat, _ => false);

        frameworkManager.Subscribe(this.FrameworkOnUpdate);
        this.dalamudServices.ClientState.TerritoryChanged += this.OnTerritoryChange;
    }

    /// <inheritdoc/>
    public override string Name => ModuleName;

    /// <inheritdoc/>
    public override (Func<bool> Get, Action<bool> Set) GetEnableStateAction()
    {
        bool Get() => true;

        void Set(bool state)
        {
        }

        return (Get, Set);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.dalamudServices.ClientState.TerritoryChanged -= this.OnTerritoryChange;
        this.frameworkManager.Unsubscribe(this.FrameworkOnUpdate);

        foreach (var cheat in this.cheats.Where(c => c.Value))
        {
            cheat.Key.OnDisabled();
        }
    }

    private void FrameworkOnUpdate(IFramework framework)
    {
        if (DateTimeOffset.UtcNow < this.nextTick)
        {
            return;
        }

        this.nextTick = DateTimeOffset.UtcNow.AddSeconds(1);

        foreach (var cheat in this.cheats)
        {
            var state = cheat.Key.Enabled;
            if (state)
            {
                if (!cheat.Value)
                {
                    if (cheat.Key.OnEnabled())
                    {
                        this.cheats[cheat.Key] = true;
                    }
                }
            }
            else
            {
                if (cheat.Value)
                {
                    cheat.Key.OnDisabled();
                    this.cheats[cheat.Key] = true;
                }
            }
        }
    }

    private void OnTerritoryChange(ushort e)
    {
        foreach (var cheat in this.cheats.Where(c => c.Value))
        {
            cheat.Key.OnTerritoryChange(e);
        }
    }
}