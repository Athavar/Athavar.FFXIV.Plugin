using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Text;

namespace Athavar.FFXIV.Plugin
{
    internal class InviterConfiguration
    {
        public InviterConfiguration()
        {
        }

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