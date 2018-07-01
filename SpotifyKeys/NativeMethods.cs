using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SpotifyKeys
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern short GlobalAddAtom(string lpString);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, Keys vk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
