using System;
using System.Text;
using System.Collections;
using Microsoft.VisualBasic;

namespace Holo.Managers
{
    /// <summary>
    /// Provides functions for management and manipulation of string objects.
    /// </summary>
    public static class stringManager
    {
        /// <summary>
        /// Contains the strings loaded from system_strings.
        /// </summary>
        private static Hashtable langStrings;
        /// <summary>
        /// Contains the array of swearwords to be filtered from chat etc, loaded from system_wordfilter.
        /// </summary>
        private static string[] swearWords;
        /// <summary>
        /// Swearwords in chat etc should be replaced by this censor.
        /// </summary>
        private static string filterCensor;
        /// <summary>
        /// The language extension to use for the emulator.
        /// </summary>
        internal static string langExt;
        /// <summary>
        /// Initializes the string manager with a certain language.
        /// </summary>
        /// <param name="langExtension">The language to use for the emulator, eg, 'en' for English.</param>
        public static void Init(string langExtension)
        {
            langExt = langExtension;
            langStrings = new Hashtable();

            Out.WriteLine("Initializing strings from system_strings table for language '" + langExtension + "' ...");

            string[] langKeys = DB.runReadColumn("SELECT stringid FROM system_strings ORDER BY id ASC", 0);
            string[] langVars = DB.runReadColumn("SELECT var_" + langExt + " FROM system_strings ORDER BY id ASC", 0);

            for (int i = 0; i < langKeys.Length; i++)
            {
                if (langKeys[i] == "")
                    continue;
                if (langVars[i] == "")
                    langVars[i] = langKeys[i];
                langStrings.Add(langKeys[i], langVars[i]);
            }

            Out.WriteLine("Loaded " + langStrings.Count + " strings from system_strings table.");

            if (Config.getTableEntry("welcomemessage_enable") == "1")
            {
                if (getString("welcomemessage_text") != "")
                {
                    Config.enableWelcomeMessage = true;
                    Out.WriteLine("Welcome message enabled.");
                }
                else
                    Out.WriteLine("Welcome message was preferred as enabled, but has been left blank. Ignored.");
            }
            else
            {
                Out.WriteLine("Welcome message disabled.");
            }
        }
        /// <summary>
        /// Intializes/reloads the word filter, which filters swearwords etc from texts.
        /// </summary>
        public static void initFilter()
        {
            if (Config.getTableEntry("wordfilter_enable") == "1")
            {
                Out.WriteLine("Initializing word filter...");
                swearWords = DB.runReadColumn("SELECT word FROM wordfilter", 0);
                filterCensor = Config.getTableEntry("wordfilter_censor");

                if (swearWords.Length == 0 || filterCensor == "")
                    Out.WriteLine("Word filter was preferred as enabled but no words and/or replacement found, wordfilter disabled.");
                else
                {
                    Config.enableWordFilter = true;
                    Out.WriteLine("Word filter enabled, " + swearWords.Length + " word(s) found, replacement: " + filterCensor);
                }
            }
            else
            {
                Out.WriteLine("Word filter disabled.");
            }
        }
        /// <summary>
        /// Retrieves a system_strings entry for a certain key. The strings are loaded at the initialization of the string manager.
        /// </summary>
        /// <param name="stringID">The key of the string to retrieve.</param>
        public static string getString(string stringID)
        {
            try { return langStrings[stringID].ToString(); }
            catch { return stringID; }
        }
        /// <summary>
        /// Filters the swearwords in an input string and replaces them by the set censor.
        /// </summary>
        /// <param name="Text">The string to filter.</param>
        public static string filterSwearwords(string Text)
        {
            if (Config.enableWordFilter)
            {
                for (int i = 0; i < swearWords.Length; i++)
                    Text = Strings.Replace(Text, swearWords[i], filterCensor, 1, -1, Constants.vbTextCompare);
            }
            return Text;
        }
        /// <summary>
        /// Retrieves a substring from this instance. The substring starts at a specified character position and has a specified length. If any error occurs, then "" is returned.
        /// </summary>
        /// <param name="Input">The input string.</param>
        /// <param name="startIndex">The zero-based starting character position of a substring in this instance.</param>
        /// <param name="Length">The number of characters in the substring.</param>
        public static string getStringPart(string Input, int startIndex, int Length)
        {
            try { return Input.Substring(startIndex, Length); }
            catch { return ""; }
        }
        /// <summary>
        /// Wraps a string array of parameters to one string, separated by spaces.
        /// </summary>
        /// <param name="s">The string arrays with parameters.</param>
        /// <param name="startIndex">The parameter ID in the array to start off with. Parameters with lower IDs won't be included.</param>
        public static string wrapParameters(string[] s, int startIndex)
        {
            StringBuilder sb = new StringBuilder();
            //try
            {
                for (int i = startIndex; i < s.Length; i++)
                    sb.Append(" " + s[i]);

                return sb.ToString().Substring(1);
            }
            //catch { return ""; }
        }
    }
}
