namespace Athavar.FFXIV.Plugin;

using Newtonsoft.Json;

/// <summary>
///     Instancinator Module configuration.
/// </summary>
internal class InstancinatorConfiguration
{
    public string KeyCode = Native.KeyCode.NumPad0.ToString();

    public int ExtraDelay = 0;

    [JsonIgnore]
    private Configuration? configuration;

    /// <summary>
    ///     Gets or sets a value indicating whether the plugin functionality is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     Save the configuration.
    /// </summary>
    public void Save() => this.configuration?.Save();

    /// <summary>
    ///     Setup <see cref="InstancinatorConfiguration" />.
    /// </summary>
    /// <param name="conf">The <see cref="Configuration" />.</param>
    internal void Setup(Configuration conf) => this.configuration = conf;
}