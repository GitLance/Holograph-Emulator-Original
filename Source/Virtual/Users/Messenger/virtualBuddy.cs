using System;
using Holo.Managers;
using Holo.Virtual.Users;

namespace Holo.Virtual.Users.Messenger
{
    /// <summary>
    /// Represents a virtual buddy used in the virtualMessenger object.
    /// </summary>
    class virtualBuddy
    {
        /// <summary>
        /// The database ID of this user.
        /// </summary>
        internal int userID;
        /// <summary>
        /// Indicates if this user was online at the moment of the latest ToString request.
        /// </summary>
        internal bool Online;
        /// <summary>
        /// Indicates if this user was in a room at the moment of the latest ToString request.
        /// </summary>
        private bool inRoom;

        /// <summary>
        /// Intializes a virtual buddy.
        /// </summary>
        /// <param name="userID">The database ID of this buddy.</param>
        internal virtualBuddy(int userID)
        {
            this.userID = userID;
            bool b = Updated; // Update
        }

        /// <summary>
        /// Updates the booleans for online and inroom, and returns if there has been changes (so: updates) since the last call to this bool.
        /// </summary>
        internal bool Updated
        {
            get
            {
                bool _Online = userManager.containsUser(userID);
                bool _inRoom = false;

                if (_Online)
                    _inRoom = (userManager.getUser(userID).roomUser != null);
                if (_inRoom != inRoom || _Online != Online)
                {
                    Online = _Online;
                    inRoom = _inRoom;
                    return true;
                }
                else
                    return false;
            }
        }
        /// <summary>
        /// Important to check the 'Updated' bool first. Returns the status string for a virtual buddy based on the statistics of the last call of 'Updated'.
        /// </summary>
        /// <param name="includeUsername">Specifies if to include the username in the string. Only required at first sending of packet in session of client.</param>
        internal string ToString(bool includeUsername)
        {
            string OUT = Encoding.encodeVL64(userID);
            if(includeUsername)
            {
                string Username = "";
                if(Online)
                    Username = userManager.getUser(userID)._Username;
                else
                    Username = DB.runRead("SELECT name FROM users WHERE id = '" + userID + "'");
                OUT += Username + Convert.ToChar(2);
            }

            if(Online)
            {
                OUT += "II";
                if(inRoom)
                    OUT += "I";
                else
                    OUT += "H";
                OUT += userManager.getUser(userID)._Figure;
            }
            else
                OUT += "IHH";

            return OUT + Convert.ToChar(2) + "H";
        }
    }
}
