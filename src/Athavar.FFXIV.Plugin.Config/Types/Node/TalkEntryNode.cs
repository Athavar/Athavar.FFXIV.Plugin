﻿// <copyright file="TalkEntryNode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

/// <summary>
///     Text entry node type.
/// </summary>
public sealed class TalkEntryNode : Node
{
    /// <summary>
    ///     Gets a value indicating whether the matching target text is a regex.
    /// </summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public bool TargetIsRegex => this.TargetText.StartsWith("/") && this.TargetText.EndsWith("/");

    /// <summary>
    ///     Gets the matching target text as a compiled regex.
    /// </summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public Regex? TargetRegex
    {
        get
        {
            try
            {
                return new Regex(this.TargetText.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch
            {
                return null;
            }
        }
    }

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
        get => this.TargetText;

        set { }
    }

    /// <summary>
    ///     Gets or sets the matching target name.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("TargetText")]
    public string TargetText { get; set; } = string.Empty;
}