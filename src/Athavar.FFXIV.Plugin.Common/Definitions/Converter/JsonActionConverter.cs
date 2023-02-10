// <copyright file="JsonActionConverter.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Definitions.Converter;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Common.Exceptions;

internal class JsonActionConverter : JsonConverter<Dictionary<uint, Action>>
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Dictionary<uint, Action>);

    public override void Write(Utf8JsonWriter writer, Dictionary<uint, Action> value, JsonSerializerOptions options) => throw new NotImplementedException();

    public override Dictionary<uint, Action> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dictionary = new Dictionary<uint, Action>();
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            reader.Read();
        }

        while (reader.TokenType == JsonTokenType.StartObject)
        {
            var action = new Action();
            if (!reader.Read())
            {
                throw new JsonParseException("Cannot read StartObject token.");
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonParseException("PropertyName token not found after StartObject.");
            }

            uint result;
            if (!uint.TryParse(reader.GetString(), NumberStyles.HexNumber, null, out result))
            {
                throw new JsonParseException("Cannot read action id.");
            }

            action.Id = result;
            action.Name = reader.Read() ? reader.GetString() ?? string.Empty : throw new JsonParseException("Unexpected token {0} after reading the actionId");
            if (!reader.Read())
            {
                throw new JsonParseException($"Unexpected token {reader.TokenType} after reading name.");
            }

            DamageEntry[]? damageEntry = null;
            HealEntry[]? healEntry = null;

            if (reader.GetString() == "damage")
            {
                damageEntry = reader.Read() ? JsonSerializer.Deserialize<DamageEntry[]>(ref reader, options) : throw new JsonParseException($"Unexpected token {reader.TokenType} after reading damage node.");
                if (reader.TokenType != JsonTokenType.EndArray || !reader.Read())
                {
                    throw new JsonParseException($"Unexpected token {reader.TokenType} when reading EndArray.");
                }
            }

            if (reader.TokenType == JsonTokenType.PropertyName && reader.GetString() == "heal")
            {
                healEntry = reader.Read() ? JsonSerializer.Deserialize<HealEntry[]>(ref reader, options) : throw new JsonParseException($"Unexpected token {reader.TokenType} after reading heal node.");
                if (reader.TokenType != JsonTokenType.EndArray || !reader.Read())
                {
                    throw new JsonParseException($"Unexpected token {reader.TokenType} when reading EndArray.");
                }
            }

            if (reader.TokenType != JsonTokenType.EndObject || !reader.Read())
            {
                throw new JsonParseException($"Unexpected token {reader.TokenType} when reading EndObject.");
            }

            action.Damage = damageEntry ?? Array.Empty<DamageEntry>();
            action.Heal = healEntry ?? Array.Empty<HealEntry>();

            dictionary.Add(action.Id, action);
        }

        return dictionary;
    }
}