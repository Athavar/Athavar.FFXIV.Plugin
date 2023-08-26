// <copyright file="OpcodeWizardModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.OpcodeWizard;

using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Config;
using Microsoft.Extensions.DependencyInjection;

[Module(ModuleName, ModuleConfigurationType = typeof(OpcodeWizardConfiguration), HasTab = true)]
internal sealed class OpcodeWizardModule : Module<OpcodeWizardTab, OpcodeWizardConfiguration>
{
    internal const string ModuleName = "OpcodeWizard";
    private readonly IServiceProvider provider;

    public OpcodeWizardModule(OpcodeWizardConfiguration configuration, IServiceProvider provider)
        : base(configuration)
        => this.provider = provider;

    /// <inheritdoc/>
    public override string Name => ModuleName;

    protected override OpcodeWizardTab InitTab() => ActivatorUtilities.CreateInstance<OpcodeWizardTab>(this.provider);
}