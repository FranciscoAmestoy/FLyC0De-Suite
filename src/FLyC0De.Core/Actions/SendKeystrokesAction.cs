using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FLyC0De.Core.Interception;

namespace FLyC0De.Core.Actions
{
    /// <summary>
    /// Sends keystrokes to the system using keybd_event API for full key support.
    /// </summary>
    public class SendKeystrokesAction : ActionBase
    {
        public override string TypeId => "send_keys";
        public override string DisplayName => "Send Keystrokes";

        /// <summary>
        /// Key sequence string (e.g., "Ctrl+Shift+F1", "Shift+F23", "Delete").
        /// Supports human-readable format.
        /// </summary>
        public string KeySequence { get; set; }

        /// <summary>
        /// Delay in milliseconds before sending keys.
        /// </summary>
        public int DelayMs { get; set; }

        #region Windows API

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        #endregion

        // Virtual Key Codes
        private static readonly Dictionary<string, byte> VirtualKeyCodes = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase)
        {
            // Function keys F1-F24
            { "F1", 0x70 }, { "F2", 0x71 }, { "F3", 0x72 }, { "F4", 0x73 },
            { "F5", 0x74 }, { "F6", 0x75 }, { "F7", 0x76 }, { "F8", 0x77 },
            { "F9", 0x78 }, { "F10", 0x79 }, { "F11", 0x7A }, { "F12", 0x7B },
            { "F13", 0x7C }, { "F14", 0x7D }, { "F15", 0x7E }, { "F16", 0x7F },
            { "F17", 0x80 }, { "F18", 0x81 }, { "F19", 0x82 }, { "F20", 0x83 },
            { "F21", 0x84 }, { "F22", 0x85 }, { "F23", 0x86 }, { "F24", 0x87 },
            
            // Modifier keys
            { "Shift", 0x10 }, { "Ctrl", 0x11 }, { "Control", 0x11 }, { "Alt", 0x12 },
            { "LShift", 0xA0 }, { "RShift", 0xA1 },
            { "LCtrl", 0xA2 }, { "RCtrl", 0xA3 },
            { "LAlt", 0xA4 }, { "RAlt", 0xA5 },
            
            // Navigation keys
            { "Enter", 0x0D }, { "Return", 0x0D },
            { "Tab", 0x09 },
            { "Escape", 0x1B }, { "Esc", 0x1B },
            { "Backspace", 0x08 }, { "BS", 0x08 },
            { "Delete", 0x2E }, { "Del", 0x2E },
            { "Insert", 0x2D }, { "Ins", 0x2D },
            { "Home", 0x24 }, { "End", 0x23 },
            { "PageUp", 0x21 }, { "PgUp", 0x21 },
            { "PageDown", 0x22 }, { "PgDn", 0x22 },
            { "Up", 0x26 }, { "Down", 0x28 },
            { "Left", 0x25 }, { "Right", 0x27 },
            
            // Other special keys
            { "Space", 0x20 },
            { "PrintScreen", 0x2C }, { "PrtSc", 0x2C },
            { "ScrollLock", 0x91 },
            { "Pause", 0x13 }, { "Break", 0x13 },
            { "CapsLock", 0x14 },
            { "NumLock", 0x90 },
            
            // Numpad
            { "NumPad0", 0x60 }, { "NumPad1", 0x61 }, { "NumPad2", 0x62 },
            { "NumPad3", 0x63 }, { "NumPad4", 0x64 }, { "NumPad5", 0x65 },
            { "NumPad6", 0x66 }, { "NumPad7", 0x67 }, { "NumPad8", 0x68 },
            { "NumPad9", 0x69 },
            { "NumPadAdd", 0x6B }, { "NumPadSub", 0x6D },
            { "NumPadMul", 0x6A }, { "NumPadDiv", 0x6F },
            { "NumPadDot", 0x6E }, { "NumPadEnter", 0x0D },
            
            // Letters A-Z
            { "A", 0x41 }, { "B", 0x42 }, { "C", 0x43 }, { "D", 0x44 },
            { "E", 0x45 }, { "F", 0x46 }, { "G", 0x47 }, { "H", 0x48 },
            { "I", 0x49 }, { "J", 0x4A }, { "K", 0x4B }, { "L", 0x4C },
            { "M", 0x4D }, { "N", 0x4E }, { "O", 0x4F }, { "P", 0x50 },
            { "Q", 0x51 }, { "R", 0x52 }, { "S", 0x53 }, { "T", 0x54 },
            { "U", 0x55 }, { "V", 0x56 }, { "W", 0x57 }, { "X", 0x58 },
            { "Y", 0x59 }, { "Z", 0x5A },
            
            // Numbers 0-9
            { "0", 0x30 }, { "1", 0x31 }, { "2", 0x32 }, { "3", 0x33 },
            { "4", 0x34 }, { "5", 0x35 }, { "6", 0x36 }, { "7", 0x37 },
            { "8", 0x38 }, { "9", 0x39 },
        };

        // Keys that require extended key flag
        private static readonly HashSet<byte> ExtendedKeys = new HashSet<byte>
        {
            0x2D, 0x2E, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, // Ins, Del, PgUp, PgDn, Home, End, Arrows
            0x5B, 0x5C, 0x5D, // Win keys, Apps
            0x6F, // NumPad Divide
            0xA3, 0xA5, // RCtrl, RAlt
        };

        // Modifier key names
        private static readonly HashSet<string> ModifierKeyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Shift", "Ctrl", "Control", "Alt"
        };

        private void PressKey(byte vkCode)
        {
            uint flags = ExtendedKeys.Contains(vkCode) ? KEYEVENTF_EXTENDEDKEY : 0;
            keybd_event(vkCode, 0, flags, UIntPtr.Zero);
        }

        private void ReleaseKey(byte vkCode)
        {
            uint flags = KEYEVENTF_KEYUP | (ExtendedKeys.Contains(vkCode) ? KEYEVENTF_EXTENDEDKEY : 0);
            keybd_event(vkCode, 0, flags, UIntPtr.Zero);
        }

        private void SendKeyCombo(List<byte> modifiers, byte mainKey)
        {
            // Press modifiers
            foreach (var mod in modifiers)
            {
                PressKey(mod);
            }

            // Small delay after modifiers
            System.Threading.Thread.Sleep(20);

            // Press and release main key
            PressKey(mainKey);
            System.Threading.Thread.Sleep(20);
            ReleaseKey(mainKey);

            // Small delay before releasing modifiers
            System.Threading.Thread.Sleep(20);

            // Release modifiers in reverse order
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                ReleaseKey(modifiers[i]);
            }
        }

        public override async Task ExecuteAsync(KeyboardEventArgs keyEvent)
        {
            if (string.IsNullOrWhiteSpace(KeySequence))
                return;

            if (DelayMs > 0)
            {
                await Task.Delay(DelayMs);
            }

            try
            {
                var input = KeySequence.Trim();
                System.Diagnostics.Debug.WriteLine($"[SendKeys] Processing: '{input}'");

                // Check if it's a key combination (contains + separator)
                if (input.Contains("+"))
                {
                    var parts = input.Split('+');
                    var modifiers = new List<byte>();
                    byte mainKeyVk = 0;

                    foreach (var part in parts)
                    {
                        var trimmed = part.Trim();
                        if (string.IsNullOrEmpty(trimmed)) continue;

                        if (ModifierKeyNames.Contains(trimmed))
                        {
                            if (VirtualKeyCodes.TryGetValue(trimmed, out var modVk))
                            {
                                modifiers.Add(modVk);
                                System.Diagnostics.Debug.WriteLine($"[SendKeys] Modifier: {trimmed} -> 0x{modVk:X2}");
                            }
                        }
                        else
                        {
                            if (VirtualKeyCodes.TryGetValue(trimmed, out var vk))
                            {
                                mainKeyVk = vk;
                                System.Diagnostics.Debug.WriteLine($"[SendKeys] MainKey: {trimmed} -> 0x{vk:X2}");
                            }
                            else if (trimmed.Length == 1)
                            {
                                mainKeyVk = (byte)char.ToUpper(trimmed[0]);
                                System.Diagnostics.Debug.WriteLine($"[SendKeys] MainKey (char): {trimmed} -> 0x{mainKeyVk:X2}");
                            }
                        }
                    }

                    if (mainKeyVk != 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SendKeys] Sending combo: {modifiers.Count} modifiers + 0x{mainKeyVk:X2}");
                        SendKeyCombo(modifiers, mainKeyVk);
                        System.Diagnostics.Debug.WriteLine($"[SendKeys] Combo sent!");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[SendKeys] ERROR: No main key found!");
                    }
                }
                else
                {
                    // Single key
                    if (VirtualKeyCodes.TryGetValue(input, out var vk))
                    {
                        System.Diagnostics.Debug.WriteLine($"[SendKeys] Single key: {input} -> 0x{vk:X2}");
                        PressKey(vk);
                        System.Threading.Thread.Sleep(20);
                        ReleaseKey(vk);
                    }
                    else if (input.Length == 1)
                    {
                        var vkChar = (byte)char.ToUpper(input[0]);
                        System.Diagnostics.Debug.WriteLine($"[SendKeys] Single char: {input} -> 0x{vkChar:X2}");
                        PressKey(vkChar);
                        System.Threading.Thread.Sleep(20);
                        ReleaseKey(vkChar);
                    }
                    else
                    {
                        // Try as literal text using SendKeys
                        System.Diagnostics.Debug.WriteLine($"[SendKeys] Literal text via SendKeys.SendWait: {input}");
                        System.Windows.Forms.SendKeys.SendWait(input);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SendKeys] ERROR: {ex.Message}");
            }
        }

        public override bool Validate(out string error)
        {
            if (string.IsNullOrWhiteSpace(KeySequence))
            {
                error = "Key sequence is required";
                return false;
            }
            error = null;
            return true;
        }
    }
}
