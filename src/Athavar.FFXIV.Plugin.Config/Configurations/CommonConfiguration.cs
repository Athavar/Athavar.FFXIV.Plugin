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
}