using System;
using System.Text;
using System.Threading;
using System.Collections;

using Holo.Virtual.Users;
using Holo.Virtual.Rooms.Pathfinding;
namespace Holo.Virtual.Rooms.Bots
{
    /// <summary>
    /// Represents a computer controlled virtual user with an artifical intelligence (AI). The bot walks around in virtual rooms on specified coordinates, interacts with other virtual users and serves drinks and food.
    /// </summary>
    internal class virtualBot
    {
        #region Declares
        /// <summary>
        /// The ID of the bot in the virtual room.
        /// </summary>
        internal int roomUID;
        /// <summary>
        /// The virtual room the bot roams in.
        /// </summary>
        internal virtualRoom Room;
        /// <summary>
        /// The name of the bot.
        /// </summary>
        internal string Name;
        /// <summary>
        /// The mission/motto that the bot has.
        /// </summary>
        internal string Mission;
        /// <summary>
        /// The bot's figure string.
        /// </summary>
        internal string Figure;

        /// <summary>
        /// The X position of the bot in the virtual room.
        /// </summary>
        internal int X;
        /// <summary>
        /// The Y position of the bot in the virtual room.
        /// </summary>
        internal int Y;
        /// <summary>
        /// The height of the bot in the virtual room.
        /// </summary>
        internal double H;
        /// <summary>
        /// The rotation of the bot's head in the virtual room.
        /// </summary>
        internal byte Z1;
        /// <summary>
        /// The rotation of the bot's body in the virtual room.
        /// </summary>
        internal byte Z2;
        /// <summary>
        /// Used for pathfinding. The X coordinate of the bot's target square in the virtual room.
        /// </summary>
        internal int goalX;
        /// <summary>
        /// Used for pathfinding. The Y coordinate of the bot's target square in the virtual room.
        /// </summary>
        internal int goalY;
        /// <summary>
        /// Indicates if the bot uses 'freeroam', which allows it to walk everywhere where it can go to. Astar pathfinding is used.
        /// </summary>
        internal bool freeRoam;
        /// <summary>
        /// The message that the bot will use (on random) to shouting people near the bot.
        /// </summary>
        internal string noShoutingMessage;

        private delegate void statusVoid(string Key, string Value, int Length);
        /// <summary>
        /// Handles the random determining of actions.
        /// </summary>
        private Thread aiHandler;
        /// <summary>
        /// Contains the texts that the bot can 'say' on random.
        /// </summary>
        private string[] Sayings;
        /// <summary>
        /// Contains the texts that the bot can 'shout' on random.
        /// </summary>
        private string[] Shouts;
        /// <summary>
        /// Contains the coordinate's where the bot can walk to. Ignored if freeroam is enabled.
        /// </summary>
        private Coord[] Coords;
        /// <summary>
        /// Contains the chat triggers where the bot reacts on.
        /// </summary>
        private chatTrigger[] chatTriggers;
        /// <summary>
        /// The virtualRoomUser object the bot is currently serving an item to.
        /// </summary>
        private virtualRoomUser Customer;
        /// <summary>
        /// The chatTrigger object that was invoked by the current customer.
        /// </summary>
        private chatTrigger customerTrigger;
        #endregion

