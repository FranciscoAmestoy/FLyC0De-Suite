using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FLyC0De.Core.Configuration
{
    /// <summary>
    /// Root configuration for FLyC0De Suite.
    /// </summary>
    public class AppConfiguration
    {
        /// <summary>
        /// Configuration version for migration support.
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Registered keyboard devices.
        /// </summary>
        public List<DeviceConfig> Devices { get; set; } = new List<DeviceConfig>();

        /// <summary>
        /// Key bindings (key = "deviceHwId:scanCode", value = binding config).
        /// </summary>
        public List<KeyBindingConfig> KeyBindings { get; set; } = new List<KeyBindingConfig>();

        /// <summary>
        /// General settings.
        /// </summary>
        public GeneralSettings Settings { get; set; } = new GeneralSettings();
    }

    /// <summary>
    /// Configuration for a registered keyboard device.
    /// </summary>
    public class DeviceConfig
    {
        /// <summary>
        /// Hardware ID from Windows.
        /// </summary>
        public string HardwareId { get; set; }

        /// <summary>
        /// User-friendly name.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Whether keys from this device are intercepted (blocked from system).
        /// </summary>
        public bool IsIntercepted { get; set; }
    }

    /// <summary>
    /// Configuration for a key binding.
    /// </summary>
    public class KeyBindingConfig
    {
        /// <summary>
        /// Hardware ID of the device this binding applies to.
        /// </summary>
        public string DeviceHardwareId { get; set; }

        /// <summary>
        /// Scan code of the key.
        /// </summary>
        public ushort ScanCode { get; set; }

        /// <summary>
        /// Human-readable key name for display.
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        /// Whether to trigger on key down (press).
        /// </summary>
        public bool TriggerOnKeyDown { get; set; } = true;

        /// <summary>
        /// Whether to trigger on key up (release).
        /// </summary>
        public bool TriggerOnKeyUp { get; set; }

        /// <summary>
        /// Whether to block this specific key.
        /// </summary>
        public bool BlockKey { get; set; } = true;

        /// <summary>
        /// Action configuration.
        /// </summary>
        public ActionConfig Action { get; set; }
    }

    /// <summary>
    /// Configuration for an action.
    /// </summary>
    public class ActionConfig
    {
        /// <summary>
        /// Action type ID (run_app, send_keys, http_request, play_sound).
        /// </summary>
        public string TypeId { get; set; }

        /// <summary>
        /// Action-specific parameters stored as JSON object.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// General application settings.
    /// </summary>
    public class GeneralSettings
    {
        /// <summary>
        /// Start minimized to system tray.
        /// </summary>
        public bool StartMinimized { get; set; }

        /// <summary>
        /// Start with Windows.
        /// </summary>
        public bool StartWithWindows { get; set; }

        /// <summary>
        /// Minimize to tray instead of taskbar.
        /// </summary>
        public bool MinimizeToTray { get; set; } = true;

        /// <summary>
        /// Application language code (e.g., "en", "es").
        /// </summary>
        public string Language { get; set; } = "en";

        /// <summary>
        /// The target bot platform for WSS integration.
        /// Values: "Streamer.bot", "Mix It Up", "SAMMI", "Firebot".
        /// </summary>
        public string TargetBot { get; set; } = "Streamer.bot";

        /// <summary>
        /// The WebSocket port for the target bot.
        /// </summary>
        public int BotPort { get; set; } = 8080;
    }
}
