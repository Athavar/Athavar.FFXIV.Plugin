// <copyright file="TabBarHandler.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.UI;

using Athavar.FFXIV.Plugin.Common.Exceptions;
using ImGuiNET;

/// <summary>
///     Handler for ImGui tabs.
/// </summary>
public class TabBarHandler
{
    private readonly List<TabDefinition> tabs = new();
    private readonly string name;
    private readonly object lockObject = new();

    private string lastSelectedTabTitle = string.Empty;

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
            TabDefinition[] tabs;
            lock (this.lockObject)
            {
                tabs = this.tabs.ToArray();
            }

            for (var index = 0; index < tabs.Length; index++)
            {
                var tab = tabs[index];

                if (!ImGui.BeginTabItem(tab.Title))
                {
                    tab.Tab.OnNotDraw();
                    continue;
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