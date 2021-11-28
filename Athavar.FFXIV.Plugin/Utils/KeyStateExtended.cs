// <copyright file="KeyStateExtended.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Utils;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Dalamud.Game.ClientState.Keys;

internal class KeyStateExtended
{
    private readonly GetRefValueDelegate? getRefValue = null;

    /// <summary>
    ///     Initializes a new instance of the <see cref="KeyStateExtended" /> class.
    /// </summary>
    public KeyStateExtended(IDalamudServices dalamudServices)
    {
        var getRefValue = typeof(KeyState).GetMethod("GetRefValue", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new Exception($"Method GetRefValue not found in class {nameof(KeyState)}");
        var localGetRefValue = typeof(KeyStateExtended).GetField("GetRefValue", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new Exception($"Method GetRefValue not found in class {nameof(KeyStateExtended)}");
        var del = Delegate.CreateDelegate(localGetRefValue.FieldType, dalamudServices.KeyState, getRefValue);
        localGetRefValue.SetValue(this, del);
    }

    private delegate ref int GetRefValueDelegate(int virtualKey);

    /// <summary>
    /// </summary>
    /// <param name="millisecondsDuration"></param>
    /// <param name="virtualKey"></param>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    internal Task PressKey(int millisecondsDuration, params VirtualKey[] virtualKey)
        => this.PressKey(millisecondsDuration, (IEnumerable<VirtualKey>)virtualKey);

    /// <summary>
    /// </summary>
    /// <param name="millisecondsDuration"></param>
    /// <param name="virtualKeys"></param>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    internal async Task PressKey(int millisecondsDuration, IEnumerable<VirtualKey> virtualKeys)
    {
        foreach (var key in virtualKeys)
        {
            this.DownKey(key);
        }

        await Task.Delay(millisecondsDuration);

        foreach (var key in virtualKeys)
        {
            this.UpKey(key);
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="millisecondsDuration"></param>
    /// <param name="virtualKey"></param>
    /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
    internal async Task PressKey(int millisecondsDuration, VirtualKey virtualKey)
    {
        this.DownKey(virtualKey);
        await Task.Delay(millisecondsDuration);
        this.UpKey(virtualKey);
    }

    internal void DownKey(VirtualKey virtualKey) => this.SetKey(virtualKey, true);

    internal void UpKey(VirtualKey virtualKey) => this.SetKey(virtualKey, false);

    private void SetKey(VirtualKey virtualKey, bool value)
    {
        if (this.getRefValue is null)
        {
            return;
        }

        this.getRefValue.Invoke((int)virtualKey) = value ? 1 : 0;
    }
}