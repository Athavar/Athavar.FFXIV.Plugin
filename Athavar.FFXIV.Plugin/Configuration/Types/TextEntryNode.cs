using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Athavar.FFXIV.Plugin
{
    public class TextEntryNode : INode
    {
        public bool Enabled { get; set; } = true;

        [JsonIgnore]
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(ZoneText))
                    return Text;
                else
                    return $"{Text} ({ZoneText})";
            }
            set
            {
                Text = value;
            }
        }

        public string Text { get; set; } = "";

        [JsonIgnore]
        public bool IsTextRegex => Text.StartsWith("/") && Text.EndsWith("/");

        [JsonIgnore]
        public Regex? TextRegex
        {
            get
            {
                try
                {
                    return new(Text.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
                catch
                {
                    return null;
                }
            }
        }

        public bool ZoneRestricted { get; set; } = false;

        public string ZoneText { get; set; } = "";

        [JsonIgnore]
        public bool ZoneIsRegex => ZoneText.StartsWith("/") && ZoneText.EndsWith("/");

        [JsonIgnore]
        public Regex? ZoneRegex
        {
            get
            {
                try
                {
                    return new(ZoneText.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
