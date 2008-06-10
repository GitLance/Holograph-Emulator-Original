using System;
using System.Text;

using Holo.Managers;
namespace Holo.Virtual.Users.Items
{
    /// <summary>
    /// Represents the song editor of a virtual soundmachine, containing the active soundsets etc.
    /// </summary>
    class virtualSongEditor
    {
        private int machineID;
        private int userID;
        private int[] Slot;
        internal virtualSongEditor(int machineID,int userID)
        {
            this.machineID = machineID;
            this.userID = userID;
            this.Slot = new int[4];
            loadSoundsets();
        }
        #region Soundset slot management
        internal void loadSoundsets()
        {
            for (int slotID = 0; slotID < 4; slotID++)
                Slot[slotID] = DB.runReadUnsafe("SELECT soundmachine_soundset FROM furniture WHERE soundmachine_machineid = '" + machineID + "' AND soundmachine_slot = '" + (slotID + 1) + "' ORDER BY id ASC", null);
        }
        internal void addSoundset(int soundSetID, int slotID)
        {
            Slot[slotID - 1] = soundSetID;
            DB.runQuery("UPDATE furniture SET roomid = '-3',soundmachine_machineid = '" + machineID + "',soundmachine_slot = '" + slotID + "' WHERE ownerid = '" + userID + "' AND soundmachine_soundset = '" + soundSetID + "' ORDER BY id ASC LIMIT 1");
        }
        internal void removeSoundset(int slotID)
        {
            Slot[slotID - 1] = 0;
            DB.runQuery("UPDATE furniture SET roomid = '0',soundmachine_machineid = NULL,soundmachine_slot = NULL WHERE ownerid = '" + userID + "' AND soundmachine_machineid = '" + machineID + "' AND soundmachine_slot = '" + slotID + "' LIMIT 1");
        }
        internal bool slotFree(int slotID)
        {
            return (Slot[slotID - 1] == 0);
        }
        internal string getSoundsets()
        {
            int Amount = 0;
            StringBuilder Soundsets = new StringBuilder();
            for (int slotID = 0; slotID < 4; slotID++)
            {
                int Soundset = Slot[slotID];
                if (Soundset > 0)
                {
                    Soundsets.Append(Encoding.encodeVL64(slotID + 1) + Encoding.encodeVL64(Soundset) + "QB"); // QB = 9, samples per set
                    int v = (Soundset * 9) - 8;
                    for (int j = v; j <= v + 8; j++)
                        Soundsets.Append(Encoding.encodeVL64(j));
                    Amount++;
                }
            }
            return "PA" + Encoding.encodeVL64(Amount) + Soundsets.ToString();
        }
        #endregion
    }
}
