// <copyright file="DpsConfigPage.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI;

using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.UI.Config;

internal sealed class DpsConfigPage : IConfigurable
{
    public DpsConfigPage(IServiceProvider provider, Utils utils, MeterManager meterManager, DpsConfiguration configuration, EncounterManager encounterManager, NetworkHandler networkHandler)
    {
        this.ProfileListConfigPage = new ProfileListConfigPage(meterManager, provider);
        this.HistoryPage = new HistoryPage(encounterManager, utils);
        this.SettingsConfigPage = new SettingsConfigPage(configuration);

#if DEBUG
        this.LogPage = new LogPage(encounterManager, networkHandler);
#endif
    }

    public string Name
    {
        get => DpsModule.ModuleName;
        set => _ = value;
    }

    private ProfileListConfigPage ProfileListConfigPage { get; }

    private HistoryPage HistoryPage { get; }

    private SettingsConfigPage SettingsConfigPage { get; }

#if DEBUG
    private LogPage LogPage { get; }
#endif

    public IEnumerable<IConfigPage> GetConfigPages()
    {
        yield return this.ProfileListConfigPage;
        yield return this.HistoryPage;
        yield return this.SettingsConfigPage;
#if DEBUG
        yield return this.LogPage;
#endif
    }

    public void ImportConfig(IConfig c)
    {
    }
}