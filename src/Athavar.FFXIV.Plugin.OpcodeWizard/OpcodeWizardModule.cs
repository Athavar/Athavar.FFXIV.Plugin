// <copyright file="OpcodeWizardModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.OpcodeWizard;

using Athavar.FFXIV.Plugin.Common;
using Microsoft.Extensions.DependencyInjection;

public class OpcodeWizardModule : Module
{
    internal const string ModuleName = "OpcodeWizard";
    private readonly IServiceProvider provider;
    private IOpcodeWizardTab? tab;

    public OpcodeWizardModule(Configuration configuration, IServiceProvider provider)
        : base(configuration)
        => this.provider = provider;

    /// <inheritdoc />
    public override IOpcodeWizardTab Tab => this.tab ??= this.provider.GetRequiredService<IOpcodeWizardTab>();

    /// <inheritdoc />
    public override string Name => ModuleName;

    /// <inheritdoc />
    public override (Func<Configuration, bool> Get, Action<bool, Configuration> Set) GetEnableStateAction()
    {
        bool Get(Configuration c) => c.OpcodeWizard!.Enabled;

        void Set(bool state, Configuration c) => c.OpcodeWizard!.Enabled = state;

        return (Get, Set);
    }
}