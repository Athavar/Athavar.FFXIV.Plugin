namespace Athavar.FFXIV.Plugin
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class TalkFolderNode : INode
    {
        public string Name { get; set; } = string.Empty;

        [JsonProperty(ItemConverterType = typeof(ConcreteNodeConverter))]
        public List<INode> Children { get; } = new();
    }
}
