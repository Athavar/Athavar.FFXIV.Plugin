// <copyright file="AutoSpearModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.AutoSpear;

using Athavar.FFXIV.Plugin.Common;
using Dalamud.Logging;
using Microsoft.Extensions.DependencyInjection;

[Module(ModuleName, ModuleConfigurationType = typeof(AutoSpearConfiguration))]
internal class AutoSpearModule : Module<AutoSpear, AutoSpearConfiguration>
{
    private const string ModuleName = "AutoSpear";
    private readonly IServiceProvider provider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AutoSpearModule" /> class.
    /// </summary>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="provider"><see cref="IServiceProvider" /> added by DI.</param>
    public AutoSpearModule(Configuration configuration, IServiceProvider provider)
        : base(configuration, configuration.AutoSpear!)
    {
        this.provider = provider;
        PluginLog.LogDebug("Module 'AutoSpear' init");
    }

    /// <inheritdoc />
    public override string Name => ModuleName;

    /// <inheritdoc />
    public override bool Hidden => false;

    protected override AutoSpear InitTab() => ActivatorUtilities.CreateInstance<AutoSpear>(this.provider);
}