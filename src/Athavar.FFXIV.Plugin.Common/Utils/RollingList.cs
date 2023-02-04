// <copyright file="RollingList.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Utils;

using System.Collections;

public class RollingList<T> : IReadOnlyList<T>
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

    public int MaximumCount { get; }

    public int Count => this.list.Count;

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= this.Count)
            {
                throw new ArgumentOutOfRangeException();
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

    public void Clear() => this.list.Clear();

    public IEnumerator<T> GetEnumerator() => this.list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}