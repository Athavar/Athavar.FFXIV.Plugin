// <copyright file="DutyHistoryModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.DutyHistory;

using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Config;
using Microsoft.Extensions.DependencyInjection;

[Module(ModuleName, ModuleConfigurationType = typeof(DutyHistoryConfiguration), HasTab = true)]
public sealed class DutyHistoryModule : Module<DutyHistoryTab, DutyHistoryConfiguration>
{
    internal const string ModuleName = "DutyHistory";

    private readonly IServiceProvider provider;

    public DutyHistoryModule(DutyHistoryConfiguration moduleConfig, IServiceProvider provider, StateTracker tracker)
        : base(moduleConfig)
    {
        this.provider = provider;
        _ = tracker;
    }

    public override string Name => ModuleName;

    protected override DutyHistoryTab InitTab() => ActivatorUtilities.CreateInstance<DutyHistoryTab>(this.provider);
}