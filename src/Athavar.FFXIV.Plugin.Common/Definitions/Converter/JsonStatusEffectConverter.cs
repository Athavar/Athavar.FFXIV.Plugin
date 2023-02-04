// <copyright file="JsonStatusEffectConverter.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Definitions.Converter;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Common.Exceptions;

internal class JsonStatusEffectConverter : JsonConverter<Dictionary<uint, StatusEffect>>
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Dictionary<uint, StatusEffect>);

    public override void Write(Utf8JsonWriter writer, Dictionary<uint, StatusEffect> value, JsonSerializerOptions options) => throw new NotImplementedException();

    public override Dictionary<uint, StatusEffect> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dictionary = new Dictionary<uint, StatusEffect>();
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            reader.Read();
        }

        while (reader.TokenType == JsonTokenType.StartObject)
        {
            var statusEffect = new StatusEffect();
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
                throw new JsonParseException("Cannot read status id.");
            }

            statusEffect.Id = result;
            statusEffect.Name = reader.Read() ? reader.GetString() ?? string.Empty : throw new JsonParseException("Unexpected token {0} after reading the actionId");
            if (!reader.Read())
            {
                throw new JsonParseException($"Unexpected token {reader.TokenType} after reading name.");
            }

            while (reader.TokenType == JsonTokenType.PropertyName)
            {
                switch (reader.GetString())
                {
                    case "potency":
                        statusEffect.PotencyEffects = reader.Read() ? JsonSerializer.Deserialize<Potency[]>(ref reader, options) : throw new JsonParseException($"Unexpected token {reader.TokenType} after reading potency node.");
                        if (reader.TokenType != JsonTokenType.EndArray || !reader.Read())
                        {
                            throw new JsonParseException($"Unexpected token {reader.TokenType} when reading EndArray.");
                        }

                        break;
                    case "timeproc":
                        statusEffect.TimeProc = reader.Read() ? JsonSerializer.Deserialize<TimeProc>(ref reader, options) : throw new JsonParseException($"Unexpected token {reader.TokenType} after reading timeproc node.");
                        if (reader.TokenType != JsonTokenType.EndObject || !reader.Read())
                        {
                            throw new JsonParseException($"Unexpected token {reader.TokenType} when reading EndArray.");
                        }

                        break;
                    case "multiplier":
                        statusEffect.Multipliers = reader.Read() ? JsonSerializer.Deserialize<Multiplier[]>(ref reader, options) : throw new JsonParseException($"Unexpected token {reader.TokenType} after reading multiplier node.");
                        if (reader.TokenType != JsonTokenType.EndArray || !reader.Read())
                        {
                            throw new JsonParseException($"Unexpected token {reader.TokenType} when reading EndArray.");
                        }

                        break;
                    case "reactiveproc":
                        statusEffect.ReactiveProc = reader.Read() ? JsonSerializer.Deserialize<ReactiveProc>(ref reader, options) : throw new JsonParseException($"Unexpected token {reader.TokenType} after reading reactiveproc node.");
                        if (reader.TokenType != JsonTokenType.EndObject || !reader.Read())
                        {
                            throw new JsonParseException($"Unexpected token {reader.TokenType} when reading EndArray.");
                        }

                        break;
                    case "damageshield":
                        statusEffect.DamageShield = reader.Read() ? JsonSerializer.Deserialize<DamageShield>(ref reader, options) : throw new JsonParseException($"Unexpected token {reader.TokenType} after reading damageshield node.");
                        if (reader.TokenType != JsonTokenType.EndObject || !reader.Read())
                        {
                            throw new JsonParseException($"Unexpected token {reader.TokenType} when reading EndArray.");
                        }

                        break;
                }
            }

            if (reader.TokenType != JsonTokenType.EndObject || !reader.Read())
            {
                throw new JsonParseException($"Unexpected token {reader.TokenType} when reading EndObject.");
            }

            statusEffect.PotencyEffects ??= Array.Empty<Potency>();

            statusEffect.Multipliers ??= Array.Empty<Multiplier>();

            dictionary.Add(statusEffect.Id, statusEffect);
        }

        return dictionary;
    }
}