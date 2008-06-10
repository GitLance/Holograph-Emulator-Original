using System;
using System.Text;

namespace Holo.Managers
{
    /// <summary>
    /// Provides management for virtual sound machines and virtual songs.
    /// </summary>
    public static class soundMachineManager
    {
        /// <summary>
        /// Returns the string with all the soundsets in the Hand of a certain user.
        /// </summary>
        /// <param name="userID">The database ID of the user to get the soundsets of.</param>
        public static string getHandSoundsets(int userID)
        {
            int[] itemIDs = DB.runReadColumn("SELECT id FROM furniture WHERE ownerid = '" + userID + "' AND roomid = '0' AND soundmachine_soundset > 0 ORDER BY id ASC", 0, null);
            StringBuilder Soundsets = new StringBuilder(Encoding.encodeVL64(itemIDs.Length));
            if (itemIDs.Length > 0)
            {
                int[] soundSets = DB.runReadColumn("SELECT soundmachine_soundset FROM furniture WHERE ownerid = '" + userID + "' AND roomid = '0' AND soundmachine_soundset > 0 ORDER BY id ASC", 0, null);
                for (int i = 0; i < itemIDs.Length; i++)
                    Soundsets.Append(Encoding.encodeVL64(soundSets[i]));
            }
            return Soundsets.ToString();
        }
        /// <summary>
        /// Returns the length of a song in seconds as an integer. The length is calculated by counting the notes on the four tracks, if an error occurs here, then -1 is returned as length.
        /// </summary>
        /// <param name="Data">The songdata. (all 4 tracks)</param>
        public static int calculateSongLength(string Data)
        {
            int songLength = 0;
            try
            {
                string[] Track = Data.Split(':');
                for (int i = 1; i < 8; i += 3)
                {
                    int trackLength = 0;
                    string[] Samples = Track[i].Split(';');
                    for (int j = 0; j < Samples.Length; j++)
                        trackLength += int.Parse(Samples[j].Substring(Samples[j].IndexOf(",") + 1));

                    if (trackLength > songLength)
                        songLength = trackLength;
                }
                return songLength;
            }
            catch { return -1; }
        }

        public static string getMachineSongList(int machineID)
        {
            int[] IDs = DB.runReadColumn("SELECT id FROM soundmachine_songs WHERE machineid = '" + machineID + "' ORDER BY id ASC", 0, null);
            StringBuilder Songs = new StringBuilder(Encoding.encodeVL64(IDs.Length));
            if (IDs.Length > 0)
            {
                int[] Lengths = DB.runReadColumn("SELECT length FROM soundmachine_songs WHERE machineid = '" + machineID + "' ORDER BY id ASC", 0, null);
                string[] Titles = DB.runReadColumn("SELECT title FROM soundmachine_songs WHERE machineid = '" + machineID + "' ORDER BY id ASC", 0);
                string[] burntFlags = DB.runReadColumn("SELECT burnt FROM soundmachine_songs WHERE machineid = '" + machineID + "' ORDER BY id ASC", 0);

                for (int i = 0; i < IDs.Length; i++)
                {
                    Songs.Append(Encoding.encodeVL64(IDs[i]) + Encoding.encodeVL64(Lengths[i]) + Titles[i] + Convert.ToChar(2));
                    if (burntFlags[i] == "1")
                        Songs.Append("I");
                    else
                        Songs.Append("H");
                }
            }
            return Songs.ToString();
        }
        public static string getMachinePlaylist(int machineID)
        {
            int[] IDs = DB.runReadColumn("SELECT songid FROM soundmachine_playlists WHERE machineid = '" + machineID + "' ORDER BY pos ASC", 0,null);
            StringBuilder Playlist = new StringBuilder("H" + Encoding.encodeVL64(IDs.Length));
            if (IDs.Length > 0)
            {
                for (int i = 0; i < IDs.Length; i++)
                {
                    string Title = DB.runRead("SELECT title FROM soundmachine_songs WHERE id = '" + IDs[i] + "'");
                    string Creator = DB.runRead("SELECT name FROM users WHERE id = '" + DB.runRead("SELECT userid FROM soundmachine_songs WHERE id = '" + IDs[i] + "'") + "'");
                    Playlist.Append(Encoding.encodeVL64(IDs[i]) + Encoding.encodeVL64(i + 1) + Title + Convert.ToChar(2) + Creator + Convert.ToChar(2));
                }
            }
            return Playlist.ToString();
        }
        public static string getSong(int songID)
        {
            string[] songData = DB.runReadRow("SELECT title,data FROM soundmachine_songs WHERE id = '" + songID + "'");
            if (songData.Length > 0)
                return Encoding.encodeVL64(songID) + songData[0] + Convert.ToChar(2) + songData[1] + Convert.ToChar(2);
            else
                return "holo.cast.soundmachine.song.unknown";
        }
    }
}
