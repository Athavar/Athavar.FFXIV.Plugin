// <copyright file="IPluginMonitorService.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Interfaces.Manager;

using Dalamud.Plugin;

public interface IPluginMonitorService
{
    public delegate void PluginLoadingStateChanged(string name, bool state, IExposedPlugin? plugin);

    event PluginLoadingStateChanged? LoadingStateHasChanged;

    bool IsLoaded(string name);
}