        #region Constructors/destructors
        /// <summary>
        /// Contains the bot's animation statuses.
        /// </summary>
        private Hashtable Statuses;
        /// <summary>
        /// Initializes a new virtualBot object, loading the walk squares, chat texts etc.
        /// </summary>
        /// <param name="botID">The database ID of the bot.</param>
        /// <param name="roomUID">The ID that identifies this bot in room.</param>
        /// <param name="Room">The virtualRoom object where the bot is in.</param>
        internal virtualBot(int botID, int roomUID, virtualRoom Room)
        {
            this.roomUID = roomUID;
            this.Room = Room;

            string[] botDetails = DB.runReadRow("SELECT name,mission,figure,x,y,z,freeroam,message_noshouting FROM roombots WHERE id = '" + botID + "'");
            this.Name = botDetails[0];
            this.Mission = botDetails[1];
            this.Figure = botDetails[2];
            this.X = int.Parse(botDetails[3]);
            this.Y = int.Parse(botDetails[4]);
            this.Z1 = byte.Parse(botDetails[5]);
            this.Z2 = Z1;
            this.goalX = -1;
            this.freeRoam = (botDetails[6] == "1");
            this.noShoutingMessage = botDetails[7];
            this.Sayings = DB.runReadColumn("SELECT text FROM roombots_texts WHERE id = '" + botID + "' AND type = 'say'", 0);
            this.Shouts = DB.runReadColumn("SELECT text FROM roombots_texts WHERE id = '" + botID + "' AND type = 'shout'", 0);
            
            string[] triggerWords = DB.runReadColumn("SELECT words FROM roombots_texts_triggers WHERE id = '" + botID + "' ORDER BY id ASC", 0);
            if (triggerWords.Length > 0)
            {   
                string[] triggerReplies = DB.runReadColumn("SELECT replies FROM roombots_texts_triggers WHERE id = '" + botID + "' ORDER BY id ASC", 0);
                string[] triggerServeReplies = DB.runReadColumn("SELECT serve_replies FROM roombots_texts_triggers WHERE id = '" + botID + "' ORDER BY id ASC", 0);
                string[] triggerServeItems = DB.runReadColumn("SELECT serve_item FROM roombots_texts_triggers WHERE id = '" + botID + "' ORDER BY id ASC", 0);

                this.chatTriggers = new chatTrigger[triggerWords.Length];
                for (int i = 0; i < triggerWords.Length; i++)
                    this.chatTriggers[i] = new chatTrigger(triggerWords[i].Split('}'), triggerReplies[i].Split('}'), triggerServeReplies[i].Split('}'), triggerServeItems[i]);
            }

            int[] Xs = DB.runReadColumn("SELECT x FROM roombots_coords WHERE id = '" + botID + "' ORDER BY id ASC", 0, null);
            Coords = new Coord[Xs.Length + 1];
            Coords[Xs.Length] = new Coord(this.X, this.Y);

            if (Xs.Length > 1) // More coords assigned than just the start square
            {
                int[] Ys = DB.runReadColumn("SELECT y FROM roombots_coords WHERE id = '" + botID + "' ORDER BY id ASC", 0, null);
                for (int i = 0; i < Xs.Length; i++)
                    Coords[i] = new Coord(Xs[i], Ys[i]);
            }

            Statuses = new Hashtable();
            aiHandler = new Thread(new ThreadStart(AI));
            aiHandler.Priority = ThreadPriority.BelowNormal;
            aiHandler.Start();
        }
        /// <summary>
        /// Safely shuts this virtualBot down and tidies up all resources.
        /// </summary>
        internal void Kill()
        {
            try { aiHandler.Abort(); }
            catch { }

            aiHandler = null;
            Room = null;
            Statuses = null;
            Coords = null;
            Sayings = null;
            Shouts = null;
            chatTriggers = null;
            Customer = null;
            customerTrigger = null;
        }
        #endregion

        #region Bot properties
        /// <summary>
        /// The details string of the bot, containing room identifier ID, name, motto, figure etc.
        /// </summary>
        internal string detailsString
        {
            get
            {
                string s = "i:" + roomUID + Convert.ToChar(13) + "a:-1" + Convert.ToChar(13) + "n:" + Name + Convert.ToChar(13) + "f:" + Figure + Convert.ToChar(13) + "l:" + X + " " + Y + " " + H + Convert.ToChar(13);
                if (Mission != "") { s += "c:" + Mission + Convert.ToChar(13); }
                return s + "[bot]" + Convert.ToChar(13);
            }
        }
        /// <summary>
        /// The status string of the bot, containing positions, movements, statuses (animations) etc.
        /// </summary>
        internal string statusString
        {
            get
            {
                string s = roomUID + " " + X + "," + Y + "," + H.ToString().Replace(",", ".") + "," + Z1 + "," + Z2 + "/";
                foreach (string Key in Statuses.Keys)
                {
                    s += Key;
                    string Value = (string)Statuses[Key];
                    if (Value != "")
                        s += " " + Value;
                    s += "/";
                }
                return s;
            }
        }
        #endregion

