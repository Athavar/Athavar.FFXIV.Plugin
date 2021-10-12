namespace Athavar.FFXIV.Plugin
{
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;

    /// <summary>
    /// Text entry node type.
    /// </summary>
    public class TextEntryNode : INode
    {
        /// <summary>
        /// Gets or sets a value indicating whether the node is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <inheritdoc/>
        [JsonIgnore]
        public string Name
        {
            get
            {
                return !string.IsNullOrEmpty(this.ZoneText)
                    ? $"{this.Text} ({this.ZoneText})"
                    : this.Text;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the matching text.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the matching text is a regex.
        /// </summary>
        [JsonIgnore]
        public bool IsTextRegex => this.Text.StartsWith("/") && this.Text.EndsWith("/");

        /// <summary>
        /// Gets the matching text as a compiled regex.
        /// </summary>
        [JsonIgnore]
        public Regex? TextRegex
        {
            get
            {
                try
                {
                    return new(this.Text.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this entry should be zone restricted.
        /// </summary>
        public bool ZoneRestricted { get; set; } = false;

        /// <summary>
        /// Gets or sets the matching zone text.
        /// </summary>
        public string ZoneText { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the matching zone text is a regex.
        /// </summary>
        [JsonIgnore]
        public bool ZoneIsRegex => this.ZoneText.StartsWith("/") && this.ZoneText.EndsWith("/");

        /// <summary>
        /// Gets the matching zone text as a compiled regex.
        /// </summary>
        [JsonIgnore]
        public Regex? ZoneRegex
        {
            get
            {
                try
                {
                    return new(this.ZoneText.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
