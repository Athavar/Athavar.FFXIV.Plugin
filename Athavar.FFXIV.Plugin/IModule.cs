namespace Athavar.FFXIV.Plugin
{
    using System;

    internal interface IModule : IDisposable
    {
        void Draw();
    }
}
