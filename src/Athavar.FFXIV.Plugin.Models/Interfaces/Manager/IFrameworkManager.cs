// <copyright file="IFrameworkManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Interfaces.Manager;

using Dalamud.Plugin.Services;

public interface IFrameworkManager
{
    public interface IRegisteredDelegation
    {
        public string Name { get; }

        public IReadOnlyList<TimeSpan> Duration { get; }
    }

    IReadOnlyList<IRegisteredDelegation> RegisteredDelegations { get; }

    void Subscribe(IFramework.OnUpdateDelegate updateDelegate);

    void Unsubscribe(IFramework.OnUpdateDelegate updateDelegate);
}