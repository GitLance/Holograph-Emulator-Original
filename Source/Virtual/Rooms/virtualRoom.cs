using System;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using Holo.Managers;
using Holo.Virtual.Users;
using Holo.Virtual.Rooms.Bots;
using Holo.Virtual.Rooms.Items;

namespace Holo.Virtual.Rooms
{
    /// <summary>
    /// Represents a virtual publicroom or guestroom, with management for users, items and the map. Threaded.
    /// </summary>
    public class virtualRoom
    {
        #region Declares
        /// <summary>
        /// The ID of this room.
        /// </summary>
        internal int roomID;
        /// <summary>
        /// Indicates if this room is a publicroom.
        /// </summary>
        internal bool isPublicroom;
        /// <summary>
        /// Manages the flooritems inside the room.
        /// </summary>
        internal FloorItemManager floorItemManager;
        /// <summary>
        /// Manages the wallitems inside the room.
        /// </summary>
        internal WallItemManager wallItemManager;

        private string _publicroomItems;
        private string _Heightmap;
        /// <summary>
        /// Indicates if this room has a swimming pool.
        /// </summary>
        internal bool hasSwimmingPool;

        /// <summary>
        /// The state of a certain coord on the room map.
        /// </summary>
        private squareState[,] sqSTATE;
        /// <summary>
        /// The rotation of the item on a certain coord on the room map.
        /// </summary>
        private byte[,] sqITEMROT;
        /// <summary>
        /// The floorheight of a certain coord on the room's heightmap.
        /// </summary>
        private byte[,] sqFLOORHEIGHT;
        /// <summary>
        /// The height of the item on a certain coord on the room map.
        /// </summary>
        private double[,] sqITEMHEIGHT;
        /// <summary>
        /// Indicates if there is a user/bot/pet on a certain coord of the room map.
        /// </summary>
        private bool[,] sqUNIT;
        /// <summary>
        /// The item stack on a certain coord of the room map.
        /// </summary>
        private furnitureStack[,] sqSTACK;
        public enum squareState { Open = 0, Blocked = 1, Seat = 2, Bed = 3, Rug = 4 };
        squareTrigger[,] sqTRIGGER;

        /// <summary>
        /// The collection that contains the virtualRoomUser objects for the virtual users in this room.
        /// </summary>
        private Hashtable _Users;
        /// <summary>
        /// The collection that contains the virtualBot objects for the bots in this room.
        /// </summary>
        private Hashtable _Bots;
        /// <summary>
        /// The collection that contains the IDs of the virtual user groups that are active in this room.
        /// </summary>
        private HashSet<int> _activeGroups;
        /// <summary>
        /// The thread that handles the @b status updating and walking of virtual unit.
        /// </summary>
        private Thread _statusHandler;
        /// <summary>
        /// The string that contains the status updates for the next cycle of the _statusHandler thread.
        /// </summary>
        private StringBuilder _statusUpdates;

        /// <summary>
        /// The X position of the room's door.
        /// </summary>
        internal int doorX;
        /// <summary>
        /// The Y position of the room's door.
        /// </summary>
        internal int doorY;
        /// <summary>
        /// Publicroom only. The rotation that the user should gain when staying in the room's door.
        /// </summary>
        private byte doorZ;
        /// <summary>
        /// The height that the user should gain when staying in the room's door.
        /// </summary>
        private int doorH;

        /// <summary>
        /// Sends timed 'AG' casts to the room, such as disco lights and camera's.
        /// </summary>
        private Thread specialCastHandler;
        #endregion

        #region Constructors/Destructors
        /// <summary>
        /// Initializes a new instance of a virtual room. The room is prepared for usage.
        /// </summary>
        /// <param name="roomID"></param>
        /// <param name="isPublicroom"></param>
        public virtualRoom(int roomID, bool isPublicroom)
        {
            this.roomID = roomID;
            this.isPublicroom = isPublicroom;
            
            string roomModel = DB.runRead("SELECT model FROM rooms WHERE id = '" + roomID + "'");
            doorX = DB.runRead("SELECT door_x FROM room_modeldata WHERE model = '" + roomModel + "'", null);
            doorY = DB.runRead("SELECT door_y FROM room_modeldata WHERE model = '" + roomModel + "'", null);
            doorH = DB.runRead("SELECT door_h FROM room_modeldata WHERE model = '" + roomModel + "'", null);
            doorZ = byte.Parse(DB.runRead("SELECT door_z FROM room_modeldata WHERE model = '" + roomModel + "'"));
            _Heightmap = DB.runRead("SELECT heightmap FROM room_modeldata WHERE model = '" + roomModel + "'");

            string[] tmpHeightmap = _Heightmap.Split(Convert.ToChar(13));
            int colX = tmpHeightmap[0].Length;
            int colY = tmpHeightmap.Length - 1;

            sqSTATE = new squareState[colX, colY];
            sqFLOORHEIGHT = new byte[colX, colY];
            sqITEMROT = new byte[colX, colY];
            sqITEMHEIGHT = new double[colX, colY];
            sqUNIT = new bool[colX, colY];
            sqSTACK = new furnitureStack[colX, colY];
            sqTRIGGER = new squareTrigger[colX,colY];

            for (int y = 0; y < colY; y++)
            {
                for (int x = 0; x < colX; x++)
                {
                    string _SQ = tmpHeightmap[y].Substring(x, 1).Trim().ToLower();
                    if (_SQ == "x")
                        sqSTATE[x, y] = squareState.Blocked;
                    else
                    {
                        sqSTATE[x, y] = squareState.Open;
                        try { sqFLOORHEIGHT[x, y] = byte.Parse(_SQ); }
                        catch { }
                    }
                }
            }

            if (isPublicroom)
            {
                string[] Items = DB.runRead("SELECT publicroom_items FROM room_modeldata WHERE model = '" + roomModel + "'").Split("\n".ToCharArray());
                for (int i = 0; i < Items.Length; i++)
                {
                    string[] itemData = Items[i].Split(' ');
                    int X = int.Parse(itemData[2]);
                    int Y = int.Parse(itemData[3]);
                    squareState sType = (squareState)int.Parse(itemData[6]);
                    sqSTATE[X, Y] = sType;
                    if (sType == squareState.Seat)
                    {
                        sqITEMROT[X, Y] = byte.Parse(itemData[5]);
                        sqITEMHEIGHT[X, Y] = 1.0;
                    }

                    _publicroomItems += itemData[0] + " " + itemData[1] + " " + itemData[2] + " " + itemData[3] + " " + itemData[4] + " " + itemData[5] + Convert.ToChar(13);
                }

                int[] triggerIDs = DB.runReadColumn("SELECT id FROM room_modeldata_triggers WHERE model = '" + roomModel + "'",0,null);
                if (triggerIDs.Length > 0)
                {
                    for (int i = 0; i < triggerIDs.Length; i++)
                    {
                        string Object = DB.runRead("SELECT object FROM room_modeldata_triggers WHERE id = '" + triggerIDs[i] + "'");
                        int[] Nums = DB.runReadRow("SELECT x,y,goalx,goaly,stepx,stepy,roomid,state FROM room_modeldata_triggers WHERE id = '" + triggerIDs[i] + "'",null);
                        sqTRIGGER[Nums[0], Nums[1]] = new squareTrigger(Object, Nums[2], Nums[3],Nums[4],Nums[5],(Nums[7] == 1),Nums[6]);
                    }
                }

                if(DB.checkExists("SELECT specialcast_interval FROM room_modeldata WHERE model = '" + roomModel + "' AND specialcast_interval > 0"))
                {
                    specialCastHandler = new Thread(new ParameterizedThreadStart(handleSpecialCasts));
                    specialCastHandler.Priority = ThreadPriority.Lowest;
                    specialCastHandler.Start(roomModel);
                }

                hasSwimmingPool = DB.checkExists("SELECT swimmingpool FROM room_modeldata WHERE model = '" + roomModel + "' AND swimmingpool = '1'");
            }
            else
            {
                int[] itemIDs = DB.runReadColumn("SELECT id FROM furniture WHERE roomid = '" + roomID + "' ORDER BY h ASC",0,null);
                floorItemManager = new FloorItemManager(this);
                wallItemManager = new WallItemManager(this);

                if(itemIDs.Length > 0)
                {
                    int[] itemTIDs = DB.runReadColumn("SELECT tid FROM furniture WHERE roomid = '" + roomID + "' ORDER BY h ASC",0,null);
                    int[] itemXs = DB.runReadColumn("SELECT x FROM furniture WHERE roomid = '" + roomID + "' ORDER BY h ASC",0,null);
                    int[] itemYs = DB.runReadColumn("SELECT y FROM furniture WHERE roomid = '" + roomID + "' ORDER BY h ASC",0,null);
                    int[] itemZs = DB.runReadColumn("SELECT z FROM furniture WHERE roomid = '" + roomID + "' ORDER BY h ASC",0,null);
                    string[] itemHs = DB.runReadColumn("SELECT h FROM furniture WHERE roomid = '" + roomID + "' ORDER BY h ASC",0);
                    string[] itemVars = DB.runReadColumn("SELECT var FROM furniture WHERE roomid = '" + roomID + "' ORDER BY h ASC",0);
                    string[] itemWallPos = DB.runReadColumn("SELECT wallpos FROM furniture WHERE roomid = '" + roomID + "' ORDER BY h ASC",0);
                    
                    for(int i = 0; i < itemIDs.Length; i++)
                    {
                        if(itemWallPos[i] == "") // Flooritem
                            floorItemManager.addItem(itemIDs[i], itemTIDs[i], itemXs[i], itemYs[i], itemZs[i], double.Parse(itemHs[i]), itemVars[i]);
                        else // Wallitem
                            wallItemManager.addItem(itemIDs[i], itemTIDs[i], itemWallPos[i], itemVars[i], false);
                    }
                }
            }
            _Users = new Hashtable();
            _Bots = new Hashtable();
            loadBots();

            _activeGroups = new HashSet<int>();
            _statusUpdates = new StringBuilder();
            _statusHandler = new Thread(new ThreadStart(cycleStatuses));
            _statusHandler.Start();

            sqSTATE[doorX, doorY] = 0; // Door always walkable
        }
        /// <summary>
        /// Invoked by CRL garbage collector. Destroys all the remaining objects if all references to this object have been removed.
        /// </summary>
        ~virtualRoom()
        {
            //Holo.Out.WriteLine(".", Out.logFlags.ImportantAction, ConsoleColor.Cyan, ConsoleColor.DarkCyan);
        }
        #endregion

