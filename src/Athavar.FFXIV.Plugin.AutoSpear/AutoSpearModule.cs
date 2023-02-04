// <copyright file="AutoSpearModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.AutoSpear;

using Athavar.FFXIV.Plugin.Common;
using Dalamud.Logging;
using Microsoft.Extensions.DependencyInjection;

public class AutoSpearModule : Module
{
    private const string ModuleName = "AutoSpear";
    private readonly IServiceProvider provider;
    private IAutoSpearTab? tab;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AutoSpearModule" /> class.
    /// </summary>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="provider"><see cref="IServiceProvider" /> added by DI.</param>
    public AutoSpearModule(Configuration configuration, IServiceProvider provider)
        : base(configuration)
    {
        this.provider = provider;
        PluginLog.LogDebug("Module 'AutoSpear' init");
    }

    /// <inheritdoc />
    public override string Name => ModuleName;

    /// <inheritdoc />
    public override bool Hidden => false;

    /// <inheritdoc />
    public override IAutoSpearTab Tab => this.tab ??= this.provider.GetRequiredService<IAutoSpearTab>();

    /// <inheritdoc />
    public override (Func<Configuration, bool> Get, Action<bool, Configuration> Set) GetEnableStateAction()
    {
        bool Get(Configuration c) => c.AutoSpear!.Enabled;

        void Set(bool state, Configuration c) => c.AutoSpear!.Enabled = state;

        return (Get, Set);
    }
}