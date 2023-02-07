// <copyright file="DpsConfigPage.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI;

using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.UI.Config;

internal class DpsConfigPage : IConfigurable
{
    public DpsConfigPage(IServiceProvider provider, MeterManager meterManager, Configuration configuration, EncounterManager encounterManager, NetworkHandler networkHandler)
    {
        this.ProfileListConfigPage = new ProfileListConfigPage(meterManager, provider);
        this.HistoryPage = new HistoryPage(encounterManager);
        this.SettingsConfigPage = new SettingsConfigPage(configuration.Dps!);

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