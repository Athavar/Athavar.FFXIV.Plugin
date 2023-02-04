// <copyright file="BaseConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dalamud.Logging;

[JsonDerivedType(typeof(BarConfig), nameof(BarConfig))]
[JsonDerivedType(typeof(BarColorsConfig), nameof(BarColorsConfig))]
[JsonDerivedType(typeof(GeneralConfig), nameof(GeneralConfig))]
[JsonDerivedType(typeof(HeaderConfig), nameof(HeaderConfig))]
[JsonDerivedType(typeof(VisibilityConfig), nameof(VisibilityConfig))]
[JsonDerivedType(typeof(MeterConfig), nameof(MeterConfig))]
public abstract class BaseConfig : IConfig
{
    public static BaseConfig? GetFromImportString(string importString)
    {
        if (string.IsNullOrEmpty(importString))
        {
            return default;
        }

        try
        {
            var bytes = Convert.FromBase64String(importString);

            using var inputStream = new MemoryStream(bytes);
            using var compressionStream = new DeflateStream(inputStream, CompressionMode.Decompress);
            using var reader = new StreamReader(compressionStream, Encoding.UTF8);
            var decodedJsonString = reader.ReadToEnd();

            var importedObj = JsonSerializer.Deserialize<BaseConfig>(decodedJsonString, new JsonSerializerOptions());
            return importedObj;
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex.ToString());
        }

        return default;
    }

    public string? GetExportString()
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(this);

            using var outputStream = new MemoryStream();
            using var compressionStream = new DeflateStream(outputStream, CompressionLevel.Optimal);
            using var writer = new StreamWriter(compressionStream, Encoding.UTF8);
            writer.Write(jsonString);

            return Convert.ToBase64String(outputStream.ToArray());
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex.ToString());
        }

        return null;
    }
}