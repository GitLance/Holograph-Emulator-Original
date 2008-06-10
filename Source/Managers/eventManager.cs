using System;
using System.Text;
using System.Threading;
using System.Collections;

using Holo.Virtual.Users;
namespace Holo.Managers
{
    /// <summary>
    /// Provides management for events hosted by virtual users in their virtual rooms.
    /// </summary>
    public static class eventManager
    {
        #region Declares
        /// <summary>
        /// Array of hashtables that keeps the virtualEvent structs.
        /// </summary>
        private static Hashtable[] Events;
        /// <summary>
        /// The thread that removes the 'dead events' (virtual events where the hoster has left the virtual room where the event is hosted) from the manager every xx seconds. (configureable in system_config)
        /// </summary>
        private static Thread deadEventDropper;
        /// <summary>
        /// The amount of seconds between every 'check dead events' check. Initialized in seconds and multiplied with 1000, so value in milliseconds.
        /// </summary>
        private static int deadEventDropInterval;
        /// <summary>
        /// The amount of categories where virtual users can create virtual events in. Add and edit category titles in external_texts.
        /// </summary>
        internal static int categoryAmount;
        #endregion

        #region Manager voids
        /// <summary>
        /// Initializes or resets the virtual event manager, intializing the amount of categories and starting the 'dead event' collector.
        /// </summary>
        public static void Init()
        {
            try { deadEventDropper.Abort(); }
            catch { }

            categoryAmount = int.Parse(Config.getTableEntry("events_categorycount"));
            Events = new Hashtable[categoryAmount];
            for (int i = 0; i < categoryAmount; i++)
                Events[i] = new Hashtable();

            deadEventDropInterval = int.Parse(Config.getTableEntry("events_deadevents_removeinterval")) * 1000;
            deadEventDropper = new Thread(new ThreadStart(dropDeadEvents));
            deadEventDropper.Priority = ThreadPriority.BelowNormal;
            deadEventDropper.Start();
        }
        /// <summary>
        /// Ran on a thread with an interval of 2 minutes. Drops 'dead' virtual events (events where the hoster has left the virtual room where the event was hosted) from the manager.
        /// </summary>
        private static void dropDeadEvents()
        {
            while (true)
            {
                for (int i = 0; i < categoryAmount; i++)
                {
                    foreach (virtualEvent Event in ((Hashtable)Events[i].Clone()).Values)
                    {
                        if (userManager.containsUser(Event.userID) == false)
                            Events[i].Remove(Event.roomID);
                        else
                        {
                            virtualUser Hoster = userManager.getUser(Event.userID);
                            if (Hoster._roomID != Event.roomID)
                            {
                                Events[i].Remove(Event.roomID);
                                Hoster._hostsEvent = false;
                            }
                        }
                    }
                }
                Thread.Sleep(deadEventDropInterval); // Sleep the configured amount of time before repeating
                Out.WriteTrace("Drop dead event loop");
            }
        }
        #endregion

