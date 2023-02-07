// <copyright file="HistoryPage.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI.Config;

using System.Numerics;
using Athavar.FFXIV.Plugin.Config;

public class HistoryPage : IConfigPage
{
    public string Name { get; }

    public IConfig GetDefault() => new DummyConfig();

    public IConfig GetConfig() => new DummyConfig();

    public void DrawConfig(Vector2 size, float padX, float padY)
    {
    }
}