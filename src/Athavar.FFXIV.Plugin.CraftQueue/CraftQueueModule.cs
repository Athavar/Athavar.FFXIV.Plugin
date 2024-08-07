// <copyright file="CraftQueueModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue;

using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Microsoft.Extensions.DependencyInjection;

[Module(ModuleName, ModuleConfigurationType = typeof(CraftQueueConfiguration), HasTab = true)]
internal sealed class CraftQueueModule : Module<CraftQueueTab, CraftQueueConfiguration>
{
    private const string ModuleName = "CraftQueue";

    private readonly IServiceProvider provider;

    private CraftQueue? craftQueue;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CraftQueueModule"/> class.
    /// </summary>
    /// <param name="configuration"><see cref="CraftQueueConfiguration"/> added by DI.</param>
    /// <param name="provider"><see cref="IServiceProvider"/> added by DI.</param>
    /// <param name="log"><see cref="IPluginLogger"/> added by DI.</param>
    public CraftQueueModule(CraftQueueConfiguration configuration, IServiceProvider provider, IPluginLogger log)
        : base(configuration)
    {
        this.provider = provider;

        log.Debug("Module 'CraftQueue' init");
    }

    /// <inheritdoc/>
    public override string Name => ModuleName;

    /// <inheritdoc/>
    public override void Dispose()
    {
        base.Dispose();
        this.craftQueue?.Dispose();
    }

    protected override CraftQueueTab InitTab()
    {
        this.craftQueue = ActivatorUtilities.CreateInstance<CraftQueue>(this.provider);
        return ActivatorUtilities.CreateInstance<CraftQueueTab>(this.provider, this.craftQueue);
    }
}