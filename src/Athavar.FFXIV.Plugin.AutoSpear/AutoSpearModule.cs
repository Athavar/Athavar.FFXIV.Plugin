// <copyright file="AutoSpearModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.AutoSpear;

using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Microsoft.Extensions.DependencyInjection;

[Module(ModuleName, ModuleConfigurationType = typeof(AutoSpearConfiguration), HasTab = true)]
internal sealed class AutoSpearModule : Module<AutoSpear, AutoSpearConfiguration>
{
    private const string ModuleName = "AutoSpear";
    private readonly IServiceProvider provider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AutoSpearModule"/> class.
    /// </summary>
    /// <param name="configuration"><see cref="AutoSpearConfiguration"/> added by DI.</param>
    /// <param name="provider"><see cref="IServiceProvider"/> added by DI.</param>
    /// <param name="log"><see cref="IPluginLogger"/> added by DI.</param>
    public AutoSpearModule(AutoSpearConfiguration configuration, IServiceProvider provider, IPluginLogger log)
        : base(configuration)
    {
        this.provider = provider;
        log.Debug("Module 'AutoSpear' init");
    }

    /// <inheritdoc/>
    public override string Name => ModuleName;

    protected override AutoSpear InitTab() => ActivatorUtilities.CreateInstance<AutoSpear>(this.provider);
}