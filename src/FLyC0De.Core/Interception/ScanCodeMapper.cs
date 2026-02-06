using System.Collections.Generic;

namespace FLyC0De.Core.Interception
{
    /// <summary>
    /// Maps keyboard scan codes to human-readable key names.
    /// </summary>
    public static class ScanCodeMapper
    {
        private static readonly Dictionary<ushort, string> ScanCodes = new Dictionary<ushort, string>
        {
            // Function keys
            { 0x3B, "F1" }, { 0x3C, "F2" }, { 0x3D, "F3" }, { 0x3E, "F4" },
            { 0x3F, "F5" }, { 0x40, "F6" }, { 0x41, "F7" }, { 0x42, "F8" },
            { 0x43, "F9" }, { 0x44, "F10" }, { 0x57, "F11" }, { 0x58, "F12" },

            // Number row
            { 0x02, "1" }, { 0x03, "2" }, { 0x04, "3" }, { 0x05, "4" },
            { 0x06, "5" }, { 0x07, "6" }, { 0x08, "7" }, { 0x09, "8" },
            { 0x0A, "9" }, { 0x0B, "0" },

            // Letters
            { 0x1E, "A" }, { 0x30, "B" }, { 0x2E, "C" }, { 0x20, "D" },
            { 0x12, "E" }, { 0x21, "F" }, { 0x22, "G" }, { 0x23, "H" },
            { 0x17, "I" }, { 0x24, "J" }, { 0x25, "K" }, { 0x26, "L" },
            { 0x32, "M" }, { 0x31, "N" }, { 0x18, "O" }, { 0x19, "P" },
            { 0x10, "Q" }, { 0x13, "R" }, { 0x1F, "S" }, { 0x14, "T" },
            { 0x16, "U" }, { 0x2F, "V" }, { 0x11, "W" }, { 0x2D, "X" },
            { 0x15, "Y" }, { 0x2C, "Z" },

            // Numpad
            { 0x52, "NumPad0" }, { 0x4F, "NumPad1" }, { 0x50, "NumPad2" },
            { 0x51, "NumPad3" }, { 0x4B, "NumPad4" }, { 0x4C, "NumPad5" },
            { 0x4D, "NumPad6" }, { 0x47, "NumPad7" }, { 0x48, "NumPad8" },
            { 0x49, "NumPad9" }, { 0x53, "NumPadDot" }, { 0x37, "NumPadMul" },
            { 0x4A, "NumPadSub" }, { 0x4E, "NumPadAdd" },

            // Special keys
            { 0x01, "Escape" }, { 0x0E, "Backspace" }, { 0x0F, "Tab" },
            { 0x1C, "Enter" }, { 0x1D, "LeftCtrl" }, { 0x2A, "LeftShift" },
            { 0x36, "RightShift" }, { 0x38, "LeftAlt" }, { 0x39, "Space" },
            { 0x3A, "CapsLock" }, { 0x45, "NumLock" }, { 0x46, "ScrollLock" },

            // Symbols
            { 0x0C, "Minus" }, { 0x0D, "Equals" }, { 0x1A, "LeftBracket" },
            { 0x1B, "RightBracket" }, { 0x27, "Semicolon" }, { 0x28, "Quote" },
            { 0x29, "Grave" }, { 0x2B, "Backslash" }, { 0x33, "Comma" },
            { 0x34, "Period" }, { 0x35, "Slash" },
        };

        // Extended keys (E0 prefix)
        private static readonly Dictionary<ushort, string> ExtendedScanCodes = new Dictionary<ushort, string>
        {
            { 0x1C, "NumPadEnter" }, { 0x1D, "RightCtrl" }, { 0x35, "NumPadDiv" },
            { 0x38, "RightAlt" }, { 0x47, "Home" }, { 0x48, "Up" },
            { 0x49, "PageUp" }, { 0x4B, "Left" }, { 0x4D, "Right" },
            { 0x4F, "End" }, { 0x50, "Down" }, { 0x51, "PageDown" },
            { 0x52, "Insert" }, { 0x53, "Delete" },
            { 0x5B, "LeftWin" }, { 0x5C, "RightWin" }, { 0x5D, "Menu" },
        };

        /// <summary>
        /// Gets the human-readable name for a scan code.
        /// </summary>
        public static string GetKeyName(ushort scanCode, ushort state)
        {
            bool isExtended = (state & InterceptionDriver.KeyE0) != 0;
            
            if (isExtended && ExtendedScanCodes.TryGetValue(scanCode, out string extName))
            {
                return extName;
            }

            if (ScanCodes.TryGetValue(scanCode, out string name))
            {
                return name;
            }

            return $"Key0x{scanCode:X2}";
        }

        /// <summary>
        /// Gets all known key names for UI display.
        /// </summary>
        public static IEnumerable<string> GetAllKeyNames()
        {
            foreach (var name in ScanCodes.Values)
                yield return name;
            foreach (var name in ExtendedScanCodes.Values)
                yield return name;
        }
    }
}
