// <copyright file="MeterManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps;

using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.UI;

internal class MeterManager
{
    private readonly IServiceProvider provider;
    private readonly DpsConfiguration Configuration;
    private Lazy<DpsTab>? tab;

    public MeterManager(Configuration configuration, IServiceProvider provider)
    {
        this.provider = provider;
        this.Configuration = configuration.Dps!;
        this.Load();
    }

    public List<MeterWindow> Meters { get; set; } = new();

    public void DeleteMeter(MeterWindow meter)
    {
        this.Meters.Remove(meter);
        this.Configuration.Meters.Remove(meter.Config);
        this.Save();
    }

    public void AddMeter(MeterWindow meter)
    {
        this.Meters.Add(meter);
        this.Configuration.Meters.Add(meter.Config);
        this.Save();
    }

    public void ConfigureMeter(MeterWindow meterWindow) => this.tab?.Value.PushConfig(meterWindow);

    public void Save() => this.Configuration.Save();

    internal void Setup(Lazy<DpsTab> t) => this.tab = t;

    private void Load()
    {
        var list = new List<MeterWindow>();
        foreach (var meter in this.Configuration.Meters)
        {
            list.Add(new MeterWindow(meter, this.provider, this));
        }

        this.Meters = list;
    }
}