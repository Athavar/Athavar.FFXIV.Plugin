// <copyright file="FrameworkManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Dalamud.Game;

internal class FrameworkManager : IDisposable, IFrameworkManager
{
    private readonly IDalamudServices dalamudServices;
    private readonly List<Framework.OnUpdateDelegate> onUpdateDelegates;

    private Framework.OnUpdateDelegate[] onUpdateDelegatesArray;

    public FrameworkManager(IDalamudServices dalamudServices)
    {
        this.dalamudServices = dalamudServices;

        this.onUpdateDelegates = new List<Framework.OnUpdateDelegate>();
        this.onUpdateDelegatesArray = Array.Empty<Framework.OnUpdateDelegate>();
        this.dalamudServices.Framework.Update += this.FrameworkOnUpdate;
    }

    public void Subscribe(Framework.OnUpdateDelegate updateDelegate)
    {
        this.onUpdateDelegates.Add(updateDelegate);
        this.onUpdateDelegatesArray = this.onUpdateDelegates.ToArray();
    }

    public void Unsubscribe(Framework.OnUpdateDelegate updateDelegate)
    {
        this.onUpdateDelegates.Remove(updateDelegate);
        this.onUpdateDelegatesArray = this.onUpdateDelegates.ToArray();
    }

    public void Dispose() => this.dalamudServices.Framework.Update -= this.FrameworkOnUpdate;

    private void FrameworkOnUpdate(Framework framework)
    {
        foreach (var updateDelegate in this.onUpdateDelegatesArray)
        {
            updateDelegate(framework);
        }
    }
}