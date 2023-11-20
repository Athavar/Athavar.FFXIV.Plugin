// <copyright file="IModuleManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Interfaces.Manager;

public interface IModuleManager
{
    public delegate void ModuleStateChange(Module module, IModuleData data);

    event ModuleStateChange StateChange;

    public interface IModuleData
    {
        string Name { get; }

        bool HasTab { get; }

        bool Enabled { get; set; }

        bool TabEnabled { get; set; }
    }

    public IEnumerable<string> GetModuleNames();

    public IEnumerable<IModuleData> GetModuleData();

    void LoadModules();
}