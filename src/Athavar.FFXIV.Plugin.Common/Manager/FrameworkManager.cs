// <copyright file="FrameworkManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Dalamud.Plugin.Services;

internal class FrameworkManager : IDisposable, IFrameworkManager
{
    private readonly IDalamudServices dalamudServices;
    private readonly List<IFramework.OnUpdateDelegate> onUpdateDelegates;

    private IFramework.OnUpdateDelegate[] onUpdateDelegatesArray;

    public FrameworkManager(IDalamudServices dalamudServices)
    {
        this.dalamudServices = dalamudServices;

        this.onUpdateDelegates = new List<IFramework.OnUpdateDelegate>();
        this.onUpdateDelegatesArray = Array.Empty<IFramework.OnUpdateDelegate>();
        this.dalamudServices.Framework.Update += this.FrameworkOnUpdate;
    }

    public void Subscribe(IFramework.OnUpdateDelegate updateDelegate)
    {
        this.onUpdateDelegates.Add(updateDelegate);
        this.onUpdateDelegatesArray = this.onUpdateDelegates.ToArray();
    }

    public void Unsubscribe(IFramework.OnUpdateDelegate updateDelegate)
    {
        this.onUpdateDelegates.Remove(updateDelegate);
        this.onUpdateDelegatesArray = this.onUpdateDelegates.ToArray();
    }

    public void Dispose() => this.dalamudServices.Framework.Update -= this.FrameworkOnUpdate;

    private void FrameworkOnUpdate(IFramework framework)
    {
        foreach (var updateDelegate in this.onUpdateDelegatesArray)
        {
            updateDelegate(framework);
        }
    }
}