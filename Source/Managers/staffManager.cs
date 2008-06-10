using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Holo.Managers
{
    /// <summary>
    /// Provides management for CFH and moderacy tasks for staff.
    /// </summary>
    public static class staffManager
    {
        /// <summary>
        /// Adds a staff message to the system_stafflog table, regarding one of the actions that a MOD/other staff member performed. Details in the message: type action, ID of the user (staffmember) that performed the action, the target user ID/room ID and the message + staff note.
        /// </summary>
        /// <param name="Action">The action performed. [alert, kick, ban, ralert, rkick]</param>
        /// <param name="userID">The ID of the staffmember that performed the action.</param>
        /// <param name="targetID">The ID of the target user/target room.</param>
        /// <param name="Message">The message that went with the action.</param>
        /// <param name="Note">A staff-only note for Housekeeping. [optional]</param>
        public static void addStaffMessage(string Action, int userID, int targetID, string Message, string Note)
        {
            DB.runQuery("INSERT INTO system_stafflog (action,userid,targetid,message,note,timestamp) VALUES ('" + Action + "','" + userID + "','" + targetID + "','" + DB.Stripslash(Message) + "','" + DB.Stripslash(Note) + "','" + DateTime.Now.ToString() + "')");
        }
    }
}
