using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using mstat.Core.Win32;
using PInvoke;

namespace mstat.Core
{
    /// <summary>
    /// Argument class for the <see cref="WinHook.ForegroundWindowChanged"/> event handler
    /// </summary>
    public class ForegroundWindowChangedArg
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        // ReSharper disable once InconsistentNaming
        public IntPtr hWnd { get; set; }
    }

    /// <summary>
    /// Argument class for the <see cref="WinHook.MouseChanged"/> event handler
    /// </summary>
    public class MouseMessageArg
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Msg { get; set; }
    }

    public class WinHook
    {
        /// <summary>
        ///     The function assigned to ForegroundWindowChanged event handler gets invoked
        ///     when the current foreground window changes
        /// </summary>
        public static EventHandler<ForegroundWindowChangedArg> ForegroundWindowChanged;

        /// <summary>
        ///     The function assigned to MouseChanged event handler gets invoked
        ///     when a <see cref="User32.WindowMessage.WM_LBUTTONDOWN"/> or <see cref="User32.WindowMessage.WM_RBUTTONDOWN"/>
        ///     message is caught in <see cref="MouseHookProc"/>
        /// </summary>
        public static EventHandler<MouseMessageArg> MouseChanged;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        /// <summary>
        ///     Delegate object for <see cref="WinEventProc"/>
        /// </summary>
        private static readonly Imports.WinEventProcEx EventProcDelegate;

        /// <summary>
        ///     Function pointer to <see cref="WinEventProc"/> for unmanaged code
        /// </summary>
        private static readonly IntPtr CbForegroundPtr;

        /// <summary>
        ///     The hook handle that the OS assigns when calling SetWinEventHook.
        /// </summary>
        private static readonly User32.SafeEventHookHandle ForegroundWindowHook;

        /// <summary>
        ///     The hook handle that the OS assigns when calling SetWindowsHookEx.
        /// </summary>
        private static readonly User32.SafeHookHandle MouseHook;

        /// <summary>
        ///     Delegate object for <see cref="MouseHookProc"/>
        /// </summary>
        private static readonly User32.WindowsHookDelegate MouseHookDelegate;

        /// <summary>
        ///     Function pointer to <see cref="MouseHookProc"/> for unmanaged code
        /// </summary>
        private static readonly IntPtr CbMouseLlPtr;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        /// <summary>
        ///     Set up hooks
        /// </summary>
        static WinHook()
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;

            // Set up hooks
            EventProcDelegate = WinEventProc;
            CbForegroundPtr = Marshal.GetFunctionPointerForDelegate(EventProcDelegate);
            ForegroundWindowHook = User32.SetWinEventHook(User32.WindowsEventHookType.EVENT_SYSTEM_FOREGROUND,
                User32.WindowsEventHookType.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, CbForegroundPtr, 0, 0,
                User32.WindowsEventHookFlags.WINEVENT_OUTOFCONTEXT);

            MouseHookDelegate = MouseHookProc;
            CbMouseLlPtr = Marshal.GetFunctionPointerForDelegate(MouseHookDelegate);
            MouseHook = User32.SetWindowsHookEx(User32.WindowsHookType.WH_MOUSE_LL, CbMouseLlPtr,
                Kernel32.GetModuleHandle(null).DangerousGetHandle(), 0);

            // Run standard event loop
            //EventLoop.Run();
        }

        /// <inheritdoc cref="Imports.WinEventProcEx"/>
        private static void WinEventProc(IntPtr hWinEventHook, uint @event,
            IntPtr hWnd, int idObject, int idChild, int dwEventThread, uint dwMsEventTime)
        {
            ForegroundWindowChanged?.Invoke(null, CreateForegroundChangedEventArg());
        }

        /// <summary>
        ///     Create a ForegroundWindowChangedArg object containing the image name and process id
        ///     associated with the current foreground window. The <see cref="ForegroundWindowChangedArg.ProcessName"/>
        ///     argument will be set to "unknown" if we failed to determine it
        /// </summary>
        /// <returns>
        ///     An instance of <see cref="ForegroundWindowChangedArg"/>
        /// </returns>
        public static ForegroundWindowChangedArg CreateForegroundChangedEventArg()
        {
            var window = User32.GetForegroundWindow();
            User32.GetWindowThreadProcessId(window, out var pid);

            string processName;
            try
            {
                var process = Process.GetProcessById(pid);
                processName = process.ProcessName;

            }
            catch (ArgumentException)
            {
                // The process we were looking for isn't running anymore
                processName = "unknown";
            }

            return new ForegroundWindowChangedArg
            {
                ProcessId = pid,
                ProcessName = processName,
                hWnd = window
            };
        }

        /// <summary>
        /// <inheritdoc cref="User32.WindowsHookDelegate"/>
        /// </summary>
        private static int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var msg = (User32.WindowMessage) wParam;
                if (msg == User32.WindowMessage.WM_LBUTTONDOWN || msg == User32.WindowMessage.WM_RBUTTONDOWN)
                {
                    var input = Marshal.PtrToStructure<User32.MOUSEINPUT>(lParam);
                    MouseChanged?.Invoke(null, new MouseMessageArg
                    {
                        Msg = (int)msg,
                        X =  input.dx,
                        Y = input.dy
                    });
                }
            }

            return User32.CallNextHookEx(MouseHook.DangerousGetHandle(), nCode, wParam, lParam);
        }

        /// <summary>
        ///     Unset any set hooks at the end of execution
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CurrentDomainOnProcessExit(object sender, EventArgs e)
        {
            Imports.UnhookWindowsHookEx(MouseHook.DangerousGetHandle());
            Imports.UnhookWinEvent(ForegroundWindowHook.DangerousGetHandle());
        }
    }
}
