using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using Holo.Managers;
using Holo.Virtual;
using Holo.Virtual.Users;
using Holo.Virtual.Rooms;

namespace Holo.Virtual.Users.Messenger
{
    /// <summary>
    /// Represents the messenger for a virtual user, which provides keeping buddy lists, instant messaging, inviting friends to a user's virtual room and various other features. The virtual messenger object provides voids for updating status of friends, instant messaging and more.
    /// </summary>
    class virtualMessenger
    {
        #region Declares
        /// <summary>
        /// The database ID of the parent virtual user.
        /// </summary>
        private int userID;
        private Hashtable Buddies;
        #endregion

        #region Constructors/destructors
        /// <summary>
        /// Initializes the virtual messenger for the parent virtual user, generating friendlist, friendrequests etc.
        /// </summary>
        /// <param name="userID">The database ID of the parent virtual user.</param>
        internal virtualMessenger(int userID)
        {
            this.userID = userID;
            this.Buddies = new Hashtable();
        }
        internal string friendList()
        {
            int[] userIDs = userManager.getUserFriendIDs(userID);
            StringBuilder Buddylist = new StringBuilder(Encoding.encodeVL64(200) + Encoding.encodeVL64(200) + Encoding.encodeVL64(600) + "H" + Encoding.encodeVL64(userIDs.Length));
            
            virtualBuddy Me = new virtualBuddy(userID);
            for (int i = 0; i < userIDs.Length; i++)
            {
                virtualBuddy Buddy = new virtualBuddy(userIDs[i]);
                try
                {
                    if (Buddy.Online)
                        userManager.getUser(userIDs[i]).Messenger.addBuddy(Me, true);
                }
                catch { }
                Buddies.Add(userIDs[i], Buddy);
                Buddylist.Append(Buddy.ToString(true));
            }
            Buddylist.Append(Encoding.encodeVL64(200) + "H");
            return Buddylist.ToString();
        }
        internal string friendRequests()
        {
            int[] userIDs = DB.runReadColumn("SELECT userid_from FROM messenger_friendrequests WHERE userid_to = '" + this.userID + "' ORDER by requestid ASC",0,null);
            StringBuilder Requests = new StringBuilder(Encoding.encodeVL64(userIDs.Length) + Encoding.encodeVL64(userIDs.Length));
            if(userIDs.Length > 0)
            {
                int[] requestIDs = DB.runReadColumn("SELECT requestid FROM messenger_friendrequests WHERE userid_to = '" + this.userID + "' ORDER BY requestid ASC",0,null);
                for(int i = 0; i < userIDs.Length; i++)
                    Requests.Append(Encoding.encodeVL64(requestIDs[i]) + DB.runRead("SELECT name FROM users WHERE id = '" + userIDs[i] + "'") + Convert.ToChar(2) + userIDs[i] + Convert.ToChar(2));
            }
            return Requests.ToString();
        }
        internal void Clear()
        {
            
        }

        internal void addBuddy(virtualBuddy Buddy, bool Update)
        {
            if (Buddies.ContainsKey(Buddy.userID) == false)
                Buddies.Add(Buddy.userID, Buddy);
            if(Update)
                User.sendData("@MHII" + Buddy.ToString(true));
        }
        /// <summary>
        /// Deletes a buddy from the friendlist and virtual messenger of this user, but leaves the database row untouched.
        /// </summary>
        /// <param name="ID">The database ID of the buddy to delete from the friendlist.</param>
        internal void removeBuddy(int ID)
        {
            User.sendData("@MHI" + "M" + Encoding.encodeVL64(ID));
            if (Buddies.Contains(ID))
                Buddies.Remove(ID);
        }
        internal string getUpdates()
        {
            int updateAmount = 0;
            StringBuilder Updates = new StringBuilder();
            try
            {
                foreach (virtualBuddy Buddy in Buddies.Values)
                {
                    if (Buddy.Updated)
                    {
                        updateAmount++;
                        Updates.Append("H" + Buddy.ToString(false));
                    }
                }
                return "H" + Encoding.encodeVL64(updateAmount) + Updates.ToString();
            }
            catch { return "HH"; }
        }
        #endregion
        /// <summary>
        /// Returns a boolean that indicates if the messenger contains a certain buddy, and this buddy is online.
        /// </summary>
        /// <param name="userID">The database ID of the buddy to check.</param>
        internal bool containsOnlineBuddy(int userID)
        {
            if (Buddies.ContainsKey(userID) == false)
                return false;
            else
                return userManager.containsUser(userID);
        }
        /// <summary>
        /// Returns a bool that indicates if there is a friendship between the parent virtual user and a certain user.
        /// </summary>
        /// <param name="userID">The database ID of the user to check.</param>
        internal bool hasFriendship(int userID)
        {
            return Buddies.ContainsKey(userID);
        }
        /// <summary>
        /// Returns a bool that indicates if there are friend requests hinth and forth between the the parent virtual user and a certain user.
        /// </summary>
        /// <param name="userID">The database ID of the user to check.</param>
        internal bool hasFriendRequests(int userID)
        {
            return DB.checkExists("SELECT requestid FROM messenger_friendrequests WHERE (userid_to = '" + this.userID + "' AND userid_from = '" + userID + "') OR (userid_to = '" + userID + "' AND userid_from = '" + this.userID + "')");
        }

        #region Object management
        /// <summary>
        /// Returns the parent virtual user instance of this virtual messenger.
        /// </summary>
        internal virtualUser User
        {
            get
            {
                return userManager.getUser(this.userID);
            }
        }
        #endregion
    }
}
