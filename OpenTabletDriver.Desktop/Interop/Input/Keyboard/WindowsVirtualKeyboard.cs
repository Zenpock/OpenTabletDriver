using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Platform.Keyboard;

namespace OpenTabletDriver.Desktop.Interop.Input.Keyboard
{
    using static Windows;

    public class WindowsVirtualKeyboard : IVirtualKeyboard
    {
        private readonly IKeysProvider _keysProvider;

        public WindowsVirtualKeyboard(IKeysProvider keysProvider)
        {
            _keysProvider = keysProvider;
        }

        private void KeyEvent(string key, bool isPress)
        {
            var vk = (VirtualKey)_keysProvider.EtoToNative[key];
            short scanCode = (short)MapVirtualKey((uint)vk, 0);

            var input = new INPUT
            {
                type = INPUT_TYPE.KEYBD_INPUT,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = scanCode,
                        dwFlags = (isPress ? 0u : KEYEVENTF.KEYUP) | KEYEVENTF.SCANCODE,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };

            var inputs = new INPUT[] { input };
            SendInput((uint)inputs.Length, inputs, INPUT.Size);
        }

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        public void Press(string key)
        {
            KeyEvent(key, true);
        }

        public void Release(string key)
        {
            KeyEvent(key, false);
        }

        public void Press(IEnumerable<string> keys)
        {
            foreach (var key in keys)
                KeyEvent(key, true);
        }

        public void Release(IEnumerable<string> keys)
        {
            foreach (var key in keys)
                KeyEvent(key, false);
        }

        public IEnumerable<string> SupportedKeys => _keysProvider.EtoToNative.Keys;
    }
}
