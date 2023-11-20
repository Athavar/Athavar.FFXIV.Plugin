// <copyright file="LocalizeManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Lumina.Data;

internal sealed class LocalizeManager : ILocalizeManager
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