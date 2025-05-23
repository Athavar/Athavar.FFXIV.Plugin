// <copyright file="BaseConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

[JsonDerivedType(typeof(BarConfig), "BarConfig")]
[JsonDerivedType(typeof(BarColorsConfig), "BarColorConfig")]
[JsonDerivedType(typeof(GeneralConfig), "GeneralConfig")]
[JsonDerivedType(typeof(HeaderConfig), "HeaderConfig")]
[JsonDerivedType(typeof(VisibilityConfig), "VisibilityConfig")]
[JsonDerivedType(typeof(MeterConfig), "MeterConfig")]
public abstract class BaseConfig : IConfig
{
    private static JsonSerializerOptions options = new()
    {
        IncludeFields = true,
    };

    public static BaseConfig? GetFromImportString(string importString)
    {
        if (string.IsNullOrEmpty(importString))
        {
            return null;
        }

        try
        {
            var bytes = Convert.FromBase64String(importString);

            using var inputStream = new MemoryStream(bytes);
            using var compressionStream = new DeflateStream(inputStream, CompressionMode.Decompress);
            using var reader = new StreamReader(compressionStream, Encoding.UTF8);
            var decodedJsonString = reader.ReadToEnd();

            var importedObj = JsonSerializer.Deserialize<BaseConfig>(decodedJsonString, options);
            return importedObj;
        }
        catch (Exception ex)
        {
            // TODO: think about better way to log error.
            // PluginLog.Error(ex.ToString());
        }

        return null;
    }

    public string? GetExportString()
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(this, options);

            using var outputStream = new MemoryStream();
            using var compressionStream = new DeflateStream(outputStream, CompressionLevel.Optimal);
            using var writer = new StreamWriter(compressionStream, Encoding.UTF8);
            writer.Write(jsonString);
            writer.Flush();

            return Convert.ToBase64String(outputStream.ToArray());
        }
        catch (Exception ex)
        {
            // TODO: think about better way to log error.
            // PluginLog.Error(ex.ToString());
        }

        return null;
    }
}