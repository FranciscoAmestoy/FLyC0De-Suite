using System;

namespace FLyC0De.Core.Interception
{
    /// <summary>
    /// Event arguments for keyboard events from the Interception driver.
    /// </summary>
    public class KeyboardEventArgs : EventArgs
    {
        /// <summary>
        /// The device that generated this event.
        /// </summary>
        public KeyboardDevice Device { get; }

        /// <summary>
        /// The scan code of the key.
        /// </summary>
        public ushort ScanCode { get; }

        /// <summary>
        /// Whether this is a key down (press) event.
        /// </summary>
        public bool IsKeyDown { get; }

        /// <summary>
        /// Whether this is a key up (release) event.
        /// </summary>
        public bool IsKeyUp => !IsKeyDown;

        /// <summary>
        /// Raw state flags from the driver.
        /// </summary>
        public ushort State { get; }

        /// <summary>
        /// Set to true to block this key from reaching the system.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Human-readable key name if available.
        /// </summary>
        public string KeyName => ScanCodeMapper.GetKeyName(ScanCode, State);

        public KeyboardEventArgs(KeyboardDevice device, ushort scanCode, ushort state)
        {
            Device = device;
            ScanCode = scanCode;
            State = state;
            IsKeyDown = InterceptionDriver.IsKeyDownState(state);
            Handled = false;
        }

        public override string ToString()
        {
            return $"{(IsKeyDown ? "↓" : "↑")} {KeyName} (0x{ScanCode:X2}) from {Device?.FriendlyName ?? "Unknown"}";
        }
    }
}
