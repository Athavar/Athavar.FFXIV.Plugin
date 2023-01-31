// <copyright file="IModuleManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

public interface IModuleManager
{
    public delegate void ModuleStateChange(Module module);

    event ModuleStateChange StateChange;

    bool Register<T>()
        where T : Module;

    public IEnumerable<string> GetModuleNames();

    public bool IsEnables(string moduleName);

    public void Enable(string moduleName, bool state = true);
}