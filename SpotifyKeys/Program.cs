using SpotifyKeys.Properties;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace SpotifyKeys
{
    static class Program
    {
        private static HotKey keyVolumeUp;
        private static HotKey keyVolumeDown;

        private static NotifyIcon trayIcon;

        [STAThread]
        public static void Main()
        {
            // default values for hotkeys
            Keys defaultKeyUp = Keys.Up;
            KeyModifier[] defaultKeyUpModifier = { KeyModifier.Control, KeyModifier.Alt };
            Keys defaultKeyDown = Keys.Down;
            KeyModifier[] defaultKeyDownModifier = { KeyModifier.Control, KeyModifier.Alt };

            // check if the application is already running, close the new instance if it's already running
            Process[] processes = Process.GetProcessesByName("SpotifyKeys");
            if (processes.Length > 1)
            {
                return;
            }

            // register default global hotkeys
            keyVolumeUp = new HotKey(IntPtr.Zero, NativeMethods.GlobalAddAtom("SpotifyKeysVolumeUp"), defaultKeyUp, defaultKeyUpModifier, new EventHandler(VolumeUp));
            keyVolumeUp.RegisterHotKey();
            keyVolumeDown = new HotKey(IntPtr.Zero, NativeMethods.GlobalAddAtom("SpotifyKeysVolumeDown"), defaultKeyDown, defaultKeyDownModifier, new EventHandler(VolumeDown));
            keyVolumeDown.RegisterHotKey();

            // create an icon in the taskbar to close the application
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem(Resources.exit);
            menuItem.Click += new EventHandler(ExitApplication);
            contextMenu.MenuItems.Add(0, menuItem);
            trayIcon = new NotifyIcon()
            {
                Text = Resources.appName,
                Icon = new Icon(SystemIcons.Application, 40, 40), // default application icon is used, may be changed
                ContextMenu = contextMenu,
                Visible = true
            };

            Application.Run();
        }

        /// <summary>
        /// Method to retrive the process id of a process by it's name
        /// </summary>
        /// <param name="name">process name</param>
        /// <returns></returns>
        public static uint GetWindowThreadProcessId(string name)
        {
            Process[] processes = Process.GetProcessesByName(name);
            if (processes.Length > 0)
            {
                foreach (Process p in processes)
                {
                    var hWnd = p.MainWindowHandle;
                    if (hWnd != IntPtr.Zero)
                    {
                        NativeMethods.GetWindowThreadProcessId(hWnd, out uint pid);
                        return pid;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Method which exits the application.
        /// </summary>
        private static void ExitApplication(object sender, EventArgs e)
        {
            keyVolumeUp.Dispose();
            keyVolumeDown.Dispose();
            trayIcon.Dispose();
            Application.Exit();
        }

        /// <summary>
        /// Method to change the volume up or down.
        /// </summary>
        /// <param name="volumeUp">Determined whether volume gets increased or decreased. True for increase.</param>
        private static void ChangeVolume(bool volumeUp)
        {
            uint pid = Program.GetWindowThreadProcessId("Spotify");
            if (pid != 0)
            {
                float? fLevel = VolumeMixer.GetVolume(pid);
                if (fLevel != null)
                {
                    if ((fLevel < 100 && volumeUp) || (fLevel > 0 && !volumeUp))
                    {
                        float? fMasterVolume = VolumeMixer.GetMasterVolume();
                        if (fMasterVolume != null)
                        {
                            // calculate amount of steps using master volume
                            float masterVolume = fMasterVolume.Value;
                            double steps = Math.Round(2f / 5 * masterVolume);
                            steps = (steps < 1) ? 1 : steps;
                            double multiplier = Math.Pow(steps, 4) / 100;

                            float level = fLevel.Value;
                            float newLevel = level;
                            byte step = (byte)Math.Round(Math.Pow(multiplier * level, 1f / 4)); // calculate ctrlLevel from current volume, incase user or other program changes volume level

                            if (volumeUp)
                            {
                                while ((newLevel == level) && (newLevel < 100))
                                {
                                    step++;
                                    newLevel = (float)Math.Truncate(Math.Pow(step, 4) / multiplier * 100) / 100;
                                }
                            }
                            else
                            {
                                while ((newLevel == level) && (step > 0))
                                {
                                    step--;
                                    newLevel = (float)Math.Truncate(Math.Pow(step, 4) / multiplier * 100) / 100;
                                }
                            }
                            VolumeMixer.SetVolume(pid, newLevel);
                        }
                    }
                }
            }
        }

        // method to increase the volume level
        public static void VolumeUp(object sender, EventArgs e)
        {
            ChangeVolume(true);
        }

        // method to decrease the volume level
        public static void VolumeDown(object sender, EventArgs e)
        {
            ChangeVolume(false);
        }
    }

    [Flags]
    public enum KeyModifier
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8,
        NoRepeat = 16384
    }
}
