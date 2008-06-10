using System;
using System.Collections;

using Holo.Virtual.Rooms;

namespace Holo.Managers
{
    /// <summary>
    /// Provides management for virtual rooms, aswell as some misc tasks for rooms.
    /// </summary>
    public static class roomManager
    {
        #region Declares
        /// <summary>
        /// Contains the hooked virtual room objects.
        /// </summary>
        private static Hashtable _Rooms = new Hashtable();
        /// <summary>
        /// The peak amount of rooms that has been in the room manager since start of the emulator.
        /// </summary>
        private static int _peakRoomCount;
        #endregion

        #region Virtual room management
        /// <summary>
        /// Adds a virtualRoom class together with the roomID to the roomManager.
        /// </summary>
        /// <param name="roomID">The ID of the room to add..</param>
        /// <param name="Room">The virtualRoom class of this room.</param>
        public static void addRoom(int roomID, virtualRoom Room)
        {
            if (_Rooms.ContainsKey(roomID) == false)
            {
                _Rooms.Add(roomID, Room);
                Out.WriteLine("Room [" + roomID + ", publicroom: " + Room.isPublicroom.ToString().ToLower() + "] loaded.", Out.logFlags.StandardAction);
                if (_Rooms.Count > _peakRoomCount)
                    _peakRoomCount = _Rooms.Count;
            }

        }
        /// <summary>
        /// Removes a room from the roomManager. [if it exists]
        /// </summary>
        /// <param name="roomID">The ID of the room to remove.</param>
        public static void removeRoom(int roomID)
        {
            if(_Rooms.ContainsKey(roomID))
            {
                bool boolPublicroom = ((virtualRoom)_Rooms[roomID]).isPublicroom;
                _Rooms.Remove(roomID);
                updateRoomVisitorCount(roomID, 0);
                Out.WriteLine("Room [" + roomID + ", publicroom: " + boolPublicroom.ToString().ToLower() + "] destroyed.", Out.logFlags.StandardAction);
                GC.Collect();
            }
        }
        /// <summary>
        /// Returns a bool that indicates if the roomManager contains a certain room.
        /// </summary>
        /// <param name="roomID">The ID of the room.</param>
        public static bool containsRoom(int roomID)
        {
            return _Rooms.ContainsKey(roomID);
        }

        /// <summary>
        /// Returns the current amount of rooms in the roomManager.
        /// </summary>
        public static int roomCount
        {
            get
            {
                return _Rooms.Count;
            }
        }
        /// <summary>
        /// Returns the peak amount of rooms in the roomManager since boot.
        /// </summary>
        public static int peakRoomCount
        {
            get
            {
                return _peakRoomCount;
            }
        }

        /// <summary>
        /// Returns a virtualRoom class for a certain room.
        /// </summary>
        /// <param name="roomID">The ID of the room.</param>
        public static virtualRoom getRoom(int roomID)
        {
            return (virtualRoom)_Rooms[roomID];
        }
        #endregion

        #region Misc room related functions
        /// <summary>
        /// Updates the inside visitors count in the database for a certain room.
        /// </summary>
        /// <param name="roomID">The ID of the room to update.</param>
        /// <param name="visitorCount">The new visitors count.</param>
        public static void updateRoomVisitorCount(int roomID, int visitorCount)
        {
            DB.runQuery("UPDATE rooms SET visitors_now = '" + visitorCount + "' WHERE id = '" + roomID + "' LIMIT 1");
        }
        /// <summary>
        /// Returns the int ID for a certain room state.
        /// </summary>
        /// <param name="State">The room state ID.</param>
        public static int getRoomState(string State)
        {
            if (State == "closed")
                return 1;
            else if (State == "password")
                return 2;
            else
                return 0;
        }
        /// <summary>
        /// Returns the string state for a certain room state byte.
        /// </summary>
        /// <param name="State">The room state ID.</param>
        public static string getRoomState(int State)
        {
            if (State == 1)
                return "closed";
            else if (State == 2)
                return "password";
            else
                return "open";
        }
        #endregion
    }
}
