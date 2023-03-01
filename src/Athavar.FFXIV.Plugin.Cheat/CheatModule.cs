// <copyright file="CheatModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Cheat;

using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Dalamud.Game;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Implements cheats.
/// </summary>
[Module(ModuleName, Hidden = true)]
internal class CheatModule : Module, IDisposable
{
    private const string ModuleName = "CheatModule";

    private readonly IDalamudServices dalamudServices;
    private readonly Dictionary<ICheat, bool> cheats;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CheatModule" /> class.
    /// </summary>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="provider"><see cref="IServiceProvider" /> added by DI.</param>
    public CheatModule(Configuration configuration, IDalamudServices dalamudServices, IServiceProvider provider)
        : base(configuration)
    {
        this.dalamudServices = dalamudServices;

        this.cheats = typeof(ICheat).Assembly.GetTypes()
           .Where(type => type.IsAssignableTo(typeof(ICheat)) && type is { IsGenericType: false, IsInterface: false })
           .Select(t => (ICheat)ActivatorUtilities.CreateInstance(provider, t)).ToDictionary(cheat => cheat, _ => false);

        this.dalamudServices.Framework.Update += this.FrameworkOnUpdate;
    }

    /// <inheritdoc />
    public override string Name => ModuleName;

    /// <inheritdoc />
    public override bool Hidden => true;

    /// <inheritdoc />
    public override (Func<bool> Get, Action<bool> Set) GetEnableStateAction()
    {
        bool Get() => true;

        void Set(bool state)
        {
        }

        return (Get, Set);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.dalamudServices.Framework.Update -= this.FrameworkOnUpdate;

        foreach (var cheat in this.cheats.Where(c => c.Value))
        {
            cheat.Key.OnDisabled();
        }
    }

    private void FrameworkOnUpdate(Framework framework)
    {
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
}