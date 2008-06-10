using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Holo
{
    /// <summary>
    /// Provides file, environment and minor string manipulation actions.
    /// </summary>
    public static class IO
    {
        #region DLL Imports
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        #endregion
        /// <summary>
        /// Returns the directory of the executeable (without backslash at end) as a string.
        /// </summary>
        public static string workingDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6);
            }
        }
        /// <summary>
        /// Returns the value of a private profile string in a textfile as a string.
        /// </summary>
        /// <param name="iniSection">The section where the value is located in.</param>
        /// <param name="iniKey">The key of the value.</param>
        /// <param name="iniLocation">The location of the textfile.</param>
        public static string readINI(string iniSection, string iniKey, string iniLocation)
        {
            StringBuilder _TMP = new StringBuilder(255);
            try
            {
                int i = GetPrivateProfileString(iniSection, iniKey, "", _TMP, 255, iniLocation);
                return _TMP.ToString();
            }
            catch
            {
                return "";
            }
        }
        /// <summary>
        /// Updates a value of a key in a textfile using WritePrivateProfileString.
        /// </summary>
        /// <param name="iniSection">The section where the key to update is located in.</param>
        /// <param name="iniKey">The key to update.</param>
        /// <param name="iniValue">The new value for the key.</param>
        /// <param name="iniLocation">The location of the textfile.</param>
        public static void writeINI(string iniSection, string iniKey, string iniValue, string iniLocation)
        {
            WritePrivateProfileString(iniSection, iniKey, iniValue, iniLocation);
        }
        /// <summary>
        /// Returns a bool, which indicates if the specified path leads to a file.
        /// </summary>
        /// <param name="fileLocation">The full location of the file.</param>
        public static bool fileExists(string fileLocation)
        {
            return File.Exists(fileLocation);
        }
    }
}
