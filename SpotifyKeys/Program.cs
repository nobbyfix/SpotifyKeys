﻿using SpotifyKeys.Properties;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace SpotifyKeys
{
    static class Program
    {
        private static HotKey KeyVolumeUp;
        private static HotKey KeyVolumeDown;

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
            KeyVolumeUp = new HotKey(IntPtr.Zero, NativeMethods.GlobalAddAtom("SpotifyKeysVolumeUp"), defaultKeyUp, defaultKeyUpModifier, new EventHandler(VolumeUp));
            KeyVolumeUp.RegisterHotKey();
            KeyVolumeDown = new HotKey(IntPtr.Zero, NativeMethods.GlobalAddAtom("SpotifyKeysVolumeDown"), defaultKeyDown, defaultKeyDownModifier, new EventHandler(VolumeDown));
            KeyVolumeDown.RegisterHotKey();

            // create an icon in the tastbar to close the application
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem(Resources.exit);
            menuItem.Click += new EventHandler(ExitApplication);
            contextMenu.MenuItems.Add(0, menuItem);
            NotifyIcon trayIcon = new NotifyIcon
            {
                Text = Resources.appName,
                Icon = new Icon(SystemIcons.Application, 40, 40), // default application icon is used, may be changed
                ContextMenu = contextMenu,
                Visible = true
            };
            
            Application.Run();
        }

        // retrive process id by process name
        public static uint GetProcessID(string name)
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
        
        private static void ExitApplication(object sender, EventArgs e)
        {
            KeyVolumeUp.Dispose();
            KeyVolumeDown.Dispose();
            Application.Exit();
        }

        // method to decrease the volume level
        public static void VolumeDown(object sender, EventArgs e)
        {
            uint pid = Program.GetProcessID("Spotify");
            if (pid != 0)
            {
                float? fLevel = VolumeMixer.GetVolume(pid);
                if (fLevel != null)
                {
                    if(fLevel > 0)
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
                            while ((newLevel == level) && (step > 0))
                            {
                                step--;
                                newLevel = (float)Math.Truncate(Math.Pow(step, 4) / multiplier * 100) / 100;
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
            uint pid = Program.GetProcessID("Spotify");
            if (pid != 0)
            {
                float? fLevel = VolumeMixer.GetVolume(pid);
                if (fLevel != null)
                {
                    if(fLevel < 100)
                    {
                        float? fMasterVolume = VolumeMixer.GetMasterVolume();
                        if(fMasterVolume != null)
                        {
                            // calculate amount of steps using master volume
                            float masterVolume = fMasterVolume.Value;
                            double steps = Math.Round(2f / 5 * masterVolume);
                            steps = (steps < 1) ? 1 : steps;
                            double multiplier = Math.Pow(steps, 4) / 100;

                            float level = fLevel.Value;
                            float newLevel = level;
                            byte step = (byte)Math.Round(Math.Pow(multiplier * level, 1f / 4)); // calculate ctrlLevel from current volume, incase user or other program changes volume level
                            while ((newLevel == level) && (newLevel < 100))
                            {
                                step++;
                                newLevel = (float)Math.Truncate(Math.Pow(step, 4) / multiplier * 100) / 100;
                            }
                            VolumeMixer.SetVolume(pid, newLevel);
                        }
                    }
                }
            }
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
