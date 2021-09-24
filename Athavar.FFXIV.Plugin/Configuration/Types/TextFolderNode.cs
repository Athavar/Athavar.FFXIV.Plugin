using Newtonsoft.Json;
using System.Collections.Generic;

namespace Athavar.FFXIV.Plugin
{
    public class TextFolderNode : INode
    {
        public string Name { get; set; } = string.Empty;

        [JsonProperty(ItemConverterType = typeof(ConcreteNodeConverter))]
        public List<INode> Children { get; } = new();
    }
}
