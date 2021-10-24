// <copyright file="TalkEntryNode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin
{
    using System.Text.RegularExpressions;

    using Newtonsoft.Json;

    /// <summary>
    /// Text entry node type.
    /// </summary>
    public class TalkEntryNode : INode
    { /// <summary>
      /// Gets or sets a value indicating whether the node is enabled.
      /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        [JsonIgnore]
        public string Name
        {
            get
            {
                return this.TargetText;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets the matching target name.
        /// </summary>
        public string TargetText { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the matching target text is a regex.
        /// </summary>
        [JsonIgnore]
        public bool TargetIsRegex => this.TargetText.StartsWith("/") && this.TargetText.EndsWith("/");

        /// <summary>
        /// Gets the matching target text as a compiled regex.
        /// </summary>
        [JsonIgnore]
        public Regex? TargetRegex
        {
            get
            {
                try
                {
                    return new(this.TargetText.Trim('/'), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
