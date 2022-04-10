namespace Athavar.FFXIV.Plugin.Module.ItemInspector;

using Athavar.FFXIV.Plugin.Manager;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Module.HuntLink;

internal class ItemInspectorModule : IModule
{
    private const string ModuleName = "ItemInspector";
    private readonly ItemInspectorTab tab;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ItemInspectorModule" /> class.
    /// </summary>
    /// <param name="moduleManager"><see cref="ModuleManager" /> added by DI.</param>
    /// <param name="huntLinkTab"><see cref="HuntLinkTab" /> added by DI.</param>
    public ItemInspectorModule(IModuleManager moduleManager, ItemInspectorTab huntLinkTab)
    {
        this.tab = huntLinkTab;
        moduleManager.Register(this, false);
    }

    /// <inheritdoc />
    public string Name => ModuleName;

    /// <inheritdoc />
    public void Draw() => this.tab.DrawTab();

    /// <inheritdoc />
    public void Enable(bool state = true) => _ = state;
}