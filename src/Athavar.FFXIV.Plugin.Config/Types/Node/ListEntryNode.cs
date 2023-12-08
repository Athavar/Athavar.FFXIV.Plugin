// <copyright file="ListEntryNode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace

namespace Athavar.FFXIV.Plugin;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Models.Interfaces;

/// <summary>
///     List entry node type.
/// </summary>
public sealed class ListEntryNode : Node
{
    [JsonIgnore]
    private string text = string.Empty;

    [JsonIgnore]
    private string targetText = string.Empty;

    public ListEntryNode()
    {
        this.UpdateTextRegex();
        this.UpdateTargetRegex();
    }

    /// <summary>
    ///     Gets a value indicating whether the matching text is a regex.
    /// </summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public bool IsTextRegex => this.Text.StartsWith("/") && this.Text.EndsWith("/");

    /// <summary>
    ///     Gets a value indicating whether the matching target text is a regex.
    /// </summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public bool TargetIsRegex => this.TargetText.StartsWith("/") && this.TargetText.EndsWith("/");

    /// <summary>
    ///     Gets the matching text as a compiled regex.
    /// </summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public Lazy<IRegex?> TextRegex { get; private set; }

    /// <summary>
    ///     Gets the matching target text as a compiled regex.
    /// </summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public Lazy<IRegex?> TargetRegex { get; private set; }

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
            => !string.IsNullOrEmpty(this.TargetText)
                ? $"({this.TargetText}) {this.Text}"
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
    ///     Gets or sets a value indicating whether this entry should be target restricted.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("TargetRestricted")]
    public bool TargetRestricted { get; set; }

    /// <summary>
    ///     Gets or sets the matching target name.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("TargetText")]
    public string TargetText
    {
        get => this.targetText;
        set
        {
            this.targetText = value;
            this.UpdateTargetRegex();
        }
    }

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

    [MemberNotNull(nameof(TargetRegex))]
    private void UpdateTargetRegex()
        => this.TargetRegex = this.TargetIsRegex
            ? new Lazy<IRegex?>(() =>
            {
                try
                {
                    return new RegexWrapper(this.TargetText.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
                catch
                {
                    return null;
                }
            })
            : new Lazy<IRegex?>();
}