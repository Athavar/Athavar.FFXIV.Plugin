// <copyright file="TabBarHandler.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.UI;

using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using ImGuiNET;

/// <summary>
///     Handler for ImGui tabs.
/// </summary>
public sealed class TabBarHandler
{
    private readonly IPluginLogger logger;
    private readonly List<TabDefinition> tabs = new();
    private readonly string name;
    private readonly object lockObject = new();

    private string lastSelectedTabTitle = string.Empty;
    private string? selectTabIdentifier;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TabBarHandler"/> class.
    /// </summary>
    /// <param name="logger"><see cref="IPluginLogger"/> to log events.</param>
    /// <param name="name">The name of the <see cref="TabBarHandler"/>.</param>
    public TabBarHandler(IPluginLogger logger, string name)
    {
        this.logger = logger;
        this.name = $"##{name}";
    }

    /// <summary>
    ///     Add a tab.
    /// </summary>
    /// <param name="tab">The tab.</param>
    /// <returns>return for chaining.</returns>
    public TabBarHandler Add(ITab tab)
    {
        lock (this.lockObject)
        {
            if (this.tabs.All(t => t.Identifier != tab.Identifier))
            {
                this.tabs.Add(new TabDefinition(tab, $"{tab.Name}##{tab.Identifier}", tab.Identifier));
            }
        }

        return this;
    }

    public string GetTabTitle() => this.lastSelectedTabTitle;

    public void SelectTab(string identifier) => this.selectTabIdentifier = identifier;

    public void SetEnableState(string identifier, bool enable)
    {
        lock (this.lockObject)
        {
            var tab = this.tabs.SingleOrDefault(t => t.Identifier == identifier)?.Tab;
            if (tab != null)
            {
                tab.Enabled = enable;
            }
        }
    }

    public void Sort(int index)
    {
        lock (this.lockObject)
        {
            this.tabs.Sort(index, this.tabs.Count - index, null);
        }
    }

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
                if (!tab.Tab.Enabled)
                {
                    continue;
                }

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

                AthavarPluginException.CatchCrash(this.logger, () => tab.Tab.Draw());

                ImGui.PopID();

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private record TabDefinition(ITab Tab, string Title, string Identifier) : IComparable<TabDefinition>, IComparable
    {
        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            return obj is TabDefinition other ? this.CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(TabDefinition)}");
        }

        public int CompareTo(TabDefinition? other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            return string.Compare(this.Identifier, other.Identifier, StringComparison.Ordinal);
        }
    }
}