namespace Athavar.FFXIV.Plugin.Common.Manager;

using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Plugin.Services;

internal class PluginMonitorService : IDisposable, IPluginMonitorService
{
    private readonly IDalamudServices dalamudServices;
    private readonly IFrameworkManager frameworkManager;

    private readonly Dictionary<string, Version> loadedPlugins = [];

    public PluginMonitorService(IDalamudServices dalamudServices, IFrameworkManager frameworkManager)
    {
        this.dalamudServices = dalamudServices;
        this.frameworkManager = frameworkManager;

        this.frameworkManager.Subscribe(this.FrameworkUpdate);
    }

    public event IPluginMonitorService.PluginLoadingStateChanged? LoadingStateHasChanged;

    public bool IsLoaded(string name) => this.loadedPlugins.ContainsKey(name);

    /// <inheritdoc/>
    public void Dispose()
    {
        this.frameworkManager.Unsubscribe(this.FrameworkUpdate);
    }

    private void FrameworkUpdate(IFramework framework)
    {
        Dictionary<string, Version> tmp = new(this.loadedPlugins);
        foreach (var exposedPlugin in this.dalamudServices.PluginInterface.InstalledPlugins)
        {
            var name = exposedPlugin.InternalName;
            if (exposedPlugin.IsLoaded)
            {
                if (!tmp.TryGetValue(name, out var version))
                {
                    this.loadedPlugins.Add(name, exposedPlugin.Version);
                    this.LoadingStateHasChanged?.Invoke(name, true, exposedPlugin);
                    continue;
                }

                if (exposedPlugin.Version != version)
                {
                    this.loadedPlugins[name] = exposedPlugin.Version;
                    this.LoadingStateHasChanged?.Invoke(name, false, exposedPlugin);
                    this.LoadingStateHasChanged?.Invoke(name, true, exposedPlugin);
                }
            }
            else
            {
                if (tmp.TryGetValue(name, out _))
                {
                    this.loadedPlugins.Remove(name);
                    this.LoadingStateHasChanged?.Invoke(name, false, exposedPlugin);
                }
            }
        }

        foreach (var (name, _) in tmp)
        {
            this.LoadingStateHasChanged?.Invoke(name, false, null);
        }
    }
}