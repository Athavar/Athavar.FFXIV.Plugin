// <copyright file="ILocalizeManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Interfaces.Manager;

using Lumina.Data;

public interface ILocalizeManager
{
    void ChangeLanguage(Language language);

    string Localize(string? message);
}