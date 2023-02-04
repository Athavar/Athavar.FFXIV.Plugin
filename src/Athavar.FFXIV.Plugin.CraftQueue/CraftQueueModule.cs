// <copyright file="CraftQueueModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue;

using Athavar.FFXIV.Plugin.Common;
using Dalamud.Logging;
using Microsoft.Extensions.DependencyInjection;

public class CraftQueueModule : Module
{
    private const string ModuleName = "CraftQueue";

    private readonly IServiceProvider provider;

    private ICraftQueueTab? tab;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CraftQueueModule" /> class.
    /// </summary>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="provider"><see cref="IServiceProvider" /> added by DI.</param>
    public CraftQueueModule(Configuration configuration, IServiceProvider provider)
        : base(configuration)
    {
        this.provider = provider;

        PluginLog.LogDebug("Module 'CraftQueue' init");
    }

    /// <inheritdoc />
    public override string Name => ModuleName;

    /// <inheritdoc />
    public override bool Hidden => false;

    /// <inheritdoc />
    public override ICraftQueueTab Tab => this.tab ??= this.provider.GetRequiredService<ICraftQueueTab>();

    /// <inheritdoc />
    public override (Func<Configuration, bool> Get, Action<bool, Configuration> Set) GetEnableStateAction()
    {
        bool Get(Configuration c) => c.CraftQueue!.Enabled;

        void Set(bool state, Configuration configuration) => configuration.CraftQueue!.Enabled = state;

        return (Get, Set);
    }
}