        #region Event management
        /// <summary>
        /// Creates a new virtual event with the details given in the correct category.
        /// </summary>
        /// <param name="categoryID">The ID of the category to host the event in.</param>
        /// <param name="userID">The database ID of the virtual user that hosts the event.</param>
        /// <param name="roomID">The database ID of the virtual room where the event is hosted.</param>
        /// <param name="Name">The name of the new virtual event.</param>
        /// <param name="Description">The description of the new virtual event.</param>
        public static void createEvent(int categoryID, int userID, int roomID, string Name, string Description)
        {
            if (Events[categoryID - 1].Contains(roomID) == false)
            {
                virtualEvent Event = new virtualEvent(roomID, userID, Name, Description);
                Events[categoryID - 1].Add(roomID, Event);
            }
        }
        /// <summary>
        /// Scrolls through all categories to remove a virtual event from the event manager, if the event is removed then the void is exited.
        /// </summary>
        /// <param name="roomID">The database ID of the virtual room where the event was hosted.</param>
        public static void removeEvent(int roomID)
        {
            for (int i = 0; i < categoryAmount; i++)
            {
                if(Events[i].ContainsKey(roomID))
                {
                    Events[i].Remove(roomID);
                    return;
                }
            }
        }
        /// <summary>
        /// Edits a virtual event and re-inserts it in the category.
        /// </summary>
        /// <param name="categoryID">The ID of the category where the event is hosted.</param>
        /// <param name="roomID">The database ID of the virtual room where the event was hosted.</param>
        /// <param name="Name">The (new) name of the virtual event.</param>
        /// <param name="Description">The (new) description of the virtual event.</param>
        public static void editEvent(int categoryID, int roomID, string Name, string Description)
        {
            if (Events[categoryID - 1].Contains(roomID))
            {
                virtualEvent Event = (virtualEvent)Events[categoryID - 1][roomID];
                Event.Name = Name;
                Event.Description = Description;
                Events[categoryID - 1].Remove(roomID);
                Events[categoryID - 1].Add(roomID,Event); // Swap (because the object is a struct)
            }
        }
        /// <summary>
        /// Returns a boolean that indicates if there exists a category with the given ID.
        /// </summary>
        /// <param name="categoryID">The ID of the category to check.</param>
        public static bool categoryOK(int categoryID)
        {
            return (categoryID > 0 && categoryID <= categoryAmount);
        }
        /// <summary>
        /// Returns a string of all the virtual events in a certain category, where the hosting user is inside his/her virtual room. Other events will be ignored.
        /// </summary>
        /// <param name="categoryID">The category ID to get the events of.</param>
        public static string getEvents(int categoryID)
        {
            try
            {
                int Count = 0;
                StringBuilder List = new StringBuilder();
                
                foreach (virtualEvent Event in Events[categoryID - 1].Values)
                {
                    if (userManager.containsUser(Event.userID)) // Hoster is online
                    {
                        List.Append(Event.roomID + Convert.ToChar(2).ToString() + userManager.getUser(Event.userID)._Username + Convert.ToChar(2) + Event.Name + Convert.ToChar(2) + Event.Description + Convert.ToChar(2) + Event.Started + Convert.ToChar(2));
                        Count++;
                    }
                }
                return Encoding.encodeVL64(Count) + List.ToString();
            }
            catch { return "H"; }
        }
        /// <summary>
        /// Returns the information string about a single virtual event. If the event isn't found in a category, or the hoster isn't online, then -1 is returned.
        /// </summary>
        /// <param name="roomID">The database ID of the virtual room.</param>
        public static string getEvent(int roomID)
        {
            try
            {
                for (int i = 0; i < categoryAmount; i++)
                {
                    if (Events[i].ContainsKey(roomID))
                    {
                        virtualEvent Event = (virtualEvent)Events[i][roomID];
                        return Event.userID + Convert.ToChar(2).ToString() + userManager.getUser(Event.userID)._Username + Convert.ToChar(2).ToString() + Event.roomID + Convert.ToChar(2).ToString() + Encoding.encodeVL64(i + 1) + Event.Name + Convert.ToChar(2).ToString() + Event.Description + Convert.ToChar(2).ToString() + Event.Started + Convert.ToChar(2).ToString();
                    }
                }
                return "-1";
            }
            catch { return "-1"; }
        }
        #endregion

        #region Private objects
        /// <summary>
        /// Represents a virtual event hosted by a virtual user in his/her virtual room.
        /// </summary>
        private struct virtualEvent
        {
            /// <summary>
            /// The database ID of the virtual room where this event takes place.
            /// </summary>
            internal int roomID;
            /// <summary>
            /// The database ID of the virtual user that hosts this user in his/her virtual room.
            /// </summary>
            internal int userID;
            /// <summary>
            /// The name of this virtual event.
            /// </summary>
            internal string Name;
            /// <summary>
            /// The description of this virtual event.
            /// </summary>
            internal string Description;
            /// <summary>
            /// The time this virtual event started. (today)
            /// </summary>
            internal string Started;

            /// <summary>
            /// Intializes a new virtual event.
            /// </summary>
            /// <param name="roomID">The database ID of the virtual room where this event is hosted.</param>
            /// <param name="Hoster">The database ID of the virtual user that hosts this event in his/her virtual room.</param>
            /// <param name="Name">The name of this event.</param>
            /// <param name="Description">The description of this event.</param>
            internal virtualEvent(int roomID, int userID, string Name, string Description)
            {
                this.roomID = roomID;
                this.userID = userID;
                this.Name = Name;
                this.Description = Description;
                this.Started = DateTime.Now.ToShortTimeString();
            }
        }
        #endregion
    }
}
