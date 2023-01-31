// <copyright file="AutoSpearModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.AutoSpear;

using Athavar.FFXIV.Plugin.Common;
using Dalamud.Logging;

public class AutoSpearModule : Module
{
    private const string ModuleName = "AutoSpear";

    /// <summary>
    ///     Initializes a new instance of the <see cref="AutoSpearModule" /> class.
    /// </summary>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="tab"><see cref="IAutoSpearTab" /> added by DI.</param>
    public AutoSpearModule(Configuration configuration, IAutoSpearTab tab)
    {
        this.Configuration = configuration.AutoSpear!;

        this.Tab = tab;
        PluginLog.LogDebug("Module 'AutoSpear' init");
    }

    /// <inheritdoc />
    public override string Name => ModuleName;

    /// <inheritdoc />
    public override bool Hidden => false;

    /// <inheritdoc />
    public override bool Enabled => this.Configuration.Enabled;

    /// <inheritdoc />
    public override IAutoSpearTab? Tab { get; }

    /// <summary>
    ///     Gets the configuration.
    /// </summary>
    internal AutoSpearConfiguration Configuration { get; }

    /// <inheritdoc />
    public override void Enable(bool state = true) => this.Configuration.Enabled = state;
}