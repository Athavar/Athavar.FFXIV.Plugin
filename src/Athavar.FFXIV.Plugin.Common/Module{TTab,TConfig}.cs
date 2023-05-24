// <copyright file="Module{TTab,TConfig}.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common;

using Athavar.FFXIV.Plugin.Common.UI;

public abstract class Module<TTab, TConfig> : Module<TConfig>, IDisposable
    where TTab : Tab
    where TConfig : BasicModuleConfig
{
    private TTab? tab;

    protected Module(Configuration configuration, TConfig moduleConfig)
        : base(configuration, moduleConfig)
    {
    }

    /// <inheritdoc/>
    public override TTab Tab => this.tab ??= this.InitTab();

    /// <inheritdoc/>
    public virtual void Dispose() => this.tab?.Dispose();

    protected abstract TTab InitTab();
}