namespace Athavar.FFXIV.Plugin.Config;

using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(
    WriteIndented = false,
    IncludeFields = true,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(AutoSpearConfiguration))]
[JsonSerializable(typeof(CommonConfiguration))]
[JsonSerializable(typeof(CraftQueueConfiguration))]
[JsonSerializable(typeof(DpsConfiguration))]
[JsonSerializable(typeof(DutyHistoryConfiguration))]
[JsonSerializable(typeof(InstancinatorConfiguration))]
[JsonSerializable(typeof(MacroConfiguration))]
[JsonSerializable(typeof(OpcodeWizardConfiguration))]
[JsonSerializable(typeof(SliceIsRightConfiguration))]
[JsonSerializable(typeof(YesConfiguration))]
public partial class ConfigurationJsonSerializerContext : JsonSerializerContext
{
}