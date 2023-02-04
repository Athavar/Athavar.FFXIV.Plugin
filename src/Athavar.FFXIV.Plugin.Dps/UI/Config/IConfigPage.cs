// <copyright file="IConfigPage.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI.Config;

using System.Numerics;
using Athavar.FFXIV.Plugin.Config;

internal interface IConfigPage
{
    string Name { get; }

    IConfig GetDefault();

    IConfig GetConfig();

    void DrawConfig(Vector2 size, float padX, float padY);
}