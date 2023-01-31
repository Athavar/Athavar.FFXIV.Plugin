// <copyright file="ConcreteNodeConverter.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
///     Converts INodes to MacroNodes, FolderNodes, TextEntryNode or TextFolderNode.
/// </summary>
public class ConcreteNodeConverter : JsonConverter
{
    /// <inheritdoc />
    public override bool CanRead => true;

    /// <inheritdoc />
    public override bool CanWrite => false;

    /// <inheritdoc />
    public override bool CanConvert(Type objectType) => objectType == typeof(INode);

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        var jType = jObject["$type"]?.Value<string>();

        if (jType == this.SimpleName(typeof(TextEntryNode)))
        {
            return this.CreateObject<TextEntryNode>(jObject, serializer);
        }

        if (jType == this.SimpleName(typeof(ListEntryNode)))
        {
            return this.CreateObject<ListEntryNode>(jObject, serializer);
        }

        if (jType == this.SimpleName(typeof(TalkEntryNode)))
        {
            return this.CreateObject<TalkEntryNode>(jObject, serializer);
        }

        if (jType == this.SimpleName(typeof(TextFolderNode)))
        {
            return this.CreateObject<TextFolderNode>(jObject, serializer);
        }

        if (jType == this.SimpleName(typeof(MacroNode)))
        {
            return this.CreateObject<MacroNode>(jObject, serializer);
        }

        if (jType == this.SimpleName(typeof(RotationNode)))
        {
            return this.CreateObject<RotationNode>(jObject, serializer);
        }

        if (jType == this.SimpleName(typeof(FolderNode)))
        {
            return this.CreateObject<FolderNode>(jObject, serializer);
        }

        throw new NotSupportedException($"Node type \"{jType}\" is not supported.");
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new JsonException("Write is disabled");

    private T CreateObject<T>(JObject jObject, JsonSerializer serializer)
        where T : new()
    {
        var obj = new T();
        serializer.Populate(jObject.CreateReader(), obj);
        return obj;
    }

    private string SimpleName(Type type) => $"{type.FullName}, {type.Assembly.GetName().Name}";
}