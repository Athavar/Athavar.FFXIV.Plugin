// <copyright file="PluginLauchButton.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin;

using System;
using System.IO;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Dalamud.Interface;
using ImGuiScene;

internal sealed class PluginLaunchButton : IDisposable
{
    private readonly IDalamudServices services;
    private readonly Action onTrigger;
    private TextureWrap? icon;
    private TitleScreenMenu.TitleScreenMenuEntry? entry;

    public PluginLaunchButton(IDalamudServices services, Action onTrigger)
    {
        this.services = services;
        this.onTrigger = onTrigger;
    }

    public void AddEntry()
    {
        if (this.entry is not null)
        {
            return;
        }

        void CreateEntry()
        {
            this.icon = this.services.PluginInterface.UiBuilder.LoadImage(Path.Combine(this.services.PluginInterface.AssemblyLocation.DirectoryName!, "icon.png"));
            if (this.icon != null)
            {
                this.entry = this.services.TitleScreenMenu.AddEntry($"Manage {Plugin.PluginName}", this.icon, this.onTrigger);
            }

            this.services.PluginInterface.UiBuilder.Draw -= CreateEntry;
        }

        this.services.PluginInterface.UiBuilder.Draw += CreateEntry;
    }

    public void RemoveEntry()
    {
        var entry = this.entry;
        if (entry != null)
        {
            this.entry = null;
            this.services.TitleScreenMenu.RemoveEntry(entry);
        }
    }

    public void Dispose()
    {
        this.RemoveEntry();
        this.icon?.Dispose();
    }
}