        #region Actions
        /// <summary>
        /// Invoked by a virtualRoomUser. There is checked if this bot reacts on a certain chat message, if so, then replies/orders etc are processed.
        /// </summary>
        /// <param name="roomUser">The virtualRoomUser object that interacts with this bot by saying a message.</param>
        /// <param name="Message">The message that the virtualRoomUser said to this bot.</param>
        internal void Interact(virtualRoomUser roomUser, string Message)
        {
            Message = Message.ToLower();
            string[] messageWords = Message.Split(' ');
            if (chatTriggers != null)
            {
                foreach (chatTrigger Trigger in chatTriggers)
                {
                    for (int i = 0; i < messageWords.Length; i++)
                    {
                        if (Trigger.containsWord(messageWords[i]))
                        {
                            if (Trigger.serveItem != "") // Serve an item, walk up to the customer and hand over the beverage
                            {
                                if (Customer != null) // The bot is currently serving a different room user, ignore this trigger
                                    return;

                                Coord Closest = getClosestWalkCoordTo(roomUser.X, roomUser.Y);
                                if (Closest.X == -1) // Can't serve this user (no square close enough)
                                    return;

                                Room.sendSaying(this, Trigger.Reply);
                                removeStatus("dance");
                                addStatus("carryd", Trigger.serveItem); // Make the bot starting to carry the order to deliver it 
                                goalX = Closest.X;
                                goalY = Closest.Y;
                                this.Customer = roomUser;
                                this.customerTrigger = Trigger;
                            }
                            else
                            {
                                this.Z1 = Rotation.Calculate(X,Y,roomUser.X,roomUser.Y);
                                this.Z2 = this.Z1;
                                Room.sendSaying(this, Trigger.Reply);
                            }
                            return; // One trigger per time
                        }
                    }
                }
            }
        }
        /// <summary>
        /// If the bot currently is processing an order, then it'll hand over the order and prepare for a new one.
        /// </summary>
        internal void checkOrders()
        {
            if (Customer != null)
            {
                {
                    goalX = -1;
                    Rotate(Customer.X, Customer.Y);
                    removeStatus("carryd");
                    Room.sendSaying(this, customerTrigger.serveReply);

                    if (Customer.statusManager.containsStatus("sit") == false)
                    {
                        Customer.Z1 = Rotation.Calculate(Customer.X, Customer.Y, X, Y);
                        Customer.Z2 = Customer.Z1;
                    }
                    Customer.statusManager.carryItem(customerTrigger.serveItem);
                }
                
                {
                    Customer = null;
                    customerTrigger = null;
                }
            }
        }
        /// <summary>
        /// Rotates the bot to a certain X and Y coordinate and refreshes it in the room. If the bot is sitting, then rotating will be ignored.
        /// </summary>
        /// <param name="toX">The X coordinate to face.</param>
        /// <param name="toY">The Y coordinate to face.</param>
        internal void Rotate(int toX, int toY)
        {
            Rotate(Rotation.Calculate(X, Y, toX, toY));
        }
        /// <summary>
        /// Sets a new rotation for the bot and refreshes it in the room. If the bot is sitting, then rotating will be ignored.
        /// </summary>
        /// <param name="R">The new rotation to use.</param>
        internal void Rotate(byte R)
        {
            if (R != Z1 && Statuses.ContainsKey("sit") == false)
            {
                Z1 = R;
                Z2 = R;
                Refresh();
            }
        }
        /// <summary>
        /// Returns a Coord object with the X and Y of the walkcoord that is as closest to the given position.
        /// </summary>
        /// <param name="X">The X position.</param>
        /// <param name="Y">The Y position.</param>
        internal Coord getClosestWalkCoordTo(int X, int Y)
        {
            int minDistance = 6;
            Coord Closest = new Coord(-1, 0);

           foreach (Coord Coord in Coords)
            {
                int curDistance = Math.Abs(X - Coord.X) + Math.Abs(Y - Coord.Y);
                if (curDistance < minDistance)
                {
                    minDistance = curDistance;
                    Closest = Coord;
                }
            }
            
            return Closest;
        }
        #endregion

