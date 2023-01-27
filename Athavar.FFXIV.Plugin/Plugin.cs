// <copyright file="Plugin.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;
using Athavar.FFXIV.Plugin.Manager;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Module;
using Athavar.FFXIV.Plugin.Utils;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Main plugin implementation.
/// </summary>
public sealed class Plugin : IDalamudPlugin
{
    /// <summary>
    ///     prefix of the command.
    /// </summary>
    internal const string CommandName = "/ath";

    /// <summary>
    ///     The Plugin name.
    /// </summary>
    internal const string PluginName = "Athavar's Toolbox";

    private readonly DalamudPluginInterface pluginInterface;

    private readonly ServiceProvider provider;
    private readonly PluginService servive;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Plugin" /> class.
    /// </summary>
    /// <param name="pluginInterface">Dalamud plugin interface.</param>
    public Plugin(
        [RequiredVersion("1.0")]
        DalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;

        this.provider = this.BuildProvider();
        this.servive = this.provider.GetRequiredService<PluginService>();
        this.servive.Start();
    }

    /// <inheritdoc />
    public string Name => PluginName;

    /// <inheritdoc />
    public void Dispose()
    {
        this.servive.Stop();
        this.provider.DisposeAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Try to catch all exception.
    /// </summary>
    /// <param name="action">Action that can throw exception.</param>
    internal static void CatchCrash(Action action)
    {
        try
        {
            action.Invoke();
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Don't crash the game");
        }
    }

    private ServiceProvider BuildProvider()
    {
        return new ServiceCollection()
           .AddSingleton(this.pluginInterface)
           .AddSingleton<IDalamudServices, DalamudServices>()
           .AddSingleton<IModuleManager, ModuleManager>()
           .AddSingleton<ILocalizerManager, LocalizerManager>()
           .AddSingleton<IIconCacheManager, IconCacheManager>()
           .AddSingleton<IGearsetManager, GearsetManager>()
           .AddSingleton<PluginWindow>()
           .AddSingleton(o =>
            {
                var ser = o.GetRequiredService<IDalamudServices>();
                CraftingSkill.Populate(ser.DataManager);

                var pi = ser.PluginInterface;

                var c = (Configuration?)pi.GetPluginConfig() ?? new Configuration();
                c.Setup(this.pluginInterface);
                return c;
            })
           .AddSingleton(_ => new WindowSystem("Athavar's Toolbox"))
           .AddSingleton<IChatManager, ChatManager>()
           .AddSingleton<EquipmentScanner>()
           .AddSingleton<KeyStateExtended>()
           .AddSingleton<AutoTranslateWindow>()
           .AddSingleton<IClick, Click>()
           .AddSingleton<ICommandInterface, CommandInterface>()
           .AddMacroModule()
           .AddYesModule()
           .AddInstancinatorModule()
           .AddAutoSpearModule()
           .AddCheatModule()
           .AddCraftQueueModule()
#if DEBUG
           .AddItemInspectorModule()
#endif
           .AddSingleton<PluginService>()
           .BuildServiceProvider();
    }
}