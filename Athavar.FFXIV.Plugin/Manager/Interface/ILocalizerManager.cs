// <copyright file="ILocalizerManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Manager.Interface;

internal interface ILocalizerManager
{
    void ChangeLanguage(Language language);

    string Localize(string? message);
}