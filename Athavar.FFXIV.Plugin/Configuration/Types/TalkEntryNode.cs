using System.Collections.Generic;

namespace Athavar.FFXIV.Plugin
{
    public class TalkEntryNode : INode
    {
        public bool Enabled { get; set; } = true;

        public string Name { get; set; } = string.Empty;

        public List<string> Text { get; set; } = new();

        public List<string> ReplacmentText { get; set; } = new();
    }
}
