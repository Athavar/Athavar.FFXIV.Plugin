// <copyright file="InviterConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dalamud.Game.Text;

    internal class InviterConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InviterConfiguration"/> class.
        /// </summary>
        public InviterConfiguration()
        {
        }

        /// <summary>
        /// Init the configuration with default values.
        /// </summary>
        public void Init()
        {
            this.HiddenChatType ??= new List<XivChatType> {
                XivChatType.None,
                XivChatType.CustomEmote,
                XivChatType.StandardEmote,
                XivChatType.SystemMessage,
                XivChatType.SystemError,
                XivChatType.GatheringSystemMessage,
                XivChatType.ErrorMessage,
                XivChatType.RetainerSale,

                XivChatType.TellOutgoing,
                XivChatType.Party,
                XivChatType.Alliance,
                XivChatType.PvPTeam,
            };

            this.FilteredChannels ??= new List<XivChatType>(Enum.GetValues<XivChatType>().Except(this.HiddenChatType));
        }

        public bool Enable = false;

        public string TextPattern = "inv";

        public bool RegexMatch = false;

        public bool PrintMessage = false;

        public bool PrintError = true;

        public int Delay = 200;

        public List<XivChatType>? HiddenChatType = null!;

        public List<XivChatType>? FilteredChannels = null!;
    }
}