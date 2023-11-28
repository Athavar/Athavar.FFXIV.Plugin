// <copyright file="RollingList.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Utils;

using System.Collections;

public sealed class RollingList<T> : IReadOnlyList<T>
{
    private readonly LinkedList<T> list = new();
    private readonly bool reversed;

    public RollingList(int maximumCount, bool reversed = false)
    {
        if (maximumCount <= 0)
        {
            throw new ArgumentException(null, nameof(maximumCount));
        }

        this.reversed = reversed;
        this.MaximumCount = maximumCount;
    }

    public int Count => this.list.Count;

    public int MaximumCount { get; private set; }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= this.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (index == 0)
            {
                return this.list.First!.Value;
            }

            return this.list.Skip(index).First();
        }
    }

    public void Add(T value)
    {
        if (this.reversed)
        {
            if (this.list.Count == this.MaximumCount)
            {
                this.list.RemoveLast();
            }

            this.list.AddFirst(value);
        }
        else
        {
            if (this.list.Count == this.MaximumCount)
            {
                this.list.RemoveFirst();
            }

            this.list.AddLast(value);
        }
    }

    public void Resize(int newMaximumCount)
    {
        if (newMaximumCount <= 0)
        {
            throw new ArgumentException(null, nameof(newMaximumCount));
        }

        this.MaximumCount = newMaximumCount;
        while (this.Count > this.MaximumCount)
        {
            if (this.reversed)
            {
                this.list.RemoveLast();
            }
            else
            {
                this.list.RemoveFirst();
            }
        }
    }

    public void Clear() => this.list.Clear();

    public IEnumerator<T> GetEnumerator() => this.list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}