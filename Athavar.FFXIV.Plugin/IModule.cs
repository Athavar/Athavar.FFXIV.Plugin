// <copyright file="IModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin
{
    using System;

    internal interface IModule : IDisposable
    {
        void Draw();
    }
}
