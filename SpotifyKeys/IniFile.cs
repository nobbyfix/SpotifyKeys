using System.Runtime.InteropServices;
using System.Text;

namespace Ini
{
    internal static class NativeMethods
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        internal static extern uint GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
    }

    public static class IniFile
    {
        /// <summary>
        /// Write Data to the Ini File
        /// </summary>
        /// <param name="section">section name</param>
        /// <param name="key">key name</param>
        /// <param name="value">value name</param>
        /// <param name="path">file path</param>
        /// <returns>Returns true if the data could be written to the Ini File otherwise false.</returns>
        public static bool IniWriteValue(string section, string key, string value, string path)
        {
            if(NativeMethods.WritePrivateProfileString(section, key, value, path))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// read data value from the ini file
        /// </summary>
        /// <param name="section">section name</param>
        /// <param name="key">key name</param>
        /// <param name="path">file path</param>
        /// <param name="returnValue">value of key in given section</returns>
        /// <retruns>Returns size of read string, see also 'GetPrivateProfileString' documentation.</retruns>
        public static uint IniReadValue(string section, string key, string path, out string returnValue)
        {
            return IniReadValue(section, key, 255, path, out returnValue);
        }

        /// <summary>
        /// read data value from the ini file
        /// </summary>
        /// <param name="section">section name</param>
        /// <param name="key">key name</param>
        /// <param name="size">size of string buffer</param>
        /// <param name="path">file path</param>
        /// <param name="returnValue">value of key in given section</returns>
        /// <retruns>Returns size of read string, see also 'GetPrivateProfileString' documentation.</retruns>
        public static uint IniReadValue(string section, string key, int size, string path, out string returnValue)
        {
            StringBuilder stringBuilder = new StringBuilder(size);
            uint result = NativeMethods.GetPrivateProfileString(section, key, null, stringBuilder, size, path);
            returnValue = stringBuilder.ToString();
            return result;
        }
    }
}
