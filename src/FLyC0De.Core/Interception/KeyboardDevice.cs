using System;
using System.Collections.Generic;

namespace FLyC0De.Core.Interception
{
    /// <summary>
    /// Represents a keyboard device detected by the Interception driver.
    /// </summary>
    public class KeyboardDevice
    {
        /// <summary>
        /// Device ID assigned by Interception (1-10 for keyboards).
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Hardware ID string from Windows.
        /// </summary>
        public string HardwareId { get; set; }

        /// <summary>
        /// User-defined friendly name (e.g., "MacroPad", "Main Keyboard").
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Whether this device's keys should be intercepted/blocked.
        /// </summary>
        public bool IsIntercepted { get; set; }

        /// <summary>
        /// Timestamp when this device was last detected.
        /// </summary>
        public DateTime LastSeen { get; set; }

        public override string ToString()
        {
            return $"{FriendlyName ?? "Unknown"} (Device {DeviceId})";
        }

        public override bool Equals(object obj)
        {
            return obj is KeyboardDevice device && HardwareId == device.HardwareId;
        }

        public override int GetHashCode()
        {
            return HardwareId?.GetHashCode() ?? 0;
        }
    }
}
