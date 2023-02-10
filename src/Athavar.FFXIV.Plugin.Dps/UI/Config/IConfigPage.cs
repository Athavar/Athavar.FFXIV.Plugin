// <copyright file="IConfigPage.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
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