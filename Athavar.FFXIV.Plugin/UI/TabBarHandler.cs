// <copyright file="TabHandler.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.UI;

using System;
using System.Collections.Generic;
using ImGuiNET;

/// <summary>
///     Handler for ImGui tabs.
/// </summary>
internal class TabBarHandler : IDisposable
{
    private readonly List<Tab> tabs = new();
    private readonly string name;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TabBarHandler" /> class.
    /// </summary>
    /// <param name="name">The name of the <see cref="TabBarHandler" />.</param>
    public TabBarHandler(string name) => this.name = name;

    /// <summary>
    ///     Register a tab.
    /// </summary>
    /// <param name="tab">The tab.</param>
    /// <returns>return for chaining.</returns>
    public TabBarHandler Register(Tab tab)
    {
        this.tabs.Add(tab);
        return this;
    }

    /// <summary>
    ///     Draw the TabBar.
    /// </summary>
    public void Draw()
    {
        if (ImGui.BeginTabBar(this.name))
        {
            for (var index = 0; index < this.tabs.Count; index++)
            {
                var tab = this.tabs[index];

                if (!ImGui.BeginTabItem(tab.Name))
                {
                    tab.OnNotDraw();
                    continue;
                }

                ImGui.PushID(tab.Identifier);

                tab.Draw();

                ImGui.PopID();

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        for (var index = 0; index < this.tabs.Count; index++)
        {
            var tab = this.tabs[index];
            tab.Dispose();
        }
    }
}