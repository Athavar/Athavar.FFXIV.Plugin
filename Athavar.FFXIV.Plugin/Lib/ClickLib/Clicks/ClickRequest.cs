﻿using FFXIVClientStructs.FFXIV.Client.UI;

namespace ClickLib.Clicks
{
    internal sealed class ClickRequest : ClickBase
    {
        protected override string Name => "Request";
        protected override string AddonName => "Request";

        public unsafe ClickRequest() : base()
        {
            AvailableClicks["request_hand_over"] = (addon) => SendClick(addon, EventType.CHANGE, 0, ((AddonRequest*)addon)->HandOverButton);
            AvailableClicks["request_cancel"] = (addon) => SendClick(addon, EventType.CHANGE, 1, ((AddonRequest*)addon)->CancelButton);
            AvailableClicks["request_select_first_item"] = (addon) => SendClick(addon, EventType.CHANGE, 6, ((AddonRequest*)addon)->AtkComponentDragDrop270);
        }
    }
}
