using System;
using System.Diagnostics;
using System.Globalization;
using System.Timers;
using Ini;

namespace SpotifyKeys
{
    public class RainmeterControl: IDisposable
    {
        private Timer timer = new Timer(500);
        public bool TimerRunning { get; private set; }

        public static bool IsRainmeterRunning() => Process.GetProcessesByName("Rainmeter").Length > 0;

        public static void RefreshVisualizer()
        {
            using (Process p = new Process())
            {
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.Arguments = "/c \"D:\\Programme\\Program Files\\Rainmeter\\Rainmeter.exe\" !Refresh monstercat-visualizer";
                p.StartInfo.FileName = @"c:\Windows\System32\cmd.exe";
                p.Start();
            }
        }

        public static void ChangeSensitivity(int sensitivity)
        {
            if (sensitivity > 70) sensitivity = 70;
            if (sensitivity <= 0) sensitivity = 1;

            IniFile inifile = new IniFile("D:\\Dokumente\\Rainmeter\\Skins\\monstercat-visualizer\\@Resources\\variables.ini");
            inifile.IniWriteValue("Variables", "Sensitivity", sensitivity.ToString(CultureInfo.InvariantCulture));
        }

        public void StartTimer()
        {
            TimerRunning = true;

            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Elapsed += (s_, e_) => TimerElapsed();

            timer.Start();

        }

        private void TimerElapsed() => TimerRunning = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                TimerRunning = false;
            }

            timer.Dispose();
        }
    }
}
