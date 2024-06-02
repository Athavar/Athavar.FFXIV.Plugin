namespace Athavar.FFXIV.Plugin.Common.DalamudWrapper;

using System.Reflection;
using Athavar.FFXIV.Plugin.Models.DalamudLike;
using Dalamud.Plugin;

/// <summary>
///     Wrapper around the internal LocalPlugin of Dalamud.
/// </summary>
internal sealed class LocalPluginWrapper : LocalPlugin
{
    private static Type? localPluginType;
    private readonly dynamic instance;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LocalPluginWrapper"/> class.
    /// </summary>
    /// <param name="instance">The un-accessible LocalPlugin object.</param>
    internal LocalPluginWrapper(dynamic instance) => this.instance = instance;

    /// <summary>
    ///     Gets the name of the plugin.
    /// </summary>
    public override string Name => (string)(Type.GetProperty("Name")?.GetValue(this.instance) ?? string.Empty);

    /// <summary>
    ///     Gets the plugin instance.
    /// </summary>
    public override IDalamudPlugin? Instance => (IDalamudPlugin?)Type.GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField)?.GetValue(this.instance);

    /// <summary>
    ///     Gets the assembly name of the plugin.
    /// </summary>
    public override AssemblyName? AssemblyName => (AssemblyName?)Type.GetField("AssemblyName")?.GetValue(this.instance);

    /// <summary>
    ///     Gets the state of the plugin.
    /// </summary>
    public override PluginState State => (PluginState)(Type.GetField("State")?.GetValue(this.instance) ?? PluginState.DependencyResolutionFailed);

    private static Type Type => localPluginType ??= DalamudServiceWrapper.GetDalamudAssembly().GetType("Dalamud.Plugin.Internal.Types.LocalPlugin") ?? throw new Exception("Fail to find Type Dalamud.Plugin.Internal.Types.LocalPlugin");
}