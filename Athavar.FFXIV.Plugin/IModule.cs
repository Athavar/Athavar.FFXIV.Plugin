// <copyright file="IModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin;

internal interface IModule
{
    string Name { get; }

    void Draw();

    void Enable(bool state = true);
}