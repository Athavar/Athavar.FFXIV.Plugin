namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

public sealed unsafe class ClickRetainerItemTransferList : ClickBase<ClickRetainerItemTransferList, AddonRetainerItemTransferList>
{
    public ClickRetainerItemTransferList(IntPtr addon = default)
        : base("RetainerItemTransferList", addon)
    {
    }

    public static implicit operator ClickRetainerItemTransferList(IntPtr addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickRetainerItemTransferList Using(IntPtr addon) => new(addon);

    /// <summary>
    ///     Click the confirm button.
    /// </summary>
    [ClickName("retainer_transfer_ask_confirm")]
    public void Confirm() => this.ClickAddonButton(this.Addon->ConfirmButton, 0);
}