        #region Status management
        /// <summary>
        /// Adds a status key and a value to the bot's statuses. If the status is already inside, then the previous one will be removed.
        /// </summary>
        /// <param name="Key">The key of the status.</param>
        /// <param name="Value">The value of the status.</param>
        internal void addStatus(string Key, string Value)
        {
            if (Statuses.ContainsKey(Key))
                Statuses.Remove(Key);
            Statuses.Add(Key, Value);
        }
        /// <summary>
        /// Removes a certain status from the bot's statuses.
        /// </summary>
        /// <param name="Key">The key of the status to remove.</param>
        internal void removeStatus(string Key)
        {
            try
            {
                if (Statuses.ContainsKey(Key))
                    Statuses.Remove(Key);
            }
            catch { }
        }
        /// <summary>
        /// Returns a bool that indicates if the bot has a certain status at the moment.
        /// </summary>
        /// <param name="Key">The key of the status to check.</param>
        internal bool containsStatus(string Key)
        {
            return Statuses.ContainsKey(Key);
        }
        /// <summary>
        /// Refreshes the status of the bot in the virtual room.
        /// </summary>
        internal void Refresh()
        {
            Room.Refresh(this);
        }
        /// <summary>
        /// Adds a status to the bot, keeps it for a specified amount of time [in ms] and removes the status. Refreshes at add and remove.
        /// </summary>
        /// <param name="Key">The key of the status, eg, 'sit'.</param>
        /// <param name="Value">The value of the status, eg, '1.0'.</param>
        /// <param name="Length">The amount of milliseconsd to keep the status before removing it again.</param>
        internal void handleStatus(string Key, string Value, int Length)
        {
            if (Statuses.ContainsKey(Key))
                Statuses.Remove(Key);
            new statusVoid(HANDLESTATUS).BeginInvoke(Key, Value, Length, null, null);   
        }
        private void HANDLESTATUS(string Key, string Value, int Length)
        {
            try
            {
                Statuses.Add(Key, Value);
                Refresh();
                Thread.Sleep(Length);
                Statuses.Remove(Key);
                Refresh();
            }
            catch { }
        }
        #endregion

