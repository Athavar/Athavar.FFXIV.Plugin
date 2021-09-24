using Inviter;
using SomethingNeedDoing;
using System;
using YesAlready;

namespace Athavar.FFXIV.Plugin
{
    internal class Modules
    {
        public static void Initialize()
        {
            Configuration = Configuration.Load();
            Localizer = new Localizer();
            MacroModule = new(Configuration.Macro);
            YesModule = new(Configuration.Yes);
            InviterModule = new(Configuration, Localizer);
        }

        public static void Dispose()
        {
            YesModule.Dispose();
            MacroModule.Dispose();
            InviterModule.Dispose();
        }

        public static Configuration Configuration { get; private set; } = null!;
        public static Localizer Localizer { get; private set; } = null!;

        public static YesModule YesModule { get; private set; } = null!;
        public static MacroModule MacroModule { get; private set; } = null!;
        public static InviterModule InviterModule { get; private set; } = null!;
    }
}
