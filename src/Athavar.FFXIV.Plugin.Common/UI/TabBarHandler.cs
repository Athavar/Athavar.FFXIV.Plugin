// <copyright file="TabHandler.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.UI;

using ImGuiNET;

/// <summary>
///     Handler for ImGui tabs.
/// </summary>
public class TabBarHandler
{
    private readonly List<ITab> tabs = new();
    private readonly string name;
    private readonly object lockObject = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="TabBarHandler" /> class.
    /// </summary>
    /// <param name="name">The name of the <see cref="TabBarHandler" />.</param>
    public TabBarHandler(string name) => this.name = name;

    /// <summary>
    ///     Add a tab.
    /// </summary>
    /// <param name="tab">The tab.</param>
    /// <returns>return for chaining.</returns>
    public TabBarHandler Add(ITab tab)
    {
        lock (this.lockObject)
        {
            this.tabs.Add(tab);
        }

        return this;
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
            this.tabs.Remove(tab);
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
            ITab[] tabs;
            lock (this.lockObject)
            {
                tabs = this.tabs.ToArray();
            }

            for (var index = 0; index < tabs.Length; index++)
            {
                var tab = tabs[index];

                if (!ImGui.BeginTabItem(tab.Name))
                {
                    tab.OnNotDraw();
                    continue;
                }

                ImGui.PushID(tab.Identifier);

                AthavarPluginException.CatchCrash(() => tab.Draw());

                ImGui.PopID();

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }
}