// <copyright file="Localizer.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Manager;

using Athavar.FFXIV.Plugin.Manager.Interface;

internal class LocalizerManager : ILocalizerManager
{
    private Language language = Language.En;

    public void ChangeLanguage(Language language) => this.language = language;

    public string Localize(string? message)
    {
        if (message is null)
        {
            return "NullText";
        }

        return message;
    }
}