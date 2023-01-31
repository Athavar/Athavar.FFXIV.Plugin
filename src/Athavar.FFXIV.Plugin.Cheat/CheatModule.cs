// <copyright file="CheatModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Cheat;

using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Dalamud.Game;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Implements cheats.
/// </summary>
public class CheatModule : Module, IDisposable
{
    private const string ModuleName = "CheatModule";

    private readonly IDalamudServices dalamudServices;
    private readonly Dictionary<ICheat, bool> cheats;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CheatModule" /> class.
    /// </summary>
    /// <param name="moduleManager"><see cref="IModuleManager" /> added by DI.</param>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="provider"><see cref="IServiceProvider" /> added by DI.</param>
    public CheatModule(IModuleManager moduleManager, IDalamudServices dalamudServices, IServiceProvider provider)
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
    public override bool Enabled => true;

    /// <inheritdoc />
    public override void Enable(bool state)
    {
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