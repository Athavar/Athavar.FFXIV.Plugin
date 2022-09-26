// <copyright file="Configuration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

using System.Timers;
using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using Newtonsoft.Json;

internal class Configuration : IPluginConfiguration
{
    [JsonIgnore]
    private DalamudPluginInterface? pi;

    [JsonIgnore]
    private Timer? saveTimer;

    /// <inheritdoc />
    public int Version { get; set; } = 1;

    public YesConfiguration? Yes { get; set; }

    public MacroConfiguration? Macro { get; set; }

    public InstancinatorConfiguration? Instancinator { get; set; }

    public AutoSpearConfiguration? AutoSpear { get; set; }

    public bool ShowToolTips { get; set; } = true;

    public Language Language { get; set; } = Language.En;

    /// <summary>
    ///     Gets or sets the chat channel to use.
    /// </summary>
    public XivChatType ChatType { get; set; } = XivChatType.Debug;

    /// <summary>
    ///     Gets or sets the error chat channel to use.
    /// </summary>
    public XivChatType ErrorChatType { get; set; } = XivChatType.Urgent;

    /// <summary>
    ///     Save the configuration.
    /// </summary>
    /// <param name="instant">Indicate if the configuration should be saved instant.</param>
    public void Save(bool instant = false)
    {
        if (this.saveTimer is null)
        {
            this.saveTimer = new Timer();
            this.saveTimer.Interval = 2500;
            this.saveTimer.AutoReset = false;
            this.saveTimer.Elapsed += (_, _) => this.Save(true);
        }

        if (instant)
        {
            this.saveTimer.Stop();
            this.pi?.SavePluginConfig(this);
        }
        else if (!this.saveTimer.Enabled)
        {
            this.saveTimer.Start();
        }
    }

    public void Dispose()
    {
        this.saveTimer?.Dispose();
        this.saveTimer = null;
    }

    /// <summary>
    ///     Setup <see cref="DalamudPluginInterface" />.
    /// </summary>
    /// <param name="interface">The <see cref="DalamudPluginInterface" />.</param>
    internal void Setup(DalamudPluginInterface @interface)
    {
        this.pi = @interface;

        this.Yes ??= new YesConfiguration();
        this.Macro ??= new MacroConfiguration();
        this.Instancinator ??= new InstancinatorConfiguration();
        this.AutoSpear ??= new AutoSpearConfiguration();

        this.Yes.Setup(this);
        this.Macro.Setup(this);
        this.Instancinator.Setup(this);
        this.AutoSpear.Setup(this);
    }
}