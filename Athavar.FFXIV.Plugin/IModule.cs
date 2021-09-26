using System;

namespace Athavar.FFXIV.Plugin
{
    internal interface IModule : IDisposable
    {
        void Draw();
    }
}
