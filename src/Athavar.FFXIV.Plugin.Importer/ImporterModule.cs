// <copyright file="ImporterModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Importer;

using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Config;
using Microsoft.Extensions.DependencyInjection;

[Module(ModuleName, ModuleConfigurationType = typeof(ImporterConfiguration), HasTab = true)]
public class ImporterModule(ImporterConfiguration moduleConfig, IServiceProvider provider)
    : Module<ImporterTab, ImporterConfiguration>(moduleConfig)
{
    private const string ModuleName = "Importer";

    public override string Name => ModuleName;

    protected override ImporterTab InitTab() => ActivatorUtilities.CreateInstance<ImporterTab>(provider);
}