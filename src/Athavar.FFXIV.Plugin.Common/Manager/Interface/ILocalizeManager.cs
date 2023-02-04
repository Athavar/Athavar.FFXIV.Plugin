// <copyright file="ILocalizeManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

using Lumina.Data;

public interface ILocalizeManager
{
    void ChangeLanguage(Language language);

    string Localize(string? message);
}