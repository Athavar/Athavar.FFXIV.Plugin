// <copyright file="CraftQueueModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.CraftQueue;

using System;
using Athavar.FFXIV.Plugin.Manager.Interface;

internal class CraftQueueModule : IModule
{
    private const string ModuleName = "CraftQueue";

    public CraftQueueModule(IModuleManager moduleManager) => moduleManager.Register(this, true);

    public string Name => ModuleName;

    public bool Hidden { get; }

    public void Draw() => throw new NotImplementedException();

    public void Enable(bool state = true) => throw new NotImplementedException();
}