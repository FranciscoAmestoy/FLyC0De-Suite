using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace FLyC0De.Core.Interception
{
    /// <summary>
    /// P/Invoke wrapper for the Interception driver API.
    /// Provides low-level keyboard interception at the driver level.
    /// </summary>
    public static class InterceptionDriver
    {
        private const string DllName = "interception.dll";

        // Device type constants
        public const int MaxKeyboard = 10;
        public const int MaxMouse = 10;
        
        // Filter flags
        public const ushort FilterKeyDown = 0x01;
        public const ushort FilterKeyUp = 0x02;
        public const ushort FilterKeyE0 = 0x04;
        public const ushort FilterKeyE1 = 0x08;
        public const ushort FilterKeyAll = FilterKeyDown | FilterKeyUp | FilterKeyE0 | FilterKeyE1;

        // Key state flags
        public const ushort KeyDown = 0x00;
        public const ushort KeyUp = 0x01;
        public const ushort KeyE0 = 0x02;
        public const ushort KeyE1 = 0x04;

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyStroke
        {
            public ushort Code;        // Scan code
            public ushort State;       // Key state (up/down/e0/e1)
            public uint Information;   // Reserved
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseStroke
        {
            public ushort State;
            public ushort Flags;
            public short Rolling;
            public int X;
            public int Y;
            public uint Information;
        }

        // Interception API functions
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr interception_create_context();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void interception_destroy_context(IntPtr context);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void interception_set_filter(
            IntPtr context, 
            InterceptionPredicate predicate, 
            ushort filter);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int interception_wait(IntPtr context);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int interception_wait_with_timeout(IntPtr context, ulong milliseconds);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int interception_receive(
            IntPtr context, 
            int device, 
            ref KeyStroke stroke, 
            uint nstroke);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int interception_send(
            IntPtr context, 
            int device, 
            ref KeyStroke stroke, 
            uint nstroke);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int interception_is_keyboard(int device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int interception_is_mouse(int device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint interception_get_hardware_id(
            IntPtr context, 
            int device, 
            IntPtr hardware_id_buffer, 
            uint buffer_size);

        // Delegate for filter predicate
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int InterceptionPredicate(int device);

        /// <summary>
        /// Predicate that matches all keyboard devices.
        /// </summary>
        public static int IsKeyboard(int device) => interception_is_keyboard(device);

        /// <summary>
        /// Predicate that matches all mouse devices.
        /// </summary>
        public static int IsMouse(int device) => interception_is_mouse(device);

        /// <summary>
        /// Gets the hardware ID for a device.
        /// </summary>
        public static string GetHardwareId(IntPtr context, int device)
        {
            const int bufferSize = 512;
            IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
            try
            {
                uint size = interception_get_hardware_id(context, device, buffer, (uint)bufferSize);
                if (size > 0)
                {
                    return Marshal.PtrToStringUni(buffer);
                }
                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>
        /// Checks if a key state indicates key down.
        /// </summary>
        public static bool IsKeyDownState(ushort state) => (state & KeyUp) == 0;

        /// <summary>
        /// Checks if a key state indicates key up.
        /// </summary>
        public static bool IsKeyUpState(ushort state) => (state & KeyUp) != 0;
    }
}
