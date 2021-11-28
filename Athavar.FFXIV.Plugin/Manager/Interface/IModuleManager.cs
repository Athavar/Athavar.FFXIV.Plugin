namespace Athavar.FFXIV.Plugin.Manager.Interface;

using System.Collections.Generic;

internal interface IModuleManager
{
    bool Register(IModule module, bool enableState);


    public IEnumerable<string> GetModuleNames();

    public bool IsEnables(string moduleName);

    public void Enable(string moduleName, bool state = true);

    /// <summary>
    ///     Draw all module tabs.
    /// </summary>
    void Draw();
}