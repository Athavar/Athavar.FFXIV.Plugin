// <copyright file="TabBarHandler.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.UI;

using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Common.Utils;
using ImGuiNET;

/// <summary>
///     Handler for ImGui tabs.
/// </summary>
public sealed class TabBarHandler
{
    private readonly List<TabDefinition> tabs = new();
    private readonly string name;
    private readonly object lockObject = new();

    private string lastSelectedTabTitle = string.Empty;
    private string? selectTabIdentifier;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TabBarHandler" /> class.
    /// </summary>
    /// <param name="name">The name of the <see cref="TabBarHandler" />.</param>
    public TabBarHandler(string name) => this.name = $"##{name}";

    /// <summary>
    ///     Add a tab.
    /// </summary>
    /// <param name="tab">The tab.</param>
    /// <returns>return for chaining.</returns>
    public TabBarHandler Add(ITab tab)
    {
        lock (this.lockObject)
        {
            this.tabs.Add(new TabDefinition(tab, $"{tab.Name}##{tab.Identifier}", tab.Identifier));
        }

        return this;
    }

    public string GetTabTitle() => this.lastSelectedTabTitle;

    public void SelectTab(string identifier) => this.selectTabIdentifier = identifier;

    /// <summary>
    ///     Remove a tab.
    /// </summary>
    /// <param name="tab">The tab.</param>
    /// <returns>return for chaining.</returns>
    public TabBarHandler Remove(ITab tab)
    {
        lock (this.lockObject)
        {
            this.tabs.RemoveAll(t => t.Identifier.Equals(tab.Identifier));
        }

        return this;
    }

    /// <summary>
    ///     Draw the TabBar.
    /// </summary>
    public void Draw()
    {
        if (ImGui.BeginTabBar(this.name))
        {
            Span<TabDefinition> tabs;
            lock (this.lockObject)
            {
                tabs = this.tabs.ToArray();
            }

            foreach (var tab in tabs)
            {
                var flags = ImGuiTabItemFlags.NoCloseWithMiddleMouseButton;

                if (tab.Identifier == this.selectTabIdentifier)
                {
                    flags |= ImGuiTabItemFlags.SetSelected;
                }

                if (!ImGuiEx.BeginTabItem(tab.Title, flags))
                {
                    tab.Tab.OnNotDraw();
                    continue;
                }

                if ((flags & ImGuiTabItemFlags.SetSelected) != 0)
                {
                    this.selectTabIdentifier = null;
                }

                this.lastSelectedTabTitle = tab.Tab.Title;

                ImGui.PushID(tab.Identifier);

                AthavarPluginException.CatchCrash(() => tab.Tab.Draw());

                ImGui.PopID();

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private record TabDefinition(ITab Tab, string Title, string Identifier);
}