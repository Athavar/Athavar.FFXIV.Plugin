// <copyright file="Module{T,C}.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common;

using Athavar.FFXIV.Plugin.Common.UI;

public abstract class Module<T, C> : Module<C>, IDisposable
    where T : Tab
    where C : BasicModuleConfig
{
    private T? tab;

    protected Module(Configuration configuration, C moduleConfig)
        : base(configuration, moduleConfig)
    {
    }

    /// <inheritdoc/>
    public override T Tab => this.tab ??= this.InitTab();

    /// <inheritdoc/>
    public virtual void Dispose() => this.tab?.Dispose();

    protected abstract T InitTab();
}