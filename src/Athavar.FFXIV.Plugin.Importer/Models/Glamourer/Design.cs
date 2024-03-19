// <copyright file="Design.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Importer.Models.Glamourer;

using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

internal class Design
{
    [JsonPropertyName("FileVersion")]
    public int FileVersion { get; set; } = 1;

    [JsonPropertyName("Identifier")]
    public Guid Identifier { get; set; } = Guid.NewGuid();

    [JsonPropertyName("CreationDate")]
    public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("LastEdit")]
    public DateTimeOffset LastEdit { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("Color")]
    public string Color { get; set; } = string.Empty;

    [JsonPropertyName("QuickDesign")]
    public bool QuickDesign { get; set; } = true;

    [JsonPropertyName("Tags")]
    public string[] Tags { get; set; } = Array.Empty<string>();

    [JsonPropertyName("WriteProtected")]
    public bool WriteProtected { get; set; } = false;

    [JsonPropertyName("Equipment")]
    public Equipment Equipment { get; set; } = new();

    [JsonPropertyName("Customize")]
    public Customize Customize { get; set; } = new();

    [JsonPropertyName("Parameters")]
    public Parameters Parameters { get; set; } = new();

    [JsonPropertyName("Materials")]
    public UnImplemented Materials { get; set; } = new();

    [JsonPropertyName("Mods")]
    public Collection<Mod> Mods { get; set; } = new();

    [JsonPropertyName("Links")]
    public Links Links { get; set; } = new();

    public string ToShareText()
    {
        byte[] Compress(byte[] data, byte version)
        {
            using var compressedStream = new MemoryStream();
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Compress);
            zipStream.Write(data, 0, data.Length);
            zipStream.Flush();

            var ret = new byte[compressedStream.Length + 1];
            ret[0] = version;

            var mem = compressedStream.GetBuffer().AsMemory();
            mem.CopyTo(ret.AsMemory().Slice(1));
            return ret;
        }

        byte[] CompressFromString(string data, byte version)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            return Compress(bytes, version);
        }

        var serText = JsonSerializer.Serialize(this);
        var resultData = CompressFromString(serText, 6);
        return Convert.ToBase64String(resultData);
    }
}