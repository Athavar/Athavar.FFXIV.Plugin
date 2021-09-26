using Dalamud.Game.ClientState.Keys;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Athavar.FFXIV.Plugin.Utils
{
    internal class KeyStateExtended
    {

        private delegate ref int GetRefValueDelegate(int virtualKey);
        private GetRefValueDelegate? GetRefValue = null;

        private static KeyStateExtended? instance;

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

        internal Task PressKey(int millisecondsDuration, params VirtualKey[] virtualKey)
            => PressKey(millisecondsDuration, (IEnumerable<VirtualKey>)virtualKey);

        internal async Task PressKey(int millisecondsDuration, IEnumerable<VirtualKey> virtualKeys)
        {
            foreach (var key in virtualKeys)
            {
                DownKey(key);
            }

            await Task.Delay(millisecondsDuration);

            foreach (var key in virtualKeys)
            {
                UpKey(key);
            }
        }

        internal async Task PressKey(int millisecondsDuration, VirtualKey virtualKey)
        {
            DownKey(virtualKey);
            await Task.Delay(millisecondsDuration);
            UpKey(virtualKey);
        }

        internal void DownKey(VirtualKey virtualKey) => this.SetKey(virtualKey, true);

        internal void UpKey(VirtualKey virtualKey) => this.SetKey(virtualKey, false);

        private unsafe void SetKey(VirtualKey virtualKey, bool value)
        {
            if (GetRefValue is null)
            {
                return;
            }

            GetRefValue.Invoke((int)virtualKey) = value ? 1 : 0;
        }
    }
}
