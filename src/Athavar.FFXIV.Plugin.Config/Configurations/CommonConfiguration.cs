// <copyright file="CommonConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Config;

using System.Text.Json.Serialization;
using Dalamud.Game.Text;
using Lumina.Data;

public sealed class CommonConfiguration : BasicModuleConfig<CommonConfiguration>
{
    [JsonInclude]
    [JsonPropertyName("Version")]
    public int Version { get; set; } = 2;

    [JsonInclude]
    [JsonPropertyName("ShowToolTips")]
    public bool ShowToolTips { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("ShowLaunchButton")]
    public bool ShowLaunchButton { get; set; }

    [JsonInclude]
    [JsonPropertyName("Language")]
    public Language Language { get; set; } = Language.English;

    /// <summary>
    ///     Gets or sets the chat channel to use.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("ChatType")]
    public XivChatType ChatType { get; set; } = XivChatType.Debug;

    /// <summary>
    ///     Gets or sets the error chat channel to use.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("ErrorChatType")]
    public XivChatType ErrorChatType { get; set; } = XivChatType.Urgent;

    /// <summary>
    ///     Gets or sets a <see cref="Dictionary{TKey,TValue}"/> of <see cref="SavedDutyInfo"/>. Key is ContentId of the local
    ///     player.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("SavedDutyInfo")]
    public Dictionary<ulong, SavedDutyInfo> SavedDutyInfos { get; set; } = new();
}