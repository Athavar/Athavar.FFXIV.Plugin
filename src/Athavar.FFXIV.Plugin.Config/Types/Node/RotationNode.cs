// <copyright file="RotationNode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

using System.Collections;
using System.Collections.Immutable;
using Newtonsoft.Json;

public sealed class RotationNode : INode, IEquatable<RotationNode>
{
    [JsonIgnore]
    public List<RotationNode> Duplicates { get; } = new();

    /// <inheritdoc/>
    public string Name { get; set; } = string.Empty;

    [JsonIgnore]
    public ImmutableArray<int> Rotations { get; set; } = Array.Empty<int>().ToImmutableArray();

    [JsonProperty("Rotations")]
    private int[] RotationArray
    {
        get => this.Rotations.ToArray();
        set => this.Rotations = value.ToImmutableArray();
    }

    public static bool operator ==(RotationNode? left, RotationNode? right) => Equals(left, right);

    public static bool operator !=(RotationNode? left, RotationNode? right) => !Equals(left, right);

    /// <inheritdoc/>
    public override string ToString() => this.Name;

    /// <inheritdoc/>
    public bool Equals(RotationNode? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (this.Rotations.Length != other.Rotations.Length)
        {
            return false;
        }

        for (var i = 0; i < this.Rotations.Length; i++)
        {
            if (this.Rotations[i] != other.Rotations[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is RotationNode other && this.Equals(other));

    /// <inheritdoc/>
    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => ((IStructuralEquatable)this.Rotations).GetHashCode(EqualityComparer<int>.Default);
}