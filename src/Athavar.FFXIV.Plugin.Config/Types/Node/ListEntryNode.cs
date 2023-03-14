// <copyright file="ListEntryNode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

using System.Text.RegularExpressions;
using Newtonsoft.Json;

/// <summary>
///     List entry node type.
/// </summary>
public sealed class ListEntryNode : INode
{
    /// <summary>
    ///     Gets a value indicating whether the matching text is a regex.
    /// </summary>
    [JsonIgnore]
    public bool IsTextRegex => this.Text.StartsWith("/") && this.Text.EndsWith("/");

    /// <summary>
    ///     Gets the matching text as a compiled regex.
    /// </summary>
    [JsonIgnore]
    public Regex? TextRegex
    {
        get
        {
            try
            {
                return new Regex(this.Text.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the matching target text is a regex.
    /// </summary>
    [JsonIgnore]
    public bool TargetIsRegex => this.TargetText.StartsWith("/") && this.TargetText.EndsWith("/");

    /// <summary>
    ///     Gets the matching target text as a compiled regex.
    /// </summary>
    [JsonIgnore]
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
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     Gets or sets the name of the node.
    /// </summary>
    [JsonIgnore]
    public string Name
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
    public string Text { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether this entry should be target restricted.
    /// </summary>
    public bool TargetRestricted { get; set; } = false;

    /// <summary>
    ///     Gets or sets the matching target name.
    /// </summary>
    public string TargetText { get; set; } = string.Empty;
}