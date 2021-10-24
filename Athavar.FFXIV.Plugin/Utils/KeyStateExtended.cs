// <copyright file="KeyStateExtended.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    using Dalamud.Game.ClientState.Keys;

    internal class KeyStateExtended
    {

        private delegate ref int GetRefValueDelegate(int virtualKey);

        private GetRefValueDelegate? GetRefValue = null;

        private static KeyStateExtended? instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyStateExtended"/> class.
        /// </summary>
        public KeyStateExtended()
        {
            var getRefValue = typeof(KeyState).GetMethod("GetRefValue", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new NotImplementedException();
            var localgetRefValue = typeof(KeyStateExtended).GetField("GetRefValue", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new NotImplementedException();
            var del = Delegate.CreateDelegate(localgetRefValue.FieldType, DalamudBinding.KeyState, getRefValue);
            localgetRefValue.SetValue(this, del);
        }

        public static KeyStateExtended Instance
        {
            get
            {
                if (instance is null)
                {
                    instance = new();
                }

                return instance;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="millisecondsDuration"></param>
        /// <param name="virtualKey"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        internal Task PressKey(int millisecondsDuration, params VirtualKey[] virtualKey)
            => this.PressKey(millisecondsDuration, (IEnumerable<VirtualKey>)virtualKey);

        /// <summary>
        ///
        /// </summary>
        /// <param name="millisecondsDuration"></param>
        /// <param name="virtualKeys"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
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
        ///
        /// </summary>
        /// <param name="millisecondsDuration"></param>
        /// <param name="virtualKey"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        internal async Task PressKey(int millisecondsDuration, VirtualKey virtualKey)
        {
            this.DownKey(virtualKey);
            await Task.Delay(millisecondsDuration);
            this.UpKey(virtualKey);
        }

        internal void DownKey(VirtualKey virtualKey) => this.SetKey(virtualKey, true);

        internal void UpKey(VirtualKey virtualKey) => this.SetKey(virtualKey, false);

        private unsafe void SetKey(VirtualKey virtualKey, bool value)
        {
            if (this.GetRefValue is null)
            {
                return;
            }

            this.GetRefValue.Invoke((int)virtualKey) = value ? 1 : 0;
        }
    }
}
