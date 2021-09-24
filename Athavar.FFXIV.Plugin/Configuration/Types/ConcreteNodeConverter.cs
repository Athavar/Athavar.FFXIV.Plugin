using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Athavar.FFXIV.Plugin
{
    public class ConcreteNodeConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override bool CanConvert(Type objectType) => objectType == typeof(INode);

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var jType = jObject["$type"]?.Value<string>();


            if (jType == SimpleName(typeof(TextEntryNode)))
                return CreateObject<TextEntryNode>(jObject, serializer);
            else if (jType == SimpleName(typeof(TextFolderNode)))
                return CreateObject<TextFolderNode>(jObject, serializer);
            else if(jType == SimpleName(typeof(MacroNode)))
                return CreateObject<MacroNode>(jObject, serializer);
            else if(jType == SimpleName(typeof(FolderNode)))
                return CreateObject<FolderNode>(jObject, serializer);
            else
                throw new NotSupportedException($"Node type \"{jType}\" is not supported.");
        }

        private T CreateObject<T>(JObject jObject, JsonSerializer serializer) where T : new()
        {
            var obj = new T();
            serializer.Populate(jObject.CreateReader(), obj);
            return obj;
        }

        private static string SimpleName(Type type)
        {
            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}
