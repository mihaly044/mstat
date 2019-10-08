using System;
using System.Runtime.InteropServices;
using PInvoke;

namespace mstat.Core
{
    /// <summary>
    /// A handy utility to run a standard event loop
    /// </summary>
    public static class EventLoop
    {

        /// <summary>
        /// Run an event loop until a <see cref="User32.WindowMessage.WM_QUIT"/> message is caught
        /// </summary>
        public static void Run()
        {
            var msg = new User32.MSG();
            var handle = GCHandle.Alloc(msg);

            while (true)
            {
                if (!User32.PeekMessage((IntPtr)handle, IntPtr.Zero, User32.WindowMessage.WM_NULL, User32.WindowMessage.WM_NULL, User32.PeekMessageRemoveFlags.PM_REMOVE))
                    continue;

                if (msg.message == User32.WindowMessage.WM_QUIT)
                    break;

                User32.TranslateMessage(ref msg);
                User32.DispatchMessage(ref msg);
            }

            handle.Free();
        }
    }
}
