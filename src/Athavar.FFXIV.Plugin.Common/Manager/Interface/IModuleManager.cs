// <copyright file="IModuleManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

public interface IModuleManager
{
    public delegate void ModuleStateChange(Module module);

    event ModuleStateChange StateChange;

    public IEnumerable<string> GetModuleNames();

    public bool IsEnables(string moduleName);

    public void Enable(string moduleName, bool state = true);

    void LoadModules();
}