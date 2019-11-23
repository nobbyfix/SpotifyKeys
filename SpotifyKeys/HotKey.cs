using System;
using System.Windows.Forms;

namespace SpotifyKeys
{
    public class HotKey : IMessageFilter, IDisposable
    {
        private bool disposed = false;
        
        private readonly IntPtr handle;
        private readonly int id;
        private readonly Keys key;
        private readonly KeyModifier[] modifier;
        private event EventHandler hotKeyPressed;

        public HotKey(IntPtr handle, int id, Keys key, KeyModifier[] modifier, EventHandler hotKeyPressed)
        {
            if(modifier == null)
                throw new System.ComponentModel.InvalidEnumArgumentException("Modifiers should not be null.");

            if (key == Keys.None || modifier.Length == 0)
                throw new System.ComponentModel.InvalidEnumArgumentException("The key or modifiers should not be none.");

            if(hotKeyPressed == null)
                throw new System.ComponentModel.InvalidEnumArgumentException("The HotKeyPressedEvent cannot be null.");

            this.handle = handle;
            this.id = id;
            this.key = key;
            this.modifier = modifier;
            this.hotKeyPressed += hotKeyPressed;
        }

        public void RegisterHotKey()
        {
            // convert from enum to numeric value
            int mod = 0;
            foreach(KeyModifier _modifier in modifier)
                mod += (int)_modifier;

            // register hotkey
            bool isKeyRegisterd = NativeMethods.RegisterHotKey(handle, id, mod, key);
            
            // check if function failed
            if (!isKeyRegisterd)
            {
                NativeMethods.UnregisterHotKey(IntPtr.Zero, id);

                // retry, if failed again throw exception
                isKeyRegisterd = NativeMethods.RegisterHotKey(handle, id, mod, key);
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
            if (m.Msg == WM_HOTKEY && m.HWnd == handle && m.WParam == (IntPtr) id && hotKeyPressed != null)
            {
                hotKeyPressed(this, EventArgs.Empty); 
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
            if (disposed)
                return;

            if (disposing)
            {
                Application.RemoveMessageFilter(this);
                NativeMethods.UnregisterHotKey(handle, id);
            }
            disposed = true;
        }
    }
}
