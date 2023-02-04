// <copyright file="LocalizeManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Lumina.Data;

internal class LocalizeManager : ILocalizeManager
{
    private Language language = Language.English;

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