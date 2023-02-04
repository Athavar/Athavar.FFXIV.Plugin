// <copyright file="IPluginWindow.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin;

public interface IPluginWindow
{
    bool IsOpen { get; set; }

    void Toggle();
}