// <copyright file="Localizer.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin
{
    internal class Localizer
    {
        public Language Language = Language.EN;

        /// <summary>
        /// Initializes a new instance of the <see cref="Localizer"/> class.
        /// </summary>
        /// <param name="language"></param>
        public Localizer(Language language = Language.EN)
        {
            this.Language = language;
        }

        public string Localize(string? message)
        {
            if (message is null)
            {
                return "NullText";
            }

            return message;
        }
    }
}
