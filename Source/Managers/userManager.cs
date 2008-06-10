using System;
using System.Text;
using System.Threading;
using System.Collections;

using Holo.Virtual.Users;

namespace Holo.Managers
{
    /// <summary>
    /// Provides management for logged in users, aswell for retrieving details such as ID/name and vice versa from the database.
    /// </summary>
    public static class userManager
    {
        private static Hashtable _Users = new Hashtable();
        private static Thread pingChecker;
        private static int _peakUserCount;

        /// <summary>
        /// Starts the pingchecker thread.
        /// </summary>
        public static void Init()
        {
            try { pingChecker.Abort(); }
            catch { }
            pingChecker = new Thread(new ThreadStart(checkPings));
            pingChecker.Priority = ThreadPriority.BelowNormal;
            pingChecker.Start();
        }
        /// <summary>
        /// Adds a virtualUser class together with the userID to the userManager. Login ticket will be nulled and previous logged in instances of this user will be dropped.
        /// </summary>
        /// <param name="userID">The ID of the user to add.</param>
        /// <param name="User">The virtualUser class of this user.</param>
        public static void addUser(int userID, virtualUser User)
        {
            if (_Users.ContainsKey(userID))
            {
                virtualUser oldUser = ((virtualUser)_Users[userID]);
                oldUser.Disconnect();
                if(_Users.ContainsKey(userID))
                    _Users.Remove(userID);
            }
            
            _Users.Add(userID,User);
            DB.runQuery("UPDATE users SET ticket_sso = NULL WHERE id = '" + userID + "' LIMIT 1");
            Out.WriteLine("User " + userID + " logged in. [" + User._Username + "]",Out.logFlags.BelowStandardAction);

            if(_Users.Count > _peakUserCount)
                _peakUserCount = _Users.Count;
        }
        /// <summary>
        /// Removes a user from the userManager. [if it exists]
        /// </summary>
        /// <param name="userID">The ID of the user to remove.</param>
        public static void removeUser(int userID)
        {
            if (_Users.ContainsKey(userID))
            {
                _Users.Remove(userID);
                Out.WriteLine("User [" + userID + "] disconnected.", Out.logFlags.BelowStandardAction);
            }
        }
        /// <summary>
        /// Returns a bool that indicates if the userManager contains a certain user.
        /// </summary>
        /// <param name="userID">The ID of the user.</param>
        public static bool containsUser(int userID)
        {
            return _Users.ContainsKey(userID);
        }
        /// <summary>
        /// Returns a bool that indicates if the userManager contains a certain user.
        /// </summary>
        /// <param name="userName">The username of the user.</param>
        public static bool containsUser(string userName)
        {
            int userID = getUserID(userName);
            return _Users.ContainsKey(userID);
        }

        /// <summary>
        /// Returns the current amount of users in the userManager.
        /// </summary>
        public static int userCount
        {
            get
            {
                return _Users.Count;
            }
        }
        /// <summary>
        /// Returns the peak amount of users in the userManager since boot.
        /// </summary>
        public static int peakUserCount
        {
            get
            {
                return _peakUserCount;
            }
        }

        /// <summary>
        /// Retrieves the ID of a user from the database.
        /// </summary>
        /// <param name="userName">The username of the user.</param>
        public static int getUserID(string userName)
        {
            return DB.runRead("SELECT id FROM users WHERE name = '" + userName + "'", null);
        }
        /// <summary>
        /// Retrieves the username of a user from the database.
        /// </summary>
        /// <param name="userID">The ID of the user.</param>
        public static string getUserName(int userID)
        {
            return DB.runRead("SELECT name FROM users WHERE id = '" + userID + "'");
        }
        /// <summary>
        /// Returns a bool that indicates if a user with a certain user ID exists in the database.
        /// </summary>
        /// <param name="userID">The ID of the user to check.</param>
        public static bool userExists(int userID)
        {
            return DB.checkExists("SELECT id FROM users WHERE id = '" + userID + "'");
        }

