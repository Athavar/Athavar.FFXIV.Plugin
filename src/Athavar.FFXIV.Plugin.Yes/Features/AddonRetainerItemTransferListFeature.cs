namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;

internal class AddonRetainerItemTransferListFeature : OnSetupFeature
{
    public AddonRetainerItemTransferListFeature(YesModule module)
        : base(module)
    {
    }

    protected override string AddonName => "RetainerItemTransferList";

    protected override bool ConfigurationEnableState => this.Configuration.RetainerTransferListConfirmEnabled;

    protected override void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
    {
        ClickRetainerItemTransferList transferList = addon;
        transferList.Confirm();
    }
}