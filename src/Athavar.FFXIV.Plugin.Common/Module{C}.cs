// <copyright file="Module{T,C}.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common;

public abstract class Module<C> : Module
    where C : BasicModuleConfig
{
    protected Module(Configuration configuration, C moduleConfig)
        : base(configuration)
        => this.ModuleConfig = moduleConfig;

    protected internal C ModuleConfig { get; }

    /// <inheritdoc/>
    public override (Func<bool> Get, Action<bool> Set) GetEnableStateAction()
    {
        bool Get() => this.ModuleConfig.Enabled;

        void Set(bool state)
        {
            this.ModuleConfig.Enabled = state;
            if (state)
            {
                this.OnEnabled();
            }
            else
            {
                this.OnDisabled();
            }
        }

        return (Get, Set);
    }

    protected virtual void OnEnabled()
    {
    }

    protected virtual void OnDisabled()
    {
    }
}