using System;
using System.Threading;
using System.Collections;

using Holo.Managers;
using Holo.Virtual.Rooms;

namespace Holo.Virtual.Users
{
    /// <summary>
    /// Provides management for the statuses of a virtual user.
    /// </summary>
    public class virtualRoomUserStatusManager
    {
        #region Declares
        /// <summary>
        /// The ID of the user that uses this status manager.
        /// </summary>
        private int userID;
        /// <summary>
        /// The ID of the room the user that uses this status manager is in.
        /// </summary>
        private int roomID;
        /// <summary>
        /// Contains the status strings.
        /// </summary>
        private Hashtable _Statuses;
        /// <summary>
        /// The thread that handles the carrying, drinking and finally vanishing the item in the virtual room.
        /// </summary>
        private Thread itemCarrier;
        private delegate void statusVoid(string Key, string Value, int Length);
        #endregion

        #region Constructors/destructors
        public virtualRoomUserStatusManager(int userID, int roomID)
        {
            this.userID = userID;
            this.roomID = roomID;
            _Statuses = new Hashtable();
        }
        /// <summary>
        /// Empties the status manager and destructs all inside objects.
        /// </summary>
        internal void Clear()
        {
            try
            {
                itemCarrier.Abort();
                _Statuses.Clear();
                _Statuses = null;
            }
            catch { }
        }
        #endregion

        #region Partner objects
        /// <summary>
        /// The parent virtualUser object of this status manager.
        /// </summary>
        private virtualUser User
        {
            get
            {
                return userManager.getUser(userID);
            }
        }
        /// <summary>
        /// The virtualRoom object where the parent virtual user of this status manager is in.
        /// </summary>
        private virtualRoom Room
        {
            get
            {
                return roomManager.getRoom(roomID);
            }
        }
        /// <summary>
        /// The virtualRoomUser object of the parent virtual user of this status manager.
        /// </summary>
        private virtualRoomUser roomUser
        {
            get
            {
                return userManager.getUser(userID).roomUser;
            }
        }
        #endregion

        #region Status management
        /// <summary>
        /// Adds a status key and a value to the status manager. If the status is already inside, then the previous one will be removed.
        /// </summary>
        /// <param name="Key">The key of the status.</param>
        /// <param name="Value">The value of the status.</param>
        internal void addStatus(string Key, string Value)
        {
            if (_Statuses.ContainsKey(Key))
                _Statuses.Remove(Key);
            _Statuses.Add(Key, Value);
        }
        /// <summary>
        /// Removes a certain status from the status manager.
        /// </summary>
        /// <param name="Key">The key of the status to remove.</param>
        internal void removeStatus(string Key)
        {
            try
            {
                if (_Statuses.ContainsKey(Key))
                    _Statuses.Remove(Key);
            }
            catch { }
        }
        /// <summary>
        /// Returns a bool that indicates if a certain status is in the status manager.
        /// </summary>
        /// <param name="Key">The key of the status to check.</param>
        internal bool containsStatus(string Key)
        {
            return _Statuses.ContainsKey(Key);
        }
        /// <summary>
        /// Refreshes the status of the parent virtual user in the virtual room.
        /// </summary>
        internal void Refresh()
        {
            roomUser.Refresh();
        }
        /// <summary>
        /// Returns the status string of all the statuses currently in the status manager.
        /// </summary>
        public override string ToString()
        {
            string Output = "";
            foreach(string Key in _Statuses.Keys)
            {
                Output += Key;
                string Value = (string)_Statuses[Key];
                if(Value != "")
                    Output += " " + Value;
                Output += "/";
            }

            return Output;
        }
        #endregion

        #region Statuses
        /// <summary>
        /// Makes the user carry a drink/item in the virtual room. Starts a thread that uses config-defined values. The thread will handle the animations of the sips etc, and finally the drop.
        /// </summary>
        /// <param name="Item">The item to carry.</param>
        internal void carryItem(string Item)
        {
            dropCarrydItem();
            _Statuses.Remove("dance");
            itemCarrier = new Thread(new ParameterizedThreadStart(itemCarrierLoop));
            itemCarrier.Priority = ThreadPriority.Lowest;
            itemCarrier.Start(Item);
        }
        /// <summary>
        /// Immediately stops carrying an item.
        /// </summary>
        internal void dropCarrydItem()
        {
            if (itemCarrier != null && itemCarrier.IsAlive)
                itemCarrier.Abort();
            removeStatus("carryd");
            removeStatus("drink");
        }
        
        #endregion

        #region Status handlers
        /// <summary>
        /// Adds a status, keeps it for a specified amount of time [in ms] and removes the status again. Refreshes at add and remove.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Length"></param>
        internal void handleStatus(string Key, string Value, int Length)
        {
            if (_Statuses.ContainsKey(Key))
                _Statuses.Remove(Key);
            new statusVoid(HANDLESTATUS).BeginInvoke(Key, Value, Length, null, null);
        }
        private void HANDLESTATUS(string Key, string Value, int Length)
        {
            try
            {
                _Statuses.Add(Key, Value);
                roomUser.Refresh();
                Thread.Sleep(Length);
                _Statuses.Remove(Key);
                roomUser.Refresh();
            }
            catch { }
        }
        /// <summary>
        /// Ran on thread with lowest priority. Handles the carrying and drinking of an item in the virtual room.
        /// </summary>
        /// <param name="s">The object that will be converted in a string to serve as the item being carryd.</param>
        private void itemCarrierLoop(object s)
        {
            string carrydItem = (string)s;
            for(int i = 1; i <= Config.Statuses_itemCarrying_SipAmount; i++)
            {
                addStatus("carryd",carrydItem);
                roomUser.Refresh();
                Thread.Sleep(Config.Statuses_itemCarrying_SipInterval);

                _Statuses.Remove("carryd");

                addStatus("drink",carrydItem);
                roomUser.Refresh();
                Thread.Sleep(Config.Statuses_itemCarrying_SipDuration);

                _Statuses.Remove("drink");
            }
            roomUser.Refresh();
        }
        #endregion
    }
}
