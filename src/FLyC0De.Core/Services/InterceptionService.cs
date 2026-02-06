using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FLyC0De.Core.Interception;

namespace FLyC0De.Core.Services
{
    /// <summary>
    /// Main service for intercepting keyboard input using the Interception driver.
    /// Runs on a dedicated background thread to ensure low-latency input handling.
    /// </summary>
    public class InterceptionService : IDisposable
    {
        private IntPtr _context;
        private Thread _interceptThread;
        private volatile bool _running;
        private readonly ConcurrentDictionary<int, KeyboardDevice> _devices;
        private readonly HashSet<string> _interceptedDeviceIds;
        private readonly object _lock = new object();

        /// <summary>
        /// Fired when a key is pressed or released on any keyboard.
        /// Set e.Handled = true to block the key from reaching applications.
        /// </summary>
        public event EventHandler<KeyboardEventArgs> KeyEvent;

        /// <summary>
        /// Fired when a new keyboard device is detected.
        /// </summary>
        public event EventHandler<KeyboardDevice> DeviceDetected;

        /// <summary>
        /// Indicates if the driver is loaded and the service is running.
        /// </summary>
        public bool IsRunning => _running;

        /// <summary>
        /// Gets all detected keyboard devices.
        /// </summary>
        public IEnumerable<KeyboardDevice> Devices => _devices.Values;

        public InterceptionService()
        {
            _devices = new ConcurrentDictionary<int, KeyboardDevice>();
            _interceptedDeviceIds = new HashSet<string>();
        }

        /// <summary>
        /// Starts the interception service. The driver must be installed.
        /// </summary>
        public (bool Success, string Error) Start()
        {
            if (_running) return (true, null);

            try
            {
                _context = InterceptionDriver.interception_create_context();
                if (_context == IntPtr.Zero)
                {
                    return (false, "Failed to create interception context. The driver might not be installed or the service is disabled.");
                }

                // Set filter to capture all keyboard events
                InterceptionDriver.interception_set_filter(
                    _context,
                    InterceptionDriver.IsKeyboard,
                    InterceptionDriver.FilterKeyAll);

                _running = true;
                _interceptThread = new Thread(InterceptionLoop)
                {
                    Name = "FLyC0De Interception",
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };
                _interceptThread.Start();

                return (true, null);
            }
            catch (DllNotFoundException)
            {
                return (false, "interception.dll not found. Please ensure the DLL is in the application directory.");
            }
            catch (Exception ex)
            {
                return (false, $"Driver initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops the interception service.
        /// </summary>
        public void Stop()
        {
            _running = false;
            _interceptThread?.Join(1000);

            if (_context != IntPtr.Zero)
            {
                InterceptionDriver.interception_destroy_context(_context);
                _context = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Sets whether a device's keys should be intercepted (blocked from system).
        /// </summary>
        public void SetDeviceIntercepted(string hardwareId, bool intercepted)
        {
            lock (_lock)
            {
                if (intercepted)
                    _interceptedDeviceIds.Add(hardwareId);
                else
                    _interceptedDeviceIds.Remove(hardwareId);
            }

            // Update device object
            foreach (var device in _devices.Values)
            {
                if (device.HardwareId == hardwareId)
                {
                    device.IsIntercepted = intercepted;
                }
            }
        }

        /// <summary>
        /// Checks if a device is set to be intercepted.
        /// </summary>
        public bool IsDeviceIntercepted(string hardwareId)
        {
            lock (_lock)
            {
                return _interceptedDeviceIds.Contains(hardwareId);
            }
        }

        private void InterceptionLoop()
        {
            while (_running)
            {
                // Wait for input with timeout (allows checking _running flag)
                int device = InterceptionDriver.interception_wait_with_timeout(_context, 100);
                if (device == 0 || !_running) continue;

                // Only process keyboards
                if (InterceptionDriver.interception_is_keyboard(device) == 0) continue;

                var stroke = new InterceptionDriver.KeyStroke();
                int received = InterceptionDriver.interception_receive(_context, device, ref stroke, 1);
                if (received <= 0) continue;

                // Get or create device info
                var keyboardDevice = GetOrCreateDevice(device);

                // Create event args
                var args = new KeyboardEventArgs(keyboardDevice, stroke.Code, stroke.State);

                // Check if this device should be intercepted
                bool shouldBlock = false;
                lock (_lock)
                {
                    if (keyboardDevice.HardwareId != null)
                    {
                        shouldBlock = _interceptedDeviceIds.Contains(keyboardDevice.HardwareId);
                    }
                }

                // Fire event - handlers can set Handled = true
                try
                {
                    KeyEvent?.Invoke(this, args);
                }
                catch
                {
                    // Don't let handler exceptions crash the input thread
                }

                // If device is intercepted or handler said to block, don't forward the key
                if (!shouldBlock && !args.Handled)
                {
                    InterceptionDriver.interception_send(_context, device, ref stroke, 1);
                }
            }
        }

        private KeyboardDevice GetOrCreateDevice(int deviceId)
        {
            return _devices.GetOrAdd(deviceId, id =>
            {
                var device = new KeyboardDevice
                {
                    DeviceId = id,
                    HardwareId = InterceptionDriver.GetHardwareId(_context, id),
                    FriendlyName = $"Keyboard {id}",
                    LastSeen = DateTime.Now
                };

                // Check if already in intercepted list
                lock (_lock)
                {
                    device.IsIntercepted = device.HardwareId != null && 
                                           _interceptedDeviceIds.Contains(device.HardwareId);
                }

                // Notify listeners
                DeviceDetected?.Invoke(this, device);

                return device;
            });
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