        #region Virtual content properties
        /// <summary>
        /// Returns the heightmap of this virtual room.
        /// </summary>
        internal string Heightmap
        {
            get
            {
                return _Heightmap;
            }
        }
        /// <summary>
        /// Returns a string with all the virtual flooritems in this room.
        /// </summary>
        internal string Flooritems
        {
            get
            {
                if (isPublicroom) // check for lido tiles
                    return "H";

                return floorItemManager.Items;
            }
        }
        /// <summary>
        /// Returns a string with all the virtual wallitems in this room.
        /// </summary>
        internal string Wallitems
        {
            get
            {
                if (isPublicroom)
                    return "";

                return wallItemManager.Items;
            }
        }
        /// <summary>
        /// Returns a string with all the virtual publicroom items in this virtual room.
        /// </summary>
        internal string PublicroomItems
        {
            get
            {
                return _publicroomItems;
            }
        }
        #endregion

        #region Virtual user management
        #region User and bot adding/removing
        internal void addUser(virtualUser User)
        {
            if (User._teleporterID == 0 && (User._ROOMACCESS_PRIMARY_OK == false || User._ROOMACCESS_SECONDARY_OK == false))
                return;

            User.statusManager = new virtualRoomUserStatusManager(User.userID, this.roomID);
            User.roomUser = new virtualRoomUser(User.userID, roomID, getFreeRoomIdentifier(), User, User.statusManager);

            if (User._teleporterID == 0)
            {
                User.roomUser.X = this.doorX;
                User.roomUser.Y = this.doorY;
                User.roomUser.Z1 = this.doorZ;
                User.roomUser.Z2 = this.doorZ;
                User.roomUser.H = this.doorH;
            }
            else
            {
                floorItem Teleporter = floorItemManager.getItem(User._teleporterID);
                User.roomUser.X = Teleporter.X;
                User.roomUser.Y = Teleporter.Y;
                User.roomUser.H = Teleporter.H;
                User.roomUser.Z1 = Teleporter.Z;
                User.roomUser.Z2 = Teleporter.Z;
                User._teleporterID = 0;
                sendData(@"A\" + Teleporter.ID + "/" + User._Username + "/" + Teleporter.Sprite);
            }
            User.roomUser.goalX = -1;
            _Users.Add(User.roomUser.roomUID, User.roomUser);

            if (this.isPublicroom == false)
            {
                if (User._hasRights)
                    if (User._isOwner == false) { User.statusManager.addStatus("flatctrl", "onlyfurniture"); }
                if (User._isOwner)
                    User.statusManager.addStatus("flatctrl", "useradmin");
                User.roomUser.hasVoted = DB.checkExists("SELECT userid FROM room_votes WHERE userid = '" + User.userID + "' AND roomid = '" + this.roomID + "'");
            }
            else
                if (this.hasSwimmingPool)
                    User.roomUser.SwimOutfit = DB.runRead("SELECT figure_swim FROM users WHERE id = '" + User.userID + "'");

            sendData(@"@\" + User.roomUser.detailsString);
            if (User._groupID > 0 && _activeGroups.Contains(User._groupID) == false)
            {
                string groupBadge = DB.runRead("SELECT badge FROM groups_details WHERE id = '" + User._groupID + "'");
                sendData("Du" + "I" + Encoding.encodeVL64(User._groupID) + groupBadge + Convert.ToChar(2));
                _activeGroups.Add(User._groupID);
            }
            roomManager.updateRoomVisitorCount(this.roomID, this._Users.Count);
        }
        /// <summary>
        /// Removes a room user from the virtual room.
        /// </summary>
        /// <param name="roomUID">The room identifier of the room user to remove.</param>
        /// <param name="sendKick">Specifies if the user must be kicked with the @R packet.</param>
        /// <param name="moderatorMessage">Specifies a moderator message [B!] packet to be used at kick.</param>
        internal void removeUser(int roomUID, bool sendKick, string moderatorMessage)
        {
            if (_Users.Contains(roomUID) == false)
                return;

            virtualRoomUser roomUser = (virtualRoomUser)_Users[roomUID];
            if (sendKick)
            {
                roomUser.User.sendData("@R");
                if (moderatorMessage != "")
                    roomUser.User.sendData("B!" + moderatorMessage + Convert.ToChar(2) + "holo.cast.modkick");
            }

            sqUNIT[roomUser.X, roomUser.Y] = false;
            roomUser.User.statusManager.Clear();
            roomUser.User.statusManager = null;
            roomUser.User._roomID = 0;
            roomUser.User._inPublicroom = false;
            roomUser.User._ROOMACCESS_PRIMARY_OK = false;
            roomUser.User._ROOMACCESS_SECONDARY_OK = false;
            roomUser.User._isOwner = false;
            roomUser.User._hasRights = false;
            roomUser.User.Room = null;
            roomUser.User.roomUser = null;

            _Users.Remove(roomUID);
            if (_Users.Count > 0) // Still users in room
            {
                if (roomUser.User._groupID > 0)
                {
                    bool removeBadge = true;
                    foreach (virtualRoomUser rUser in _Users.Values)
                    {
                        if (rUser.roomUID != roomUser.roomUID && rUser.User._groupID == roomUser.User._groupID)
                        {
                            removeBadge = false;
                            break;
                        }
                    }
                    if (removeBadge)
                        _activeGroups.Remove(roomUser.User._groupID);
                }

                sendData("@]" + roomUID);
                roomManager.updateRoomVisitorCount(this.roomID, _Users.Count);
            }
            else
            {
                _Users.Clear();
                _Bots.Clear();
                _statusUpdates = null;
                if (isPublicroom == false)
                {
                    floorItemManager.Clear();
                    wallItemManager.Clear();
                }
                try { specialCastHandler.Abort(); }
                catch { }

                roomManager.removeRoom(this.roomID);
                _statusHandler.Abort();
            }
        }
        /// <summary>
        /// Returns a bool that indicates if the room contains a certain room user.
        /// </summary>
        /// <param name="roomUID">The ID that identifies the user in the virtual room.</param>
        internal bool containsUser(int roomUID)
        {
            return _Users.ContainsKey(roomUID);
        }
        #endregion

        #region Properties and providers
        /// <summary>
        /// Returns a room identifier ID that isn't used by a virtual unit in this virtual room yet.
        /// </summary>
        /// <returns></returns>
        private int getFreeRoomIdentifier()
        {
            int i = 0;
            while (true)
            {
                if (_Bots.ContainsKey(i) == false && _Users.ContainsKey(i) == false)
                    return i;
                i++;
                Out.WriteTrace("Get free room identifier");
            }
        }
        /// <summary>
        /// Returns a room identifier of a virtual unit in this room, by picking a unit at random. If there are no units in the room, then -1 is returned.
        /// </summary>
        /// <returns></returns>
        private int getRandomRoomIdentifier()
        {
            if (_Users.Count > 0)
            {
                while (true)
                {
                    int rndID = new Random(DateTime.Now.Millisecond).Next(0, _Users.Count);
                    if (_Bots.ContainsKey(rndID) || _Users.ContainsKey(rndID))
                        return rndID;
                    Out.WriteTrace("Get random room identifier");
                }
            }
            else
                return -1;
        }
        /// <summary>
        /// Returns the virtualUser object of a room user.
        /// </summary>
        /// <param name="roomUID">The room identifier of the user.</param>
        internal virtualUser getUser(int roomUID)
        {
            return ((virtualRoomUser)_Users[roomUID]).User;
        }
        /// <summary>
        /// Returns the virtualRoomUser object of a room user.
        /// </summary>
        /// <param name="roomUID">The room identifier of the user.</param>
        internal virtualRoomUser getRoomUser(int roomUID)
        {
            return ((virtualRoomUser)_Users[roomUID]);
        }

        /// <summary>
        /// The details string for all the virtual units in this room.
        /// </summary>
        internal string dynamicUnits
        {
            get
            {
                StringBuilder userList = new StringBuilder();
                foreach (virtualBot roomBot in _Bots.Values)
                    userList.Append(roomBot.detailsString);
                foreach (virtualRoomUser roomUser in _Users.Values)
                    userList.Append(roomUser.detailsString);

                return userList.ToString();
            }
        }
        /// <summary>
        /// The status string of all the virtual units in this room.
        /// </summary>
        internal string dynamicStatuses
        {
            get
            {
                StringBuilder Statuses = new StringBuilder();
                foreach (virtualBot roomBot in _Bots.Values)
                    Statuses.Append(roomBot.statusString + Convert.ToChar(13));
                foreach (virtualRoomUser roomUser in _Users.Values)
                    Statuses.Append(roomUser.statusString + Convert.ToChar(13));

                return Statuses.ToString();
            }
        }
        /// <summary>
        /// The usernames of all the virtual users in this room.
        /// </summary>
        internal string Userlist
        {
            get
            {
                StringBuilder listBuilder = new StringBuilder(Encoding.encodeVL64(this.roomID) + Encoding.encodeVL64(_Users.Count));
                foreach (virtualRoomUser roomUser in _Users.Values)
                    listBuilder.Append(roomUser.User._Username + Convert.ToChar(2));

                return listBuilder.ToString();
            }
        }
        /// <summary>
        /// The IDs and badge strings of all the active user groups in this room.
        /// </summary>
        internal string Groups
        {
            get
            {
                StringBuilder listBuilder = new StringBuilder(Encoding.encodeVL64(_activeGroups.Count));
                foreach (int groupID in _activeGroups)
                    listBuilder.Append(Encoding.encodeVL64(groupID) + DB.runRead("SELECT badge FROM groups_details WHERE id = '" + groupID + "'") + Convert.ToChar(2));

                return listBuilder.ToString();
            }
        }
        #endregion

        #region User data distribution
        /// <summary>
        /// Sends a single packet to all users inside the user manager.
        /// </summary>
        /// <param name="Data">The packet to send.</param>
        internal void sendData(string Data)
        {
            try
            {
                foreach (virtualRoomUser roomUser in _Users.Values)
                    roomUser.User.sendData(Data);
            }
            catch { }
        }
        /// <summary>
        /// Sends a single packet to all users inside the user manager, after sleeping (on different thread) for a specified amount of milliseconds.
        /// </summary>
        /// <param name="Data">The packet to send.</param>
        /// <param name="msSleep">The amount of milliseconds to sleep before sending.</param>
        internal void sendData(string Data, int msSleep)
        {
            new sendDataSleep(SENDDATASLEEP).BeginInvoke(Data, msSleep, null, null);
        }
        private delegate void sendDataSleep(string Data, int msSleep);
        private void SENDDATASLEEP(string Data, int msSleep)
        {
            Thread.Sleep(msSleep);
            foreach (virtualRoomUser roomUser in _Users.Values)
                roomUser.User.sendData(Data);
        }
        /// <summary>
        /// Sends a single packet to a user in the usermanager.
        /// </summary>
        /// <param name="userID">The ID of the user.</param>
        /// <param name="Data">The packet to send.</param>
        internal void sendData(int userID, string Data)
        {
            foreach (virtualRoomUser roomUser in _Users.Values)
            {
                if (roomUser.userID == userID)
                {
                    roomUser.User.sendData(Data);
                    return;
                }
            }
        }
        /// <summary>
        /// Sends a single packet to a user in the usermanager.
        /// </summary>
        /// <param name="Username">The username of the user.</param>
        /// <param name="Data">The packet to send.</param>
        internal void sendData(string Username, string Data)
        {
            foreach (virtualRoomUser roomUser in _Users.Values)
            {
                if (roomUser.User._Username == Username)
                {
                    roomUser.User.sendData(Data);
                    return;
                }
            }
        }
        /// <summary>
        /// Sends a special cast to all users in the usermanager.
        /// </summary>
        /// <param name="Emitter">The objects that emits the cast.</param>
        /// <param name="Cast">The cast to emit.</param>
        internal void sendSpecialCast(string Emitter, string Cast)
        {
            sendData("AG" + Emitter + " " + Cast);
        }
        /// <summary>
        /// Updates the room votes amount for all users that have voted yet. User's that haven't voted yet are skipped so their vote buttons stay visible.
        /// </summary>
        /// <param name="voteAmount">The new amount of votes.</param>
        internal void sendNewVoteAmount(int voteAmount)
        {
            string Data = "EY" + Encoding.encodeVL64(voteAmount);
            foreach (virtualRoomUser roomUser in _Users.Values)
            {
                if (roomUser.hasVoted)
                    roomUser.User.sendData(Data);
            }
        }
        #endregion

        #region User and bot status management
        /// <summary>
        /// Ran on a thread and handles walking and pathfinding. All status updates are sent to all room users.
        /// </summary>
        private void cycleStatuses()
        {
            try
            {
                while (true)
                {
                    foreach (virtualRoomUser roomUser in ((Hashtable)_Users.Clone()).Values)
                    #region Virtual user status handling
                    {
                        if (roomUser.goalX == -1) // No destination set, user is not walking/doesn't start to walk, advance to next user
                            continue;

                        // If the goal is a seat, then allow to 'walk' on the seat, so seat the user
                        squareState[,] stateMap = (squareState[,])sqSTATE.Clone();
                        try
                        {
                            if (stateMap[roomUser.goalX, roomUser.goalY] == squareState.Seat || stateMap[roomUser.goalX, roomUser.goalY] == squareState.Bed)
                                stateMap[roomUser.goalX, roomUser.goalY] = squareState.Open;
                            if (sqUNIT[roomUser.goalX, roomUser.goalY])
                                stateMap[roomUser.goalX, roomUser.goalY] = squareState.Blocked;
                        }
                        catch { }
                        // Use AStar pathfinding to get the next step to the goal
                        int[] nextCoords = new Pathfinding.Pathfinder(stateMap, sqFLOORHEIGHT, sqUNIT).getNext(roomUser.X, roomUser.Y, roomUser.goalX, roomUser.goalY);

                        roomUser.statusManager.removeStatus("mv");
                        if (nextCoords == null) // No next steps found, destination reached/stuck
                        {
                            roomUser.goalX = -1; // Next time the thread cycles this user, it won't attempt to walk since destination has been reached
                            if (sqTRIGGER[roomUser.X, roomUser.Y] != null)
                            {
                                squareTrigger Trigger = sqTRIGGER[roomUser.X, roomUser.Y];
                                if (this.hasSwimmingPool) // This virtual room has a swimming pool
                                #region Swimming pool triggers
                                {
                                    if (Trigger.Object == "curtains1" || Trigger.Object == "curtains2") // User has entered a swimming pool clothing booth
                                    {
                                        roomUser.walkLock = true;
                                        roomUser.User.sendData("A`");
                                        sendSpecialCast(Trigger.Object, "close");
                                    }
                                    else if (roomUser.SwimOutfit != "") // User wears a swim outfit and hasn't entered a swimming pool clothing booth
                                    {
                                        if (Trigger.Object == "door" && roomUser.User._Tickets != 33333) // User has entered the diving board elevator
                                        {
                                            roomUser.walkLock = true;
                                            roomUser.goalX = -1;
                                            roomUser.goalY = 0;
                                            moveUser(roomUser, Trigger.stepX, Trigger.stepY, true);
                                            sendSpecialCast("door", "close");
                                            sendData("A}");
                                            roomUser.User._Tickets--;
                                            roomUser.User.sendData("A|" + roomUser.User._Tickets);
                                            DB.runQuery("UPDATE users SET tickets = tickets - 1 WHERE id = '" + roomUser.userID + "' LIMIT 1");
                                        }
                                        else if (Trigger.Object.Substring(0, 6) == "Splash") // User has entered/left a swimming pool
                                        {
                                            sendData("AG" + Trigger.Object);
                                            if (Trigger.Object.Substring(8) == "enter")
                                            {
                                                roomUser.statusManager.dropCarrydItem();
                                                roomUser.statusManager.addStatus("swim", "");
                                            }
                                            else
                                                roomUser.statusManager.removeStatus("swim");
                                            moveUser(roomUser, Trigger.stepX, Trigger.stepY, false);

                                            roomUser.goalX = Trigger.goalX;
                                            roomUser.goalY = Trigger.goalY;
                                        }
                                    }
                                }
                                #endregion
                                else
                                {
                                    // Different trigger species here
                                }
                            }
                            else if (roomUser.walkDoor) // User has clicked the door to leave the room (and got stuck while walking or reached destination)
                            {
                                if (_Users.Count > 1)
                                {
                                    removeUser(roomUser.roomUID, true, "");
                                    continue;
                                }
                                else
                                {
                                    removeUser(roomUser.roomUID, true, "");
                                    return;
                                }
                            }
                            _statusUpdates.Append(roomUser.statusString + Convert.ToChar(13));
                        }
                        else // Next steps found by pathfinder
                        {
                            int nextX = nextCoords[0];
                            int nextY = nextCoords[1];
                            squareState nextState = sqSTATE[nextX, nextY];

                            sqUNIT[roomUser.X, roomUser.Y] = false; // Free last position, allow other users to use that spot again
                            sqUNIT[nextX, nextY] = true; // Block the spot of the next steps
                            roomUser.Z1 = Pathfinding.Rotation.Calculate(roomUser.X, roomUser.Y, nextX, nextY); // Calculate the users new rotation
                            roomUser.Z2 = roomUser.Z1;
                            roomUser.statusManager.removeStatus("sit");
                            roomUser.statusManager.removeStatus("lay");

                            double nextHeight = 0;
                            if (nextState == squareState.Rug) // If next step is on a rug, then set user's height to that of the rug [floating stacked rugs in mid-air, petals etc]
                                nextHeight = sqITEMHEIGHT[nextX, nextY];
                            else
                                nextHeight = (double)sqFLOORHEIGHT[nextX, nextY];

                            // Add the walk status to users status manager + add users whole status string to stringbuilder
                            roomUser.statusManager.addStatus("mv", nextX + "," + nextY + "," + nextHeight.ToString().Replace(',', '.'));
                            _statusUpdates.Append(roomUser.statusString + Convert.ToChar(13));

                            // Set new coords for virtual room user
                            roomUser.X = nextX;
                            roomUser.Y = nextY;
                            roomUser.H = nextHeight;
                            if (nextState == squareState.Seat) // The next steps are on a seat, seat the user, prepare the sit status for next cycle of thread
                            {
                                roomUser.statusManager.removeStatus("dance"); // Remove dance status
                                roomUser.Z1 = sqITEMROT[nextX, nextY]; // 
                                roomUser.Z2 = roomUser.Z1;
                                roomUser.statusManager.addStatus("sit", sqITEMHEIGHT[nextX, nextY].ToString().Replace(',', '.'));
                            }
                            else if (nextState == squareState.Bed)
                            {
                                roomUser.statusManager.removeStatus("dance"); // Remove dance status
                                roomUser.Z1 = sqITEMROT[nextX, nextY]; // 
                                roomUser.Z2 = roomUser.Z1;
                                roomUser.statusManager.addStatus("lay", sqITEMHEIGHT[nextX, nextY].ToString().Replace(',', '.'));
                            }
                        }
                    }
                    #endregion

                    #region Roombot walking
                    foreach (virtualBot roomBot in _Bots.Values)
                    {
                        if (roomBot.goalX == -1)
                            continue;

                        // If the goal is a seat, then allow to 'walk' on the seat, so seat the user
                        squareState[,] stateMap = (squareState[,])sqSTATE.Clone();
                        try
                        {
                            if (stateMap[roomBot.goalX, roomBot.goalY] == squareState.Seat)
                                stateMap[roomBot.goalX, roomBot.goalY] = squareState.Open;
                            if (sqUNIT[roomBot.goalX, roomBot.goalY])
                                stateMap[roomBot.goalX, roomBot.goalY] = squareState.Blocked;
                        }
                        catch { }

                        int[] nextCoords = new Pathfinding.Pathfinder(stateMap, sqFLOORHEIGHT, sqUNIT).getNext(roomBot.X, roomBot.Y, roomBot.goalX, roomBot.goalY);

                        roomBot.removeStatus("mv");
                        if (nextCoords == null) // No next steps found, destination reached/stuck
                        {
                            if (roomBot.X == roomBot.goalX && roomBot.Y == roomBot.goalY)
                            {
                                roomBot.checkOrders();
                            }
                            roomBot.goalX = -1;
                            _statusUpdates.Append(roomBot.statusString + Convert.ToChar(13));
                        }
                        else
                        {
                            int nextX = nextCoords[0];
                            int nextY = nextCoords[1];

                            sqUNIT[roomBot.X, roomBot.Y] = false; // Free last position, allow other users to use that spot again
                            sqUNIT[nextX, nextY] = true; // Block the spot of the next steps
                            roomBot.Z1 = Pathfinding.Rotation.Calculate(roomBot.X, roomBot.Y, nextX, nextY); // Calculate the bot's new rotation
                            roomBot.Z2 = roomBot.Z1;
                            roomBot.removeStatus("sit");

                            double nextHeight = (double)sqFLOORHEIGHT[nextX, nextY];
                            if (sqSTATE[nextX, nextY] == squareState.Rug) // If next step is on a rug, then set bot's height to that of the rug [floating stacked rugs in mid-air, petals etc]
                                nextHeight = sqITEMHEIGHT[nextX, nextY];
                            else
                                nextHeight = (double)sqFLOORHEIGHT[nextX, nextY];

                            roomBot.addStatus("mv", nextX + "," + nextY + "," + nextHeight.ToString().Replace(',', '.'));
                            _statusUpdates.Append(roomBot.statusString + Convert.ToChar(13));

                            // Set new coords for the bot
                            roomBot.X = nextX;
                            roomBot.Y = nextY;
                            roomBot.H = nextHeight;
                            if (sqSTATE[nextX, nextY] == squareState.Seat) // The next steps are on a seat, seat the bot, prepare the sit status for next cycle of thread
                            {
                                roomBot.removeStatus("dance"); // Remove dance status
                                roomBot.Z1 = sqITEMROT[nextX, nextY]; // 
                                roomBot.Z2 = roomBot.Z1;
                                roomBot.addStatus("sit", sqITEMHEIGHT[nextX, nextY].ToString().Replace(',', '.'));
                            }
                        }
                    }
                    #endregion

                    // Send statuses to all room users [if in stringbuilder]
                    if (_statusUpdates.Length > 0)
                    {
                        sendData("@b" + _statusUpdates.ToString());
                        _statusUpdates = new StringBuilder();
                    }
                    Thread.Sleep(410);
                    Out.WriteTrace("Status update loop");
                } // Repeat (infinite loop on thread)
            }
            catch (Exception e) { Out.WriteError(e.Message); } // thread aborted
        }

        /// <summary>
        /// Updates the status of a virtualRoomUser object in room. If the user is walking, then the user isn't refreshed immediately but processed at the next cycle of the status thread, to prevent double status strings in @b.
        /// </summary>
        /// <param name="roomUser">The virtualRoomUser object to update.</param>
        internal void Refresh(virtualRoomUser roomUser)
        {
            if (roomUser.goalX == -1)
                _statusUpdates.Append(roomUser.statusString + Convert.ToChar(13));
        }
        /// <summary>
        /// Updates the status of a virtualBot object in room. If the bot is walking, then the bot isn't refreshed immediately but processed at the next cycle of the status thread, to prevent double status strings in @b.
        /// </summary>
        /// <param name="roomBot">The virtualBot object to update.</param>
        internal void Refresh(virtualBot roomBot)
        {
            try
            {
                if (roomBot.goalX == -1)
                    _statusUpdates.Append(roomBot.statusString + Convert.ToChar(13));
            }
            catch { }
        }
        internal void refreshCoord(int X, int Y)
        {
            foreach (virtualRoomUser roomUser in _Users.Values)
            {
                if (roomUser.X == X && roomUser.Y == Y)
                {
                    if (sqSTATE[X, Y] == squareState.Seat) // Seat here
                    {
                        if (roomUser.statusManager.containsStatus("sit") == false) // User is not sitting yet
                        {
                            roomUser.Z1 = sqITEMROT[X, Y];
                            roomUser.Z2 = roomUser.Z1;
                            roomUser.statusManager.addStatus("sit", sqITEMHEIGHT[X, Y].ToString().Replace(',', '.'));
                            Refresh(roomUser);
                        }
                    }
                    else if (sqSTATE[X, Y] == squareState.Bed) // Bed here
                    {
                        if (roomUser.statusManager.containsStatus("bed") == false) // User is not laying yet
                        {
                            roomUser.Z1 = sqITEMROT[X, Y];
                            roomUser.Z2 = roomUser.Z1;
                            roomUser.statusManager.addStatus("lay", sqITEMHEIGHT[X, Y].ToString().Replace(',', '.'));
                            Refresh(roomUser);
                        }
                    }
                    else // No seat/bed here
                    {
                        roomUser.statusManager.removeStatus("sit");
                        roomUser.statusManager.removeStatus("lay");
                        roomUser.H = sqFLOORHEIGHT[X, Y];
                        Refresh(roomUser);
                    }
                    return; // One user per coord
                }
            }
        }

        #region Single step walking
        /// <summary>
        /// Moves a virtual room user one step to a certain coord [the coord has to be one step removed from the room user's current coords], with pauses and handling for seats and rugs.
        /// </summary>
        /// <param name="roomUser">The virtual room user to move.</param>
        /// <param name="toX">The X of the destination coord.</param>
        /// <param name="toY">The Y of the destination coord.</param>
        internal void moveUser(virtualRoomUser roomUser, int toX, int toY, bool secondRefresh)
        {
            new userMover(MOVEUSER).BeginInvoke(roomUser, toX, toY, secondRefresh, null, null);
        }
        private delegate void userMover(virtualRoomUser roomUser, int toX, int toY, bool secondRefresh);
        private void MOVEUSER(virtualRoomUser roomUser, int toX, int toY, bool secondRefresh)
        {
            try
            {
                sqUNIT[roomUser.X, roomUser.Y] = false;
                sqUNIT[toX, toY] = true;
                roomUser.Z1 = Pathfinding.Rotation.Calculate(roomUser.X, roomUser.Y, toX, toY);
                roomUser.Z2 = roomUser.Z1;
                roomUser.statusManager.removeStatus("sit");
                double nextHeight = 0;
                if (sqSTATE[toX, toY] == squareState.Rug)
                    nextHeight = sqITEMHEIGHT[toX, toY];
                else
                    nextHeight = (double)sqFLOORHEIGHT[toX, toY];
                roomUser.statusManager.addStatus("mv", toX + "," + toY + "," + nextHeight.ToString().Replace(',','.'));
                sendData("@b" + roomUser.statusString);

                Thread.Sleep(310);
                roomUser.X = toX;
                roomUser.Y = toY;
                roomUser.H = nextHeight;

                roomUser.statusManager.removeStatus("mv");
                if (secondRefresh)
                {
                    if (sqSTATE[toX, toY] == squareState.Seat) // The next steps are on a seat, seat the user, prepare the sit status for next cycle of thread
                    {
                        roomUser.statusManager.removeStatus("dance"); // Remove dance status
                        roomUser.Z1 = sqITEMROT[toX, toY];
                        roomUser.Z2 = roomUser.Z1;
                        roomUser.statusManager.addStatus("sit", sqITEMHEIGHT[toX, toY].ToString().Replace(',', '.'));
                        roomUser.statusManager.removeStatus("mv");
                    }
                    sendData("@b" + roomUser.statusString);
                }
            }
            catch { }
        }
        #endregion
        #endregion

        #region Chat
        /// <summary>
        /// Sends a 'say' chat message from a virtualRoomUser to the room. Users and bots in a range of 5 squares will receive the message and bob their heads. Roombots will check the message and optionally interact to it.
        /// </summary>
        /// <param name="sourceUser">The virtualRoomUser object that sent the message.</param>
        /// <param name="Message">The message that was sent.</param>
        internal void sendSaying(virtualRoomUser sourceUser, string Message)
        {
            if (sourceUser.isTyping)
            {
                sendData("Ei" + Encoding.encodeVL64(sourceUser.roomUID) + "H");
                sourceUser.isTyping = false;
            }
            //sourceUser.statusManager.handleStatus("talk", "", Message.Length * 190);

            string Data = "@X" + Encoding.encodeVL64(sourceUser.roomUID) + Message + Convert.ToChar(2);
            foreach (virtualRoomUser roomUser in _Users.Values)
            {
                if (Math.Abs(roomUser.X - sourceUser.X) < 6 && Math.Abs(roomUser.Y - sourceUser.Y) < 6)
                {
                    //if (roomUser.roomUID != sourceUser.roomUID && roomUser.goalX == -1)
                    {
                        //byte newHeadRotation = Pathfinding.Rotation.headRotation(roomUser.Z2, roomUser.X, roomUser.Y, sourceUser.X, sourceUser.Y);
                        //if (newHeadRotation < 10 && newHeadRotation != roomUser.Z1) // Rotation changed
                        //{
                        //    roomUser.Z1 = newHeadRotation;
                        //    _statusUpdates.Append(roomUser.statusString + Convert.ToChar(13));
                        //}
                    }
                    roomUser.User.sendData(Data);
                }
            }

            foreach (virtualBot roomBot in _Bots.Values)
            {
                if (Math.Abs(roomBot.X - sourceUser.X) < 6 && Math.Abs(roomBot.Y - sourceUser.Y) < 6)
                    roomBot.Interact(sourceUser, Message);
            }
        }
        /// <summary>
        /// Sends a 'say' chat message from a virtualBot to the room. Users in a range of 5 squares will receive the message and bob their heads.
        /// </summary>
        /// <param name="sourceBot">The virtualBot object that sent the message.</param>
        /// <param name="Message">The message that was sent.</param>
        internal void sendSaying(virtualBot sourceBot, string Message)
        {
            string Data = "@X" + Encoding.encodeVL64(sourceBot.roomUID) + Message + Convert.ToChar(2);
            //sourceBot.handleStatus("talk", "", Message.Length * 190);

            foreach (virtualRoomUser roomUser in _Users.Values)
            {
                if (Math.Abs(roomUser.X - sourceBot.X) < 6 && Math.Abs(roomUser.Y - sourceBot.Y) < 6)
                {
                    //if (roomUser.goalX == -1)
                    //{
                    //    byte newHeadRotation = Pathfinding.Rotation.headRotation(roomUser.Z2, roomUser.X, roomUser.Y, sourceBot.X, sourceBot.Y);
                    //    if (newHeadRotation < 10 && newHeadRotation != roomUser.Z1) // Rotation changed
                    //    {
                    //        roomUser.Z1 = newHeadRotation;
                    //        _statusUpdates.Append(roomUser.statusString + Convert.ToChar(13));
                    //    }
                    //}
                    roomUser.User.sendData(Data);
                }
            }
        }
        /// <summary>
        /// Sends a 'shout' chat message from a virtualRoomUser to the room. All users will receive the message and bob their heads. Roombots have a 1/10 chance to react with the 'please don't shout message' set for them.
        /// </summary>
        /// <param name="sourceUser">The virtualRoomUser object that sent the message.</param>
        /// <param name="Message">The message that was sent.</param>
        internal void sendShout(virtualRoomUser sourceUser, string Message)
        {
            if (sourceUser.isTyping)
            {
                sendData("Ei" + Encoding.encodeVL64(sourceUser.roomUID) + "H");
                sourceUser.isTyping = false;
            }
            //sourceUser.statusManager.handleStatus("talk", "", Message.Length * 190);

            string Data = "@Z" + Encoding.encodeVL64(sourceUser.roomUID) + Message + Convert.ToChar(2);
            foreach (virtualRoomUser roomUser in _Users.Values)
            {
                //if (roomUser.roomUID != sourceUser.roomUID && roomUser.goalX == -1)
                //{
                    //byte newHeadRotation = Pathfinding.Rotation.headRotation(roomUser.Z2, roomUser.X, roomUser.Y, sourceUser.X, sourceUser.Y);
                    //if (newHeadRotation < 10 && newHeadRotation != roomUser.Z1) // Rotation changed
                    //{
                    //    roomUser.Z1 = newHeadRotation;
                        _statusUpdates.Append(roomUser.statusString + Convert.ToChar(13));
                    //}
                //}
                roomUser.User.sendData(Data);
            }

            foreach (virtualBot roomBot in _Bots.Values)
            {
                if (Math.Abs(roomBot.X - sourceUser.X) < 6 && Math.Abs(roomBot.Y - sourceUser.Y) < 6)
                {
                    if (new Random(DateTime.Now.Millisecond).Next(0, 11) == 0)
                        sendSaying(roomBot, roomBot.noShoutingMessage);
                }
            }
        }
        /// <summary>
        /// Sends a 'shout' chat message from a virtualBot to the room. All users will receive the message and bob their heads.
        /// </summary>
        /// <param name="sourceBot">The virtualRoomBot object that sent the message.</param>
        /// <param name="Message">The message that was sent.</param>
        internal void sendShout(virtualBot sourceBot, string Message)
        {
            //sourceBot.handleStatus("talk", "", Message.Length * 190);
            string Data = "@Z" + Encoding.encodeVL64(sourceBot.roomUID) + Message + Convert.ToChar(2);

            foreach (virtualRoomUser roomUser in ((Hashtable)_Users.Clone()).Values)
            {
                //if (roomUser.roomUID != sourceBot.roomUID && roomUser.goalX == -1)
                //{
                //    byte newHeadRotation = Pathfinding.Rotation.headRotation(roomUser.Z2, roomUser.X, roomUser.Y, sourceBot.X, sourceBot.Y);
                //    if (newHeadRotation < 10 && newHeadRotation != roomUser.Z1) // Rotation changed
                //    {
                //        roomUser.Z1 = newHeadRotation;
                //        _statusUpdates.Append(roomUser.statusString + Convert.ToChar(13));
                //    }
                //}
                roomUser.User.sendData(Data);
            }
        }
        /// <summary>
        /// Sends a 'whisper' chat message, which is only visible for sender and receiver, from a certain user to a certain user in the virtual room.
        /// </summary>
        /// <param name="sourceUser">The virtualRoomUser object of the sender.</param>
        /// <param name="Receiver">The username of the receiver.</param>
        /// <param name="Message">The message being sent.</param>
        internal void sendWhisper(virtualRoomUser sourceUser, string Receiver, string Message)
        {
            if (sourceUser.isTyping)
            {
                sendData("Ei" + Encoding.encodeVL64(sourceUser.roomUID) + "H");
                sourceUser.isTyping = false;
            }

            string Data = "@Y" + Encoding.encodeVL64(sourceUser.roomUID) + Message + Convert.ToChar(2);
            foreach (virtualRoomUser roomUser in _Users.Values)
            {
                if (roomUser.User._Username == Receiver)
                {
                    sourceUser.User.sendData(Data);
                    roomUser.User.sendData(Data);
                    return;
                }
            }
        }
        #endregion

        #region Moderacy tasks
        /// <summary>
        /// Casts a 'roomkick' on the user manager, kicking all users from the room [with message] who have a lower rank than the caster of the roomkick.
        /// </summary>
        /// <param name="casterRank">The rank of the caster of the 'room kick'.</param>
        /// <param name="Message">The message that goes with the 'roomkick'.</param>
        internal void kickUsers(byte casterRank, string Message)
        {
            foreach (virtualRoomUser roomUser in ((Hashtable)_Users.Clone()).Values)
            {
                if (roomUser.User._Rank < casterRank)
                    removeUser(roomUser.roomUID, true, Message);
            }
        }
        /// <summary>
        /// Casts a 'room mute' on the user manager, muting all users in the room who aren't muted yet and have a lower rank than the caster of the room mute. The affected users receive a message with the reason of their muting, and they won't be able to chat anymore until another user unmutes them.
        /// </summary>
        /// <param name="casterRank">The rank of the caster of the 'room mute'.</param>
        /// <param name="Message">The message that goes with the 'room mute'.</param>
        internal void muteUsers(byte casterRank, string Message)
        {
            Message = "BK" + stringManager.getString("scommand_muted") + "\r" + Message;
            foreach (virtualRoomUser roomUser in ((Hashtable)_Users.Clone()).Values)
            {
                if (roomUser.User._isMuted == false && roomUser.User._Rank < casterRank)
                {
                    roomUser.User._isMuted = true;
                    roomUser.User.sendData(Message);
                }
            }
        }
        /// <summary>
        /// Casts a 'room unmute' on the user manager, unmuting all users in the room who are muted and have a lower rank than the caster of the room mute. The affected users are notified that they can chat again.
        /// </summary>
        /// <param name="casterRank">The rank of the caster of the 'room unmute'.</param>
        internal void unmuteUsers(byte casterRank)
        {
            string Message = "BK" + stringManager.getString("scommand_unmuted");
            foreach (virtualRoomUser roomUser in ((Hashtable)_Users.Clone()).Values)
            {
                if (roomUser.User._isMuted && roomUser.User._Rank < casterRank)
                {
                    roomUser.User._isMuted = false;
                    roomUser.User.sendData(Message);
                }
            }
        }
        #endregion

        #region Bot loading and deloading
        internal void loadBots()
        {
            int[] IDs = DB.runReadColumn("SELECT id FROM roombots WHERE roomid = '" + this.roomID + "'", 0, null);
            for (int i = 0; i < IDs.Length; i++)
            {
                virtualBot roomBot = new virtualBot(IDs[i], getFreeRoomIdentifier(), this);
                roomBot.H = sqFLOORHEIGHT[roomBot.X, roomBot.Y];
                sqUNIT[roomBot.X, roomBot.Y] = true;

                _Bots.Add(roomBot.roomUID, roomBot);
                sendData(@"@\" + roomBot.detailsString);
                sendData("@b" + roomBot.statusString);
            }
        }
        #endregion
        #endregion

        #region Map management
        internal bool squareBlocked(int X, int Y, int Length, int Width)
        {
            for (int jX = X; jX < X + Width; jX++)
            {
                for (int jY = Y; jY < X + Length; jY++)
                {
                    if (sqUNIT[jX, jY])
                        return true;
                }
            }
            return false;
        }
        internal void setSquareState(int X, int Y, int Length, int Width, squareState State)
        {
            for (int jX = X; jX < X + Width; jX++)
            {
                for (int jY = Y; jY < Y + Length; jY++)
                {
                    sqSTATE[jX, jY] = State;
                }
            }
        }
        internal squareTrigger getTrigger(int X, int Y)
        {
            try {return sqTRIGGER[X, Y];}
            catch {return null;}
        }
        internal int[] getMapBorders()
        {
            int[] i = new int[2];
            i[0] = sqUNIT.GetUpperBound(0);
            i[1] = sqUNIT.GetUpperBound(1);
            return i;
        }
        /// <summary>
        /// Represents a trigger square that invokes a special event.
        /// </summary>
        internal class squareTrigger
        {
            /// <summary>
            /// The object of this trigger.
            /// </summary>
            internal readonly string Object;
            /// <summary>
            /// Optional. The new destination X of the virtual unit that invokes the trigger, walking.
            /// </summary>
            internal readonly int goalX;
            /// <summary>
            /// Optional. The new destination Y of the virtual unit that invokes the trigger, walking.
            /// </summary>
            internal readonly int goalY;
            /// <summary>
            /// Optional. The next X step of the virtual unit that invokes the trigger, stepping.
            /// </summary>
            internal readonly int stepX;
            /// <summary>
            /// Optional. The next Y step of the virtual unit that invokes the trigger, stepping.
            /// </summary>
            internal readonly int stepY;
            /// <summary>
            /// Optional. Optional. In case of a warp tile, this is the database ID of the destination room.
            /// </summary>
            internal readonly int roomID;
            /// <summary>
            /// Optional. A boolean flag for the trigger.
            /// </summary>
            internal bool State;
            /// <summary>
            /// Initializes the new trigger.
            /// </summary>
            /// <param name="Object">The object of this rigger.</param>
            /// <param name="goalX">Optional. The destination X of the virtual unit that invokes the trigger, walking.</param>
            /// <param name="goalY">Optional. The destination Y of the virtual unit that invokes the trigger, walking.</param>
            /// <param name="stepX">Optional. The next X step of the virtual unit that invokes the trigger, stepping.</param>
            /// <param name="stepY">Optional. The next Y step of the virtual unit that invokes the trigger, stepping.</param>
            /// <param name="roomID">Optional. In case of a warp tile, this is the database ID of the destination room.</param>
            /// <param name="State">Optional. A boolean flag for the trigger.</param>
            internal squareTrigger(string Object, int goalX, int goalY, int stepX, int stepY, bool State, int roomID)
            {
                this.Object = Object;
                this.goalX = goalX;
                this.goalY = goalY;
                this.stepX = stepX;
                this.stepY = stepY;
                this.roomID = roomID;
                this.State = State;
            }
        }
        #endregion

        #region Item managers
        /// <summary>
        /// Provides management for virtual flooritems in a virtual room.
        /// </summary>
        internal class FloorItemManager
        {
            private virtualRoom _Room;
            private Hashtable _Items = new Hashtable();
            /// <summary>
            /// The database ID of the soundmachine in this FloorItemManager.
            /// </summary>
            internal int soundMachineID;
            /// <summary>
            /// Initializes the manager.
            /// </summary>
            /// <param name="Room">The parent room.</param>
            public FloorItemManager(virtualRoom Room)
            {
                this._Room = Room;
            }
            /// <summary>
            /// Removes all the items from the item manager and destructs all objects inside.
            /// </summary>
            internal void Clear()
            {
                try { _Items.Clear(); }
                catch { }
                _Room = null;
                _Items = null;
            }
            /// <summary>
            /// Adds a new virtual flooritem to the manager at initialization.
            /// </summary>
            /// <param name="itemID">The ID of the new item.</param>
            /// <param name="templateID">The template ID of the new item.</param>
            /// <param name="X">The X position of the new item.</param>
            /// <param name="Y">The Y position of the new item.</param>
            /// <param name="Z">The Z [rotation] of the new item.</param>
            /// <param name="H">The H position [height] of the new item.</param>
            /// <param name="Var">The variable of the new item.</param>
            internal void addItem(int itemID, int templateID, int X, int Y, int Z, double H, string Var)
            {
                catalogueManager.itemTemplate Template = catalogueManager.getTemplate(templateID);
                if (stringManager.getStringPart(Template.Sprite, 0, 13) == "sound_machine")
                    soundMachineID = itemID;

                int Length = 0;
                int Width = 0;
                if (Z == 2 || Z == 6)
                {
                    Length = Template.Length;
                    Width = Template.Width;
                }
                else
                {
                    Length = Template.Width;
                    Width = Template.Length;
                }

                for (int jX = X; jX < X + Width; jX++)
                    for (int jY = Y; jY < Y + Length; jY++)
                    {
                        furnitureStack Stack = _Room.sqSTACK[jX,jY];
                        if (Stack == null)
                        {
                            if (Template.typeID != 2 && Template.typeID != 3)
                            {
                                Stack = new furnitureStack();
                                Stack.Add(itemID);
                            }
                        }
                        else
                            Stack.Add(itemID);

                        _Room.sqSTATE[jX, jY] = (squareState)Template.typeID;
                        if (Template.typeID == 2 || Template.typeID == 3)
                        {
                            _Room.sqITEMHEIGHT[jX, jY] = H + Template.topH;
                            _Room.sqITEMROT[jX, jY] = Convert.ToByte(Z);
                        }
                        else
                        {
                            if (Template.typeID == 4)
                                _Room.sqITEMHEIGHT[jX, jY] = H;
                        }
                        _Room.sqSTACK[jX, jY] = Stack;
                    }
                floorItem Item = new floorItem(itemID, templateID, X, Y, Z, H, Var);            
                _Items.Add(itemID, Item);
            }
            /// <summary>
            /// Removes a virtual flooritem from the item manager, handles the heightmap, makes it disappear in room and returns it back to the owners hand, or deletes it.
            /// </summary>
            /// <param name="itemID">The ID of the item to remove.</param>
            /// <param name="ownerID">The ID of the user who owns this item. If 0, then the item will be dropped from the database.</param>
            internal void removeItem(int itemID, int ownerID)
            {
                if(_Items.ContainsKey(itemID))
                {
                    floorItem Item = (floorItem)_Items[itemID];
                    catalogueManager.itemTemplate Template = catalogueManager.getTemplate(Item.templateID);
                    
                    int Length = 0;
                    int Width = 0;
                    if(Item.Z == 2 || Item.Z == 6)
                    {
                        Length = Template.Length;
                        Width = Template.Width;
                    }
                    else
                    {
                        Length = Template.Width;
                        Width = Template.Length;
                    }

                    for (int jX = Item.X; jX < Item.X + Width; jX++)
                    {
                        for (int jY = Item.Y; jY < Item.Y + Length; jY++)
                        {
                            furnitureStack Stack = _Room.sqSTACK[jX, jY];
                            if (Stack != null && Stack.Count > 1)
                            {
                                if (itemID == Stack.bottomItemID())
                                {
                                    int topID = Stack.topItemID();
                                    floorItem topItem = (floorItem)_Items[topID];
                                    if (catalogueManager.getTemplate(topItem.templateID).typeID == 2)
                                        _Room.sqSTATE[jX, jY] = squareState.Seat;
                                    else
                                        _Room.sqSTATE[jX, jY] = 0;
                                }
                                else if (itemID == Stack.topItemID())
                                {
                                    int belowID = Stack.getBelowItemID(itemID);
                                    floorItem belowItem = (floorItem)_Items[belowID];
                                    byte typeID = catalogueManager.getTemplate(belowItem.templateID).typeID;

                                    _Room.sqSTATE[jX, jY] = (squareState)typeID;
                                    if (typeID == 2 || typeID == 3)
                                    {
                                        _Room.sqITEMROT[jX, jY] = belowItem.Z;
                                        _Room.sqITEMHEIGHT[jX, jY] = belowItem.H + catalogueManager.getTemplate(belowItem.templateID).topH;
                                    }
                                    else if (typeID == 4)
                                    {
                                        _Room.sqITEMHEIGHT[jX, jY] = belowItem.H;
                                    }
                                }
                                Stack.Remove(itemID);
                                _Room.sqSTACK[jX, jY] = Stack;
                            }
                            else
                            {
                                _Room.sqSTATE[jX, jY] = 0;
                                _Room.sqITEMHEIGHT[jX, jY] = 0;
                                _Room.sqITEMROT[jX, jY] = 0;
                                _Room.sqSTACK[jX, jY] = null;
                            }
                            if(Template.typeID == 2 || Template.typeID == 3)
                                _Room.refreshCoord(jX, jY);
                        }
                    }

                    if (this.soundMachineID == 0 && stringManager.getStringPart(Template.Sprite, 0, 13) == "sound_machine")
                        soundMachineID = 0;
                    
                    _Room.sendData("A^" + itemID);
                    _Items.Remove(itemID);
                    if (ownerID > 0) // Return to current owner/new owner
                        DB.runQuery("UPDATE furniture SET x = '0',y = '0',z = '0', h = '0', ownerid = '" + ownerID + "',roomid = '0' WHERE id = '" + itemID + "' LIMIT 1");
                    else
                        DB.runQuery("DELETE FROM furniture WHERE id = '" + itemID + "' LIMIT 1");
                }
            }
            internal void placeItem(int itemID, int templateID, int X, int Y, byte typeID, byte Z)
            {
                if (_Items.ContainsKey(itemID))
                    return;

                try
                {
                    catalogueManager.itemTemplate Template = catalogueManager.getTemplate(templateID);
                    bool isSoundMachine = (stringManager.getStringPart(Template.Sprite, 0, 13) == "sound_machine");
                    if (isSoundMachine && soundMachineID > 0)
                        return;

                    int Length = 0;
                    int Width = 0;
                    if (Z == 2 || Z == 6)
                    {
                        Length = Template.Length;
                        Width = Template.Width;
                    }
                    else
                    {
                        Length = Template.Width;
                        Width = Template.Length;
                    }

                    double testH = _Room.sqFLOORHEIGHT[X, Y];
                    double H = testH;
                    if (_Room.sqSTACK[X, Y] != null)
                    {
                        floorItem topItem = (floorItem)_Items[_Room.sqSTACK[X, Y].topItemID()];
                        H = topItem.H + catalogueManager.getTemplate(topItem.templateID).topH;
                    }

                    for (int jX = X; jX < X + Width; jX++)
                    {
                        for (int jY = Y; jY < Y + Length; jY++)
                        {
                            if (_Room.sqUNIT[jX, jY]) // Dynamical unit here
                                return;

                            squareState jState = _Room.sqSTATE[jX, jY];
                            if (jState != squareState.Open)
                            {
                                if (jState == squareState.Blocked)
                                {
                                    if (_Room.sqSTACK[jX, jY] == null) // Square blocked and no stack here
                                        return;
                                    else
                                    {
                                        floorItem topItem = (floorItem)_Items[_Room.sqSTACK[jX, jY].topItemID()];
                                        catalogueManager.itemTemplate topItemTemplate = catalogueManager.getTemplate(topItem.templateID);
                                        if (topItemTemplate.topH == 0 || topItemTemplate.typeID == 2 || topItemTemplate.typeID == 3) // No stacking on seat/bed
                                            return;
                                        else
                                        {
                                            if (topItem.H + topItemTemplate.topH > H) // Higher than previous topheight
                                                H = topItem.H + topItemTemplate.topH;
                                        }
                                    }
                                }
                                else if (jState == squareState.Rug && _Room.sqSTACK[jX, jY] != null)
                                {
                                    double jH = ((floorItem)_Items[_Room.sqSTACK[jX, jY].topItemID()]).H + 0.1;
                                    if (jH > H)
                                        H = jH;
                                }
                                else // Seat etc
                                    return;
                            }
                        }
                    }

                    if (H > Config.Items_Stacking_maxHeight)
                        H = Config.Items_Stacking_maxHeight;

                    for (int jX = X; jX < X + Width; jX++)
                    {
                        for (int jY = Y; jY < Y + Length; jY++)
                        {
                            furnitureStack Stack = null;
                            if (_Room.sqSTACK[jX, jY] == null)
                            {
                                if ((Template.typeID == 1 && Template.topH > 0) || Template.typeID == 4)
                                {
                                    Stack = new furnitureStack();
                                    Stack.Add(itemID);
                                }
                            }
                            else
                            {
                                Stack = _Room.sqSTACK[jX, jY];
                                Stack.Add(itemID);
                            }

                            _Room.sqSTATE[jX, jY] = (squareState)Template.typeID;
                            _Room.sqSTACK[jX, jY] = Stack;
                            if (Template.typeID == 2 || Template.typeID == 3)
                            {
                                _Room.sqITEMHEIGHT[jX, jY] = H + Template.topH;
                                _Room.sqITEMROT[jX, jY] = Z;
                            }
                            else if (Template.typeID == 4)
                                _Room.sqITEMHEIGHT[jX, jY] = H;
                        }
                    }

                    string Var = DB.runRead("SELECT var FROM furniture WHERE id = '" + itemID + "'");
                    DB.runQuery("UPDATE furniture SET roomid = '" + _Room.roomID + "',x = '" + X + "',y = '" + Y + "',z = '" + Z + "',h = '" + H.ToString().Replace(',','.') + "' WHERE id = '" + itemID + "' LIMIT 1");
                    floorItem Item = new floorItem(itemID, templateID, X, Y, Z, H, Var);
                    _Items.Add(itemID, Item);
                    _Room.sendData("A]" + Item.ToString());

                    if(isSoundMachine)
                        this.soundMachineID = itemID;
                }
                catch { }
            }
            internal void rotateItem(int itemID, byte Z)
            {
                floorItem Item = (floorItem)_Items[itemID];
                catalogueManager.itemTemplate Template = catalogueManager.getTemplate(Item.templateID);

                int Length = 1;
                int Width = 1;
                if (Template.Length > 1 && Template.Width > 1)
                {
                    if (Z == 2 || Z == 6)
                    {
                        Length = Template.Length;
                        Width = Template.Width;
                    }
                    else
                    {
                        Length = Template.Width;
                        Width = Template.Length;
                    }
                }

                for (int jX = Item.X; jX < Item.X + Width; jX++)
                    for (int jY = Item.Y; jY < Item.Y + Length; jY++)
                    {
                        furnitureStack Stack = _Room.sqSTACK[jX, jY];
                        if (Stack != null && Stack.topItemID() != itemID)
                        {

                        }
                    }

            }
            internal void relocateItem(int itemID, int X, int Y, byte Z)
            {
                try
                {
                    floorItem Item = (floorItem)_Items[itemID];
                    catalogueManager.itemTemplate Template = catalogueManager.getTemplate(Item.templateID);

                    int Length = 0;
                    int Width = 0;
                    if (Z == 2 || Z == 6)
                    {
                        Length = Template.Length;
                        Width = Template.Width;
                    }
                    else
                    {
                        Length = Template.Width;
                        Width = Template.Length;
                    }

                    double baseFloorH = _Room.sqFLOORHEIGHT[X, Y];
                    double H = baseFloorH;
                    if (_Room.sqSTACK[X, Y] != null)
                    {
                        floorItem topItem = (floorItem)_Items[_Room.sqSTACK[X, Y].topItemID()];
                        if (topItem != Item)
                        {
                            catalogueManager.itemTemplate topTemplate = catalogueManager.getTemplate(topItem.templateID);
                            if (topTemplate.typeID == 1)
                                H = topItem.H + topTemplate.topH;
                        }
                        else if (_Room.sqSTACK[X, Y].Count > 1)
                            H = topItem.H;
                    }

                    for (int jX = X; jX < X + Width; jX++)
                    {
                        for (int jY = Y; jY < Y + Length; jY++)
                        {
                            if (Template.typeID != 2 && _Room.sqUNIT[jX, jY])
                                return;

                            squareState jState = _Room.sqSTATE[jX, jY];
                            furnitureStack Stack = _Room.sqSTACK[jX, jY];
                            if (jState != squareState.Open)
                            {
                                if (Stack == null)
                                {
                                    if (jX != Item.X || jY != Item.Y)
                                        return;
                                }
                                else
                                {
                                    floorItem topItem = (floorItem)_Items[Stack.topItemID()];
                                    if (topItem != Item)
                                    {
                                        catalogueManager.itemTemplate topItemTemplate = catalogueManager.getTemplate(topItem.templateID);
                                        if (topItemTemplate.typeID == 1 && topItemTemplate.topH > 0)
                                        {
                                            if (topItem.H + topItemTemplate.topH > H)
                                                H = topItem.H + topItemTemplate.topH;
                                        }
                                        else
                                        {
                                            if (topItemTemplate.typeID == 2)
                                                return;
                                        }
                                        //return;
                                    }
                                }
                            }
                        }
                    }

                    int oldLength = 1;
                    int oldWidth = 1;
                    if (Template.Length > 1 || Template.Width > 1)
                    {
                        if (Item.Z == 2 || Item.Z == 6)
                        {
                            oldLength = Template.Length;
                            oldWidth = Template.Width;
                        }
                        else
                        {
                            oldLength = Template.Width;
                            oldWidth = Template.Length;
                        }
                    }
                    if (H > Config.Items_Stacking_maxHeight)
                        H = Config.Items_Stacking_maxHeight;

                    for (int jX = Item.X; jX < Item.X + oldWidth; jX++)
                    {
                        for (int jY = Item.Y; jY < Item.Y + oldLength; jY++)
                        {
                            furnitureStack Stack = _Room.sqSTACK[jX, jY];
                            if (Stack != null && Stack.Count > 1)
                            {
                                if (itemID == Stack.bottomItemID())
                                {
                                    if(catalogueManager.getTemplate(((floorItem)_Items[Stack.topItemID()]).templateID).typeID == 2)
                                        _Room.sqSTATE[jX,jY] = squareState.Seat;
                                    else
                                        _Room.sqSTATE[jX,jY] = squareState.Open;
                                }
                                else if (itemID == Stack.topItemID())
                                {
                                    floorItem belowItem = (floorItem)_Items[Stack.getBelowItemID(itemID)];
                                    byte typeID = catalogueManager.getTemplate(belowItem.templateID).typeID;
                                    
                                    _Room.sqSTATE[jX, jY] = (squareState)typeID;
                                    if (typeID == 2 || typeID == 3)
                                    {
                                        _Room.sqITEMROT[jX, jY] = belowItem.Z;
                                        _Room.sqITEMHEIGHT[jX, jY] = belowItem.H + catalogueManager.getTemplate(belowItem.templateID).topH;
                                    }
                                    else if (typeID == 4)
                                        _Room.sqITEMHEIGHT[jX, jY] = belowItem.H;
                                }
                                Stack.Remove(itemID);
                                _Room.sqSTACK[jX, jY] = Stack;
                            }
                            else
                            {
                                _Room.sqSTATE[jX, jY] = 0;
                                _Room.sqITEMHEIGHT[jX, jY] = 0;
                                _Room.sqITEMROT[jX, jY] = 0;
                                _Room.sqSTACK[jX, jY] = null;
                            }
                            if(Template.typeID == 2 || Template.typeID == 3)
                                _Room.refreshCoord(jX, jY);
                        }
                    }

                    Item.X = X;
                    Item.Y = Y;
                    Item.Z = Z;
                    Item.H = H;
                    _Room.sendData("A_" + Item.ToString());
                    DB.runQuery("UPDATE furniture SET x = '" + X + "',y = '" + Y + "',z = '" + Z + "',h = '" + H.ToString().Replace(',','.') + "' WHERE id = '" + itemID + "' LIMIT 1");

                    for (int jX = X; jX < X + Width; jX++)
                    {
                        for (int jY = Y; jY < Y + Length; jY++)
                        {
                            furnitureStack Stack = null;
                            if (_Room.sqSTACK[jX, jY] == null)
                            {
                                if (Template.topH > 0 && Template.typeID != 2 && Template.typeID != 3 && Template.typeID != 4)
                                {
                                    Stack = new furnitureStack();
                                    Stack.Add(itemID);
                                }
                            }
                            else
                            {
                                Stack = _Room.sqSTACK[jX, jY];
                                Stack.Add(itemID);
                            }

                            _Room.sqSTATE[jX, jY] = (squareState)Template.typeID;
                            _Room.sqSTACK[jX, jY] = Stack;
                            if (Template.typeID == 2 || Template.typeID == 3)
                            {
                                _Room.sqITEMHEIGHT[jX, jY] = H + Template.topH;
                                _Room.sqITEMROT[jX, jY] = Z;
                                _Room.refreshCoord(jX, jY);
                            }
                            else if (Template.typeID == 4)
                                _Room.sqITEMHEIGHT[jX, jY] = H;
                        }
                    }
                }
                catch { }
            }
            /// <summary>
            /// Updates the status of a virtual flooritem and updates it in the virtual room and in the database. Door items are also being handled if opened/closed.
            /// </summary>
            /// <param name="itemID">The ID of the item to update.</param>
            /// <param name="toStatus">The new status of the item.</param>
            /// <param name="hasRights">The bool that indicates if the user that signs this item has rights.</param>
            internal void toggleItemStatus(int itemID, string toStatus, bool hasRights)
            {
                if (_Items.ContainsKey(itemID) == false)
                    return;

                floorItem Item = (floorItem)_Items[itemID];
                string itemSprite = Item.Sprite;
                if (itemSprite == "edice" || itemSprite == "edicehc" || stringManager.getStringPart(itemSprite,0,11) == "prizetrophy" || stringManager.getStringPart(itemSprite,0,7) == "present") // Items that can't be signed [this regards dicerigging etc]
                    return;

                if (hasRights && toStatus.ToLower() == "c" || toStatus.ToLower() == "o")
                {
                    #region Open/close doors
                    catalogueManager.itemTemplate Template = catalogueManager.getTemplate(Item.templateID);
                    if (Template.isDoor == false)
                        return;
                    int Length = 1;
                    int Width = 1;
                    if (Template.Length > 1 || Template.Width > 1)
                    {
                        if (Item.Z == 2 || Item.Z == 6)
                        {
                            Length = Template.Length;
                            Width = Template.Width;
                        }
                        else
                        {
                            Length = Template.Width;
                            Width = Template.Length;
                        }
                    }
                    if (toStatus.ToLower() == "c")
                    {
                        if (_Room.squareBlocked(Item.X, Item.Y, Length, Width))
                            return;
                        _Room.setSquareState(Item.X, Item.Y, Length, Width, squareState.Blocked);
                    }
                    else
                        _Room.setSquareState(Item.X, Item.Y, Length, Width, squareState.Open);



                #endregion
                }

                Item.Var = toStatus;
                _Room.sendData("AX" + itemID + Convert.ToChar(2) + toStatus + Convert.ToChar(2));
                DB.runQuery("UPDATE furniture SET var = '" + toStatus + "' WHERE id = '" + itemID + "' LIMIT 1");
            }
            /// <summary>
            /// Returns a string with all the virtual wallitems in this item manager.
            /// </summary>
            internal string Items
            {
                get
                {
                    StringBuilder itemList = new StringBuilder(Encoding.encodeVL64(_Items.Count));
                    foreach (floorItem Item in _Items.Values)
                        itemList.Append(Item.ToString());

                    return itemList.ToString();
                }
            }
            /// <summary>
            /// Returns a bool that indicates if the item manager contains a certain virtual flooritem.
            /// </summary>
            /// <param name="itemID">The ID of the item to check.</param>
            internal bool containsItem(int itemID)
            {
                return _Items.ContainsKey(itemID);
            }
            /// <summary>
            /// Returns the floorItem object of a certain virtual flooritem in the item manager.
            /// </summary>
            /// <param name="itemID">The ID of the item to get the floorItem object of.</param>
            internal floorItem getItem(int itemID)
            {
                return (floorItem)_Items[itemID];
            }
        }
        /// <summary>
        /// Provides management for virtual wallitems in a virtual room.
        /// </summary>
        internal class WallItemManager
        {
            private virtualRoom _Room;
            private Hashtable _Items = new Hashtable();
            /// <summary>
            /// Initializes the manager.
            /// </summary>
            /// <param name="Room">The parent room.</param>
            public WallItemManager(virtualRoom Room)
            {
                this._Room = Room;
            }
            /// <summary>
            /// Removes all the items from the item manager and destructs all objects inside.
            /// </summary>
            internal void Clear()
            {
                try {_Items.Clear();}
                catch {}
                _Room = null;
                _Items = null;
            }
            /// <summary>
            /// Adds a virtual wallitem to the item manager and optionally makes it appear in the room.
            /// </summary>
            /// <param name="itemID">The ID of the item to add.</param>
            /// <param name="Item">The item to add.</param>
            /// <param name="Place">Indicates if the item is put in the room now, so updating database and sending appear packet to room.</param>
            internal void addItem(int itemID, int templateID, string wallPosition, string Var, bool Place)
            {
                if (_Items.Contains(itemID) == false)
                {
                    wallItem Item = new wallItem(itemID,templateID,wallPosition,Var);
                    _Items.Add(itemID,Item);
                    if (Place)
                    {
                        _Room.sendData("AS" + Item.ToString());
                        DB.runQuery("UPDATE furniture SET roomid = '" + _Room.roomID + "',wallpos = '" + wallPosition + "' WHERE id = '" + itemID + "' LIMIT 1");  
                    }
                }
            }
            /// <summary>
            /// Removes a virtual wallitem from the item manager, updates the database row/drops the item from database and makes it disappear in the room.
            /// </summary>
            /// <param name="itemID">The ID of the item to remove.</param>
            /// <param name="ownerID">The ID of the user that owns this item. If 0, then the item will be dropped from the database.</param>
            internal void removeItem(int itemID, int ownerID)
            {
                if (_Items.ContainsKey(itemID))
                {
                    _Room.sendData("AT" + itemID);
                    _Items.Remove(itemID);
                    if (ownerID > 0)
                        DB.runQuery("UPDATE furniture SET ownerid = '" + ownerID + "',roomid = '0' WHERE id = '" + itemID + "' LIMIT 1");
                    else
                        DB.runQuery("DELETE FROM furniture WHERE id = '" + itemID + "' LIMIT 1");
                }
            }
            /// <summary>
            /// Updates the status of a virtual wallitem and updates it in the virtual room and in the database. Certain items can't switch status by this way, and they will be ignored to prevent exploiting.
            /// </summary>
            /// <param name="itemID">The ID of the item to update.</param>
            /// <param name="toStatus">The new status of the item.</param>
            internal void toggleItemStatus(int itemID, int toStatus)
            {
                if (_Items.ContainsKey(itemID) == false)
                    return;

                wallItem Item = (wallItem)_Items[itemID];
                string itemSprite = Item.Sprite;
                if (itemSprite == "roomdimmer" || itemSprite == "post.it" || itemSprite == "post.it.vd" || itemSprite == "poster" || itemSprite == "habbowheel")
                    return;

                Item.Var = toStatus.ToString();
                _Room.sendData("AU" + itemID + Convert.ToChar(9) + itemSprite + Convert.ToChar(9) + " " + Item.wallPosition + Convert.ToChar(9) + Item.Var);
                DB.runQuery("UPDATE furniture SET var = '" + toStatus + "' WHERE id = '" + itemID + "' LIMIT 1");
            }
            /// <summary>
            /// Returns a string with all the virtual wallitems in this item manager.
            /// </summary>
            internal string Items
            {
                get
                {
                    StringBuilder itemList = new StringBuilder();
                    foreach (wallItem Item in _Items.Values)
                        itemList.Append(Item.ToString() + Convert.ToChar(13));

                    return itemList.ToString();
                }
            }
            /// <summary>
            /// Returns a bool that indicates if the item manager contains a certain virtual wallitem.
            /// </summary>
            /// <param name="itemID">The ID of the item to check.</param>
            internal bool containsItem(int itemID)
            {
                return _Items.ContainsKey(itemID);
            }
            /// <summary>
            /// Returns the wallItem object of a certain virtual wallitem in the item manager.
            /// </summary>
            /// <param name="itemID">The ID of the item to get the wallItem object of.</param>
            internal wallItem getItem(int itemID)
            {
                return (wallItem)_Items[itemID];
            }
        }
#endregion

        #region Special publicroom additions
        /// <summary>
        /// Threaded. Handles special casts such as disco lamps etc in the virtual room.
        /// </summary>
        /// <param name="o">The room model name as a System.Object.</param>
        private void handleSpecialCasts(object o)
        {
            try
            {
                string Emitter = DB.runRead("SELECT specialcast_emitter FROM room_modeldata WHERE model = '" + (string)o + "'");
                int[] numData = DB.runReadRow("SELECT specialcast_interval,specialcast_rnd_min,specialcast_rnd_max FROM room_modeldata WHERE model = '" + (string)o + "'", null);
                int Interval = numData[0];
                int rndMin = numData[1];
                int rndMax = numData[2];
                numData = null;

                string prevCast = "";
                while (true)
                {
                    string Cast = "";
                    int RND = new Random().Next(rndMin, rndMax + 1);

                reCast:
                    if (Emitter == "cam1") // User camera system
                    {
                        switch (RND)
                        {
                            case 1:
                                int roomUID = getRandomRoomIdentifier();
                                if (roomUID != -1)
                                    Cast = "targetcamera " + roomUID;
                                break;
                            case 2:
                                Cast = "setcamera 1";
                                break;
                            case 3:
                                Cast = "setcamera 2";
                                break;
                        }
                    }
                    else if (Emitter == "sf") // Flashing dancetiles system
                        Cast = RND.ToString();
                    else if (Emitter == "lamp") // Discolights system
                        Cast = "setlamp " + RND;

                    if (Cast == "")
                        goto reCast;
                    if (Cast != prevCast) // Cast is not the same as previous cast
                    {
                        sendSpecialCast(Emitter, Cast);
                        prevCast = Cast;
                    }
                    Thread.Sleep(Interval);
                    Out.WriteTrace("Special cast loop");
                }
            }
            catch { }
        }
        #endregion

        #region Private classes
        /// <summary>
        /// Represents a stack of virtual flooritems.
        /// </summary>
        private class furnitureStack
        {
            private int[] _itemIDs;
            /// <summary>
            /// Initializes a new stack.
            /// </summary>
            internal furnitureStack()
            {
               _itemIDs = new int[20];
            }
            /// <summary>
            /// Adds an item ID to the top position of the stack.
            /// </summary>
            /// <param name="itemID">The item ID to add.</param>
            internal void Add(int itemID)
            {
                for (int i = 0; i < 20; i++)
                {
                    if (_itemIDs[i] == 0)
                    {
                        _itemIDs[i] = itemID;
                        return;
                    }
                }
            }
            /// <summary>
            /// Removes an item ID from the stack and shrinks empty spots. [order is kept the same]
            /// </summary>
            /// <param name="itemID">The item ID to remove.</param>
            internal void Remove(int itemID)
            {
                for (int i = 0; i < 20; i++)
                    if (_itemIDs[i] == itemID)
                    {
                        _itemIDs[i] = 0;
                        break;
                    }

                int g = 0;
                int[] j = new int[20];
                for (int i = 0; i < 20; i++)
                    if (_itemIDs[i] > 0)
                    {
                        j[g] = _itemIDs[i];
                        g++;
                    }
                _itemIDs = j;
            }
            /// <summary>
            /// The most top item ID of the stack.
            /// </summary>
            internal int topItemID()
            {
                for (int i = 0; i < 20; i++)
                    if (_itemIDs[i] == 0)
                        return _itemIDs[i - 1];
                return 0;
            }
            /// <summary>
            /// The lowest located [so: first added] item ID of the stack.
            /// </summary>
            internal int bottomItemID()
            {
                return _itemIDs[0];
            }
            /// <summary>
            /// Returns the item ID located above a given item ID.
            /// </summary>
            /// <param name="aboveID">The item ID to get the item ID above of.</param>
            internal int getAboveItemID(int aboveID)
            {
                for (int i = 0; i < 20; i++)
                    if (_itemIDs[i] == aboveID)
                        return _itemIDs[i + 1];
                return 0;
            }
            /// <summary>
            /// Returns the item ID located below a given item ID.
            /// </summary>
            /// <param name="belowID">The item ID to get the item ID below of.</param>
            internal int getBelowItemID(int belowID)
            {
                for (int i = 0; i < 20; i++)
                    if (_itemIDs[i] == belowID)
                        return _itemIDs[i - 1];
                return 0;
            }
            /// <summary>
            /// Returns a bool that indicates if the stack contains a certain item ID.
            /// </summary>
            /// <param name="itemID">The item ID to check.</param>
            internal bool Contains(int itemID)
            {
                foreach (int i in _itemIDs)
                    if (i == itemID)
                        return true;

                return false;
            }
            /// <summary>
            /// The amount of item ID's in the stack.
            /// </summary>
            internal int Count
            {
                get
                {
                    int j = 0;
                    for (int i = 0; i < 20; i++)
                    {
                        if (_itemIDs[i] > 0)
                            j++;
                        else
                            return j;
                    }
                    return j;
                }
            }
        }
        #endregion
    }
}
    