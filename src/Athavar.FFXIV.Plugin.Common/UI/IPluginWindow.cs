// <copyright file="IPluginWindow.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin;

public interface IPluginWindow
{
    bool IsOpen { get; set; }

    void Toggle();

    void SelectTab(string tabIdentifier);
}