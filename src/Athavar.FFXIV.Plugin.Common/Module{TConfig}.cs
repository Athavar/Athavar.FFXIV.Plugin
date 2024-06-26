// <copyright file="Module{TConfig}.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common;

using Athavar.FFXIV.Plugin.Models;

public abstract class Module<TConfig> : Module
    where TConfig : BasicModuleConfig
{
    protected Module(TConfig moduleConfig) => this.ModuleConfig = moduleConfig;

    protected internal TConfig ModuleConfig { get; }

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