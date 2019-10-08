using System;
using System.Drawing;
using mstat.Core.Win32;
using PInvoke;

namespace mstat.Core
{
    public static class IconHelper
    {
        /// <summary>
        /// <inheritdoc cref="Imports.GetClassLong32"/>
        /// </summary>
        private static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
                return new IntPtr(Imports.GetClassLong32(hWnd, nIndex));
            return Imports.GetClassLong64(hWnd, nIndex);
        }

        /// <summary>
        ///     Retrieve the icon associated with a window 
        /// </summary>
        /// <param name="hWnd">Handle to the target window</param>
        /// <returns>
        ///     If the function was successful A 16×16 size <see cref="Bitmap"/> of the icon;
        ///     otherwise a null value
        /// </returns>
        public static Image GetSmallWindowIcon(IntPtr hWnd)
        {
            var hIcon = User32.SendMessage(hWnd, User32.WindowMessage.WM_GETICON, new IntPtr(2), IntPtr.Zero);

            if (hIcon == IntPtr.Zero)
                hIcon = GetClassLongPtr(hWnd, 0x7F0);

            if (hIcon != IntPtr.Zero)
            {
                var bmp = new Bitmap(Icon.FromHandle(hIcon).ToBitmap(), 16, 16);
                // TODO: Not sure if the retrieved icon has to be destroyed here
                //Imports.DestroyIcon(hIcon);
                return bmp;
            }

            return null;
        }
    }
}
