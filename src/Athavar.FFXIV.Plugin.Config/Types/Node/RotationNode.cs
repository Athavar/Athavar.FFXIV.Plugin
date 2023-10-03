// <copyright file="RotationNode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
// ReSharper disable once CheckNamespace

namespace Athavar.FFXIV.Plugin;

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

public sealed class RotationNode : Node
{
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public List<RotationNode> Duplicates { get; } = new();

    /// <inheritdoc/>
    public override string Name { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public ImmutableArray<int> Rotations { get; set; } = Array.Empty<int>().ToImmutableArray();

    [JsonInclude]
    [JsonPropertyName("Rotations")]
    [JsonProperty("Rotations")]
    public int[] RotationArray
    {
        get => this.Rotations.ToArray();
        set => this.Rotations = value.ToImmutableArray();
    }

    /// <inheritdoc/>
    public override string ToString() => this.Name;

    // ReSharper disable once NonReadonlyMemberInGetHashCode
    internal string GetRotationString() => string.Join('-', this.Rotations);
}