        #region Misc
        /// <summary>
        /// Ran on a thread. Handles the bot's artifical intelligence, by interacting with users and using random values etc.
        /// </summary>
        private void AI()
        {
            int lastMessageID = -1;
            Random RND = new Random(roomUID * DateTime.Now.Millisecond);
            //try
            {
                while (true)
                {
                    if (Customer != null) // Currently serving a different user
                        continue;

                    int ACTION = RND.Next(0, 15);
                    switch (ACTION)
                    {
                        case 1: // Move
                            {
                                Coord Next = new Coord();
                                if (freeRoam)
                                {
                                    int[] Borders = Room.getMapBorders();
                                    Next = new Coord(RND.Next(0, Borders[0]), RND.Next(0, Borders[1]));
                                }
                                else
                                    Next = Coords[RND.Next(0, Coords.Length)];
                                
                                if (Next.X == X && Next.Y == Y) // Coord didn't changed
                                {
                                    Z1 = (byte)RND.Next(0, 10);
                                    Z2 = Z1;
                                    Refresh();
                                }
                                else
                                {
                                    goalX = Next.X;
                                    goalY = Next.Y;
                                }
                                break;
                            }

                        case 2: // Rotate
                            {
                                byte R = (byte)RND.Next(0,10);
                                while (R == Z2)
                                {
                                    R = (byte)RND.Next(0, 10);
                                    Out.WriteTrace("Randomizing rotation of bot");
                                }
                                Rotate(R);
                                break;
                            }

                        case 3: // Shout
                            {
                                if (Shouts.Length > 0)
                                {
                                    int messageID = RND.Next(0, Shouts.Length);
                                    if (Shouts.Length > 1) // More than one shout assigned
                                    {
                                        while (messageID == lastMessageID) // Avoid shouting the same sentence two times in a row
                                        {
                                            messageID = RND.Next(0, Shouts.Length);
                                            Out.WriteTrace("Randomizing shout ID of bot");
                                        }
                                        lastMessageID = messageID;
                                    }
                                    Room.sendShout(this,Shouts[messageID]);
                                }
                                break;
                            }

                        case 4: // Say
                            {
                                if (Sayings.Length > 0)
                                {
                                    int messageID = RND.Next(0, Sayings.Length);
                                    if (Sayings.Length > 1) // More than one saying assigned
                                    {
                                        while (messageID == lastMessageID) // Avoid saying the same sentence two times in a row
                                        {
                                            messageID = RND.Next(0, Sayings.Length);
                                            Out.WriteTrace("Randomizing say ID of bot");
                                        }
                                        lastMessageID = messageID;
                                    }
                                    Room.sendSaying(this, Sayings[messageID]);
                                }
                                break;
                            }

                        case 5: // Status
                            {
                                if (RND.Next(0, 2) == 0)
                                {
                                    Statuses.Remove("dance");
                                    handleStatus("wave", "", Config.Statuses_Wave_waveDuration);
                                }
                                else
                                {
                                    addStatus("dance", "3");
                                    Refresh();
                                }
                                break;
                            }

                        case 6: // Item time
                            {
                                //Items.floorItem Seat = Room.floorItemManager.getItem(1810);
                                //goalX = Seat.X;
                                //goalY = Seat.Y;
                                break;
                            }
                    }
                    Thread.Sleep(3000);
                    Out.WriteTrace("Bot AI loop");
                }
            }
            //catch { aiHandler.Abort(); }
        }
        #endregion

        #region Private objects
        /// <summary>
        /// Represents a trigger that can be invoked by a chat message. Results in a reply and/or an order confirmation.
        /// </summary>
        private class chatTrigger
        {
            /// <summary>
            /// A System.String array with words that invoke this trigger.
            /// </summary>
            private string[] Words;
            /// <summary>
            /// A System.String array with replies that are used when this trigger is invoked.
            /// </summary>
            private string[] Replies;
            /// <summary>
            /// A System.String array with replies that are used when the bot hands over the food/drink item for this trigger.
            /// </summary>
            private string[] serveReplies;
            /// <summary>
            /// The item (food/drink) that will be served when one of this trigger's words match a given word.
            /// </summary>
            internal string serveItem;
            internal chatTrigger(string[] Words, string[] Replies, string[] serveReplies, string serveItem)
            {
                this.Words = Words;
                this.Replies = Replies;
                this.serveReplies = serveReplies;
                this.serveItem = serveItem;
            }
            /// <summary>
            /// Returns a boolean that indicates if this trigger replies on a certain word.
            /// </summary>
            /// <param name="Word">The word to check.</param>
            internal bool containsWord(string Word)
            {
                if (Word.Substring(Word.Length - 1, 1) == "?")
                    Word = Word.Substring(0, Word.Length - 1);

                for (int i = 0; i < Words.Length; i++)
                    if (Words[i] == Word)
                        return true;
                return false;
            }
            /// <summary>
            /// Returns a random reply from the replies array.
            /// </summary>
            internal string Reply
            {
                get
                {
                    return Replies[new Random(DateTime.Now.Millisecond).Next(0, Replies.Length)];
                }
            }
            /// <summary>
            /// Returns a random 'hand over item, here you are' reply from the replies array.
            /// </summary>
            internal string serveReply
            {
                get
                {
                    return serveReplies[new Random(DateTime.Now.Millisecond).Next(0, serveReplies.Length)];
                }
            }
        }
        #endregion
    }
}