        /// <summary>
        /// Returns an int array with the ID's of the messenger friends of a certain user.
        /// </summary>
        /// <param name="userID">The ID of the user to get the friend ID's from.</param>
        public static int[] getUserFriendIDs(int userID)
        {
            try
            {
                ArrayList idBuilder = new ArrayList();
                int[] friendIDs = DB.runReadColumn("SELECT friendid FROM messenger_friendships WHERE userid = '" + userID + "'", 0, null);
                for (int i = 0; i < friendIDs.Length; i++)
                    idBuilder.Add(friendIDs[i]);
                friendIDs = DB.runReadColumn("SELECT userid FROM messenger_friendships WHERE friendid = '" + userID + "'", 0, null);
                for (int i = 0; i < friendIDs.Length; i++)
                    idBuilder.Add(friendIDs[i]);

                return (int[])idBuilder.ToArray(typeof(int));
            }
            catch
            {
                return new int[0];
            }
        }
        /// <summary>
        /// Returns a virtualUser class for a certain user
        /// </summary>
        /// <param name="userID">The ID of the user.</param>
        public static virtualUser getUser(int userID)
        {
            try { return (virtualUser)_Users[userID]; }
            catch { return null; }
        }
        /// <summary>
        /// Returns a virtualUser class for a certain user.
        /// </summary>
        /// <param name="userName">The username of the user.</param>
        public static virtualUser getUser(string userName)
        {
            int userID = getUserID(userName);
            return getUser(userID);
        }
        /// <summary>
        /// Sends a single packet to all connected clients.
        /// </summary>
        /// <param name="Data">The packet to send.</param>
        public static void sendData(string Data)
        {
            foreach (virtualUser User in _Users.Values)
                User.sendData(Data);
        }
        /// <summary>
        /// Sends a single packet to all active virtual users with the specified rank. Optionally you can include users who have a higher rank than the specified rank.
        /// </summary>
        /// <param name="Rank">The minimum rank that the virtual user required to receive the data.</param>
        /// <param name="includeHigher">Indicates if virtual users with a rank that's higher than the specified rank should also receive the packet.</param>
        /// <param name="Data">The packet to send.</param>
        public static void sendToRank(byte Rank, bool includeHigher, string Data)
        {
            foreach (virtualUser User in _Users.Values)
            {
                if (User._Rank < Rank || (includeHigher == false && User._Rank > Rank))
                    continue;
                else
                    User.sendData(Data);
            }
        }
        /// <summary>
        /// Inserts a single 'chat saying' to the system_chatlog table, together with username of sayer, room ID of sayer and the current timestamp.
        /// </summary>
        /// <param name="userName">The username of the sayer.</param>
        /// <param name="roomID">The ID of the room where the sayer is in.</param>
        /// <param name="Message">The message the sayer said.</param>
        public static void addChatMessage(string userName, int roomID, string Message)
        {
            DB.runQuery("INSERT INTO system_chatlog (username,roomid,mtime,message) VALUES ('" + userName + "','" + roomID + "',CURRENT_TIMESTAMP,'" + DB.Stripslash(Message) + "')");
        }
        /// <summary>
        /// Generates an info list about a certain user. If the user isn't found or has a higher rank than the info requesting user, then an access error message is returned. Otherwise, a report with user ID, username, rank, mission, credits amount, tickets amount, virtual birthday (signup date), real birthday, email address and last IP address. If the user is online, then information about the room the user currently is in (including ID and owner name) is supplied, otherwise, the last server access date is supplied.
        /// </summary>
        /// <param name="userID">The database ID of the user to generate the info of.</param>
        /// <param name="Rank">The rank of the user that requests this info. If this rank is lower than the rank of the target user, then there is no info returned.</param>
        public static string generateUserInfo(int userID, byte Rank)
        {
            string[] userDetails = DB.runReadRow("SELECT name,rank,mission,credits,tickets,email,birth,hbirth,ipaddress_last,lastvisit FROM users WHERE id = '" + userID + "' AND rank <= " + Rank);
            if (userDetails.Length == 0)
                return stringManager.getString("userinfo_accesserror");
            else
            {
                StringBuilder Info = new StringBuilder(stringManager.getString("userinfo_header") + "\r"); // Append header
                Info.Append(stringManager.getString("common_userid") + ": " + userID + "\r"); // Append user ID
                Info.Append(stringManager.getString("common_username") + ": " + userDetails[0] + "\r"); // Append username
                Info.Append(stringManager.getString("common_userrank") + ": " + userDetails[1] + "\r"); // Append rank
                Info.Append(stringManager.getString("common_usermission") + ": " + userDetails[2] + "\r"); // Append user's mission
                Info.Append(stringManager.getString("common_credits") + ": " + userDetails[3] + "\r"); // Append user's amount of credits
                Info.Append(stringManager.getString("common_tickets") + ": " + userDetails[4] + "\r"); // Append user's amount of tickets
                Info.Append(stringManager.getString("common_hbirth") + ": " + userDetails[7] + "\r\r"); // Append 'registered at' date + blank line
                Info.Append(stringManager.getString("common_birth") + ": " + userDetails[6] + "\r"); // Append real birthday
                Info.Append(stringManager.getString("common_email") + ": " + userDetails[5] + "\r"); // Append email address
                Info.Append(stringManager.getString("common_ip") + ": " + userDetails[8] + "\r\r"); // Append user's last used IP address

                if (_Users.ContainsKey(userID)) // User online
                {
                    virtualUser User = (virtualUser)_Users[userID];
                    string Location = "";
                    if(User._roomID == 0)
                        Location = stringManager.getString("common_hotelview");
                    else
                        Location = stringManager.getString("common_room") + " '" + DB.runReadUnsafe("SELECT name FROM rooms WHERE id = '" + User._roomID + "'") + "' [id: " + User._roomID + ", " + stringManager.getString("common_owner") + ": " + DB.runReadUnsafe("SELECT owner FROM rooms WHERE id = '" + User._roomID + "'") + "]"; // Roomname, room ID and name of the user that owns the room
                    Info.Append(stringManager.getString("common_location") + ": " + Location);
                }
                else // User is offline
                    Info.Append(stringManager.getString("common_lastaccess") + ": " + userDetails[9]); // Append last server access date

                return Info.ToString();
            }
        }
        /// <summary>
        /// (Re)bans a single user for a specified amount of hours and reason. If the user is online, then it receives the ban message and get's disconnected.
        /// </summary>
        /// <param name="userID">The database ID of the user to ban.</param>
        /// <param name="Hours">The amount of hours (starts now) till the ban is lifted.</param>
        /// <param name="Reason">The reason for the ban, that describes the user why it's account is blocked from the system.</param>
        public static void setBan(int userID, int Hours, string Reason)
        {
            string Expires = DateTime.Now.AddHours(Hours).ToString();
            DB.runQuery("DELETE FROM users_bans WHERE userid = '" + userID + "' LIMIT 1"); // Delete previous bans
            DB.runQuery("INSERT INTO users_bans (userid,date_expire,descr) VALUES ('" + userID + "','" + Expires + "','" + DB.Stripslash(Reason) + "')");

            if (_Users.ContainsKey(userID))
            {
                virtualUser User = ((virtualUser)_Users[userID]);
                User.sendData("@c" + Reason);
                User.Disconnect(1000);
            }
        }
        /// <summary>
        /// Checks if there are system bans for a certain IP address.
        /// If a ban is detected, it checks if it's already expired.
        /// If that is the case, then it lifts the ban.
        /// If there is a pending ban, it returns the reason that was supplied with the banning, otherwise, it returns "".
        /// </summary>
        /// <param name="IP">The IP address to check bans for.</param>
        public static string getBanReason(string IP)
        {
            if (DB.checkExists("SELECT ipaddress FROM users_bans WHERE ipaddress = '" + IP + "'"))
            {
                string banExpires = DB.runRead("SELECT date_expire FROM users_bans WHERE ipaddress = '" + IP + "'");
                if (DateTime.Compare(DateTime.Parse(banExpires), DateTime.Now) > 0)
                    return DB.runRead("SELECT descr FROM users_bans WHERE ipaddress = '" + IP + "'"); // Still banned, return reason
                else
                    DB.runQuery("DELETE FROM user_bans WHERE ipaddress = '" + IP + "' LIMIT 1");
            }

            return ""; // No pending ban/ban expired
        }
        /// <summary>
        /// (Re)bans all the users on a certain IP address, making them unable to login, and making them unable to connect to the system. The ban is applied with a specified amount and reason. All affected users receive the ban message (which contains the reason) and they are disconnected.
        /// </summary>
        /// <param name="IP">The IP address to ban.</param>
        /// <param name="Hours">The amount of hours (starts now) till the ban is lifted.</param>
        /// <param name="Reason">The reason for the ban, that describes thes user why their IP address/accounts are blocked from the system.</param>
        public static void setBan(string IP, int Hours, string Reason)
        {
            string Expires = DateTime.Now.AddHours(Hours).ToString();
            DB.runQuery("DELETE FROM users_bans WHERE ipaddress = '" + IP + "'"); // Delete previous bans
            DB.runQuery("INSERT INTO users_bans (ipaddress,date_expire,descr) VALUES ('" + IP + "','" + Expires + "','" + DB.Stripslash(Reason) + "')");
            
            int[] userIDs = DB.runReadColumn("SELECT id FROM users WHERE ipaddress_last = '" + IP + "'", 0, null);
            for (int i = 0; i < userIDs.Length; i++)
            {
                if (_Users.ContainsKey(userIDs[i]))
                {
                    virtualUser User = ((virtualUser)_Users[userIDs[i]]);
                    User.sendData("@c" + Reason);
                    User.Disconnect(1000);
                }
            }
        }
        /// <summary>
        /// Checks if there is a system ban for a certain user.
        /// If a ban is detected, it checks if it's already expired.
        /// If that is the case, then it lifts the ban.
        /// If there is a pending ban, it returns the reason that was supplied with the banning, otherwise, it returns "".
        /// </summary>
        /// <param name="userID">The database ID of the user to check for bans.</param>
        public static string getBanReason(int userID)
        {
            if (DB.checkExists("SELECT userid FROM users_bans WHERE userid = '" + userID + "'"))
            {
                string banExpires = DB.runRead("SELECT date_expire FROM users_bans WHERE userid = '" + userID + "'");
                if (DateTime.Compare(DateTime.Parse(banExpires), DateTime.Now) > 0) // Still banned, return reason
                    return DB.runRead("SELECT descr FROM users_bans WHERE userid = '" + userID + "'");
                else
                    DB.runQuery("DELETE FROM users_bans WHERE userid = '" + userID + "' LIMIT 1");
            }
            return ""; // No pending ban/ban expired
        }
        /// <summary>
        /// Generates a ban report for a certain ban on a user, including all details that could be of use. If there was no ban found, or the user that was banned doesn't exist, then a holo.cast.banreport.null is returned.
        /// </summary>
        /// <param name="userID">The database ID of the user to generate the ban report for.</param>
        public static string generateBanReport(int userID)
        {
            string[] banDetails = DB.runReadRow("SELECT date_expire,descr,ipaddress FROM users_bans WHERE userid = '" + userID + "'");
            string[] userDetails = DB.runReadRow("SELECT name,rank,ipaddress_last FROM users WHERE id = '" + userID + "'");
                
            if (banDetails.Length == 0 || userDetails.Length == 0)
                return "holo.cast.banreport.null";
            else
            {
                string Note = "-";
                string banPoster = "not available";
                string banPosted = "not available";
                string[] logEntries = DB.runReadRow("SELECT userid,note,timestamp FROM system_stafflog WHERE action = 'ban' AND targetid = '" + userID + "' ORDER BY id ASC"); // Get latest stafflog entry for this action (if exists)
                if(logEntries.Length > 0) // system_stafflog table could be cleaned up
                {
                    if(logEntries[1] != "")
                        Note = logEntries[1];
                    banPoster = DB.runRead("SELECT name FROM users WHERE id = '" + logEntries[0] + "'");
                    banPosted = logEntries[2];
                }

                StringBuilder Report = new StringBuilder(stringManager.getString("banreport_header") + " ");
                Report.Append(userDetails[0] + " [" + userID + "]" + "\r"); // Append username and user ID
                Report.Append(stringManager.getString("common_userrank") + ": " + userDetails[1] + "\r"); // Append user's rank
                Report.Append(stringManager.getString("common_ip") + ": " + userDetails[2] + "\r"); // Append the IP address of user
                Report.Append(stringManager.getString("banreport_banner") + ": " + banPoster + "\r"); // Append username of banner
                Report.Append(stringManager.getString("banreport_posted") + ": " + banPosted + "\r"); // Append datetime when ban was posted
                Report.Append(stringManager.getString("banreport_expires") + ": " + banDetails[0] + "\r"); // Append datetime when ban expires
                Report.Append(stringManager.getString("banreport_reason") + ": " + banDetails[1] + "\r"); // Append the reason that went with the ban
                Report.Append(stringManager.getString("banreport_ipbanflag") + ": " + (banDetails[2] != "").ToString().ToLower() + "\r"); // Append true/false for the IP ban status
                Report.Append(stringManager.getString("banreport_staffnote") + ": " + Note); // Append the staffnote that went with the ban

                return Report.ToString();
            }                                 
        }
        /// <summary>
        /// Generates a ban report for a certain IP address, including all details that could be of use. If there was no ban found, or the user that was banned doesn't exist, then a holo.cast.banreport.null is returned.
        /// </summary>
        /// <param name="IP">The IP address to generate the ban report for.</param>
        public static string generateBanReport(string IP)
        {
            string[] banDetails = DB.runReadRow("SELECT userid,date_expire,descr FROM users_bans WHERE ipaddress = '" + IP + "'");

            if (banDetails.Length == 0)
                return "holo.cast.banreport.null";
            else
            {
                string Note = "-";
                string banPoster = "not available";
                string banPosted = "not available";
                string[] logEntries = DB.runReadRow("SELECT userid,note,timestamp FROM system_stafflog WHERE action = 'ban' AND targetid = '" + banDetails[0] + "' ORDER BY id DESC"); // Get latest stafflog entry for this action (if exists)
                if (logEntries.Length > 0) // system_stafflog table could be cleaned up
                {
                    if (logEntries[1] != "")
                        Note = logEntries[1];
                    banPoster = DB.runRead("SELECT name FROM users WHERE id = '" + logEntries[0] + "'");
                    banPosted = logEntries[2];
                }

                StringBuilder Report = new StringBuilder(stringManager.getString("banreport_header") + " ");
                Report.Append(IP + "\r"); // Append IP address
                Report.Append(stringManager.getString("banreport_banner") + ": " + banPoster + "\r"); // Append username of banner
                Report.Append(stringManager.getString("banreport_posted") + ": " + banPosted + "\r"); // Append datetime when ban was posted
                Report.Append(stringManager.getString("banreport_expires") + ": " + banDetails[0] + "\r"); // Append datetime when ban expires
                Report.Append(stringManager.getString("banreport_reason") + ": " + banDetails[1] + "\r"); // Append the reason that went with the ban
                Report.Append(stringManager.getString("banreport_ipbanflag") + ": " + (banDetails[2] != "").ToString().ToLower() + "\r"); // Append true/false for the IP ban status
                Report.Append(stringManager.getString("banreport_staffnote") + ": " + Note + "\r\r"); // Append the staffnote that went with the ban
                
                string[] affectedUsernames = DB.runReadColumn("SELECT name FROM users WHERE ipaddress_last = '" + IP + "'", 0);
                Report.Append(stringManager.getString("banreport_affectedusernames") + ":");
                for (int i = 0; i < affectedUsernames.Length; i++) // Add usernames of user's that were affected by IP ban
                    Report.Append("\r - " + affectedUsernames[i]);

                return Report.ToString();
            }
        }
        /// <summary>
        /// Ran on a thread at interval 60000ms, checks ping status of users and disconnects timed out users.
        /// </summary>
        private static void checkPings()
        {
            while (true)
            {
                foreach (virtualUser User in ((Hashtable)_Users.Clone()).Values)
                {
                    if (User.pingOK)
                    {
                        User.pingOK = false;
                        User.sendData("@r");
                    }
                    else
                    {
                        Holo.Out.WriteLine(User._Username + " timed out.");
                        User.Disconnect();
                    }
                }
                Thread.Sleep(60000);
                Out.WriteTrace("Checking virtual user pings");
            }
        }
    }
}
