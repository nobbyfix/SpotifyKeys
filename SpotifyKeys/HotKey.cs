using System;
using System.Windows.Forms;

namespace SpotifyKeys
{
    public class HotKey : IMessageFilter, IDisposable
    {
        bool Disposed = false;

        private readonly IntPtr Handle;
        private readonly int Id;
        private readonly Keys Key;
        private readonly KeyModifier[] Modifier;
        private event EventHandler HotKeyPressed;

        public HotKey(IntPtr handle, int id, Keys key, KeyModifier[] modifier, EventHandler hotKeyPressed)
        {
            if(modifier == null)
                throw new System.ComponentModel.InvalidEnumArgumentException("Modifiers should not be null.");

            if (key == Keys.None || modifier.Length == 0)
                throw new System.ComponentModel.InvalidEnumArgumentException("The key or modifiers should not be none.");

            if(hotKeyPressed == null)
                throw new System.ComponentModel.InvalidEnumArgumentException("The HotKeyPressedEvent cannot be null.");

            Handle = handle;
            Id = id;
            Key = key;
            Modifier = modifier;
            HotKeyPressed += hotKeyPressed;
        }

        public void RegisterHotKey()
        {
            // convert from enum to numeric value
            int mod = 0;
            foreach(KeyModifier modifier in Modifier)
                mod = mod + (int)modifier;

            // register hotkey
            bool isKeyRegisterd = NativeMethods.RegisterHotKey(Handle, Id, mod, Key);
            
            // check if function failed
            if (!isKeyRegisterd)
            {
                NativeMethods.UnregisterHotKey(IntPtr.Zero, Id);

                // retry, if failed again throw exception
                isKeyRegisterd = NativeMethods.RegisterHotKey(Handle, Id, mod, Key);
                if (!isKeyRegisterd)
                    throw new InvalidOperationException("The hotkey is in use.");
                else
                    Application.AddMessageFilter(this);
            } else
            {
                Application.AddMessageFilter(this);
            }
        }

        public bool PreFilterMessage(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY && m.HWnd == Handle && m.WParam == (IntPtr) Id && HotKeyPressed != null)
            {
                HotKeyPressed(this, EventArgs.Empty); 
                return true;
            }
            return false;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                Application.RemoveMessageFilter(this);
                NativeMethods.UnregisterHotKey(Handle, Id);
            }
            Disposed = true;
        }
    }
}
