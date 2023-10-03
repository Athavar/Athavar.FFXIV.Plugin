// <copyright file="TextEntryNode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace

namespace Athavar.FFXIV.Plugin;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Config.Interfaces;

/// <summary>
///     Text entry node type.
/// </summary>
public sealed class TextEntryNode : Node
{
    [JsonIgnore]
    private string text = string.Empty;

    [JsonIgnore]
    private string zoneText = string.Empty;

    public TextEntryNode()
    {
        this.UpdateTextRegex();
        this.UpdateZoneRegex();
    }

    /// <summary>
    ///     Gets a value indicating whether the matching text is a regex.
    /// </summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public bool IsTextRegex => this.Text.StartsWith("/") && this.Text.EndsWith("/");

    /// <summary>
    ///     Gets a value indicating whether the matching zone text is a regex.
    /// </summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public bool ZoneIsRegex => this.ZoneText.StartsWith("/") && this.ZoneText.EndsWith("/");

    /// <summary>
    ///     Gets the matching text as a compiled regex.
    /// </summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public Lazy<IRegex?> TextRegex { get; private set; }

    /// <summary>
    ///     Gets the matching zone text as a compiled regex.
    /// </summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public Lazy<IRegex?> ZoneRegex { get; private set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the node is enabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     Gets or sets the name of the node.
    /// </summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public override string Name
    {
        get
            => !string.IsNullOrEmpty(this.ZoneText)
                ? $"({this.ZoneText}) {this.Text}"
                : this.Text;

        set { }
    }

    /// <summary>
    ///     Gets or sets the matching text.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Text")]
    public string Text
    {
        get => this.text;
        set
        {
            this.text = value;
            this.UpdateTextRegex();
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether this entry should be zone restricted.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("ZoneRestricted")]
    public bool ZoneRestricted { get; set; }

    /// <summary>
    ///     Gets or sets the matching zone text.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("ZoneText")]
    public string ZoneText
    {
        get => this.zoneText;
        set
        {
            this.zoneText = value;
            this.UpdateZoneRegex();
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether yes should be pressed instead of no.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("IsYes")]
    public bool IsYes { get; set; } = true;

    [MemberNotNull(nameof(TextRegex))]
    private void UpdateTextRegex()
        => this.TextRegex = this.IsTextRegex
            ? new Lazy<IRegex?>(() =>
            {
                try
                {
                    return new RegexWrapper(this.Text.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
                catch
                {
                    return null;
                }
            })
            : new Lazy<IRegex?>();

    [MemberNotNull(nameof(ZoneRegex))]
    private void UpdateZoneRegex()
        => this.ZoneRegex = this.ZoneIsRegex
            ? new Lazy<IRegex?>(() =>
            {
                try
                {
                    return new RegexWrapper(this.ZoneText.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
                catch
                {
                    return null;
                }
            })
            : new Lazy<IRegex?>();
}