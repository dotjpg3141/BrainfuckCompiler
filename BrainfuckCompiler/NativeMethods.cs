using System;
using System.Runtime.InteropServices;

namespace BrainfuckCompiler
{
    internal class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int cmdShow);
    }
}
