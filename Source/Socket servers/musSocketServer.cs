using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using Holo.Managers;
using Holo.Virtual.Users;
using Holo.Virtual.Rooms;

namespace Holo.Socketservers
{
    /// <summary>
    /// Asynchronous socket server for the MUS connections.
    /// </summary>
    public static class musSocketServer
    {
        private static Socket socketHandler;
        private static int _Port;
        private static string _musHost;
        /// <summary> 
        /// Initializes the socket listener for MUS connections and starts listening. 
        /// </summary> 
        /// <param name="bindPort">The port where the socket listener should be bound to.</param> 
        /// <remarks></remarks> 
        internal static bool Init(int bindPort, string musHost)
        {
            _Port = bindPort;
            _musHost = musHost;
            socketHandler = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Out.WriteLine("Starting up asynchronous socket server for MUS connections for port " + bindPort + "...");
            try
            {
                socketHandler.Bind(new IPEndPoint(IPAddress.Any, bindPort));
                socketHandler.Listen(25);
                socketHandler.BeginAccept(new AsyncCallback(connectionRequest), socketHandler);

                Out.WriteLine("Asynchronous socket server for MUS connections running on port " + bindPort);
                Out.WriteLine("Listening for MUS connections from " + musHost);
                return true;
            }

            catch
            {
                Out.WriteError("Error while setting up asynchronous socket server for MUS connections on port " + bindPort);
                Out.WriteError("Port " + bindPort + " could be invalid or in use already.");
                return false;
            }
        }
        private static void connectionRequest(IAsyncResult iAr)
        {
            Socket newSocket = ((Socket)iAr.AsyncState).EndAccept(iAr);
            if (newSocket.RemoteEndPoint.ToString().Split(':')[0] != _musHost)
            {
                newSocket.Close();
                return;
            }

            musConnection newConnection = new musConnection(newSocket);
            socketHandler.BeginAccept(new AsyncCallback(connectionRequest), socketHandler);
        }
        /// <summary>
        /// Represents an asynchronous, one time usage, socket connection between the emulator and HoloCMS, used for various live updates between site & server.
        /// </summary>
        private class musConnection
        {
            private Socket Connector;
            private byte[] dataBuffer = new byte[10001];
            /// <summary>
            /// Initializes the musConnection and listens for one single packet, processes it and closes the connection. On any error, the connection is closed.
            /// </summary>
            /// <param name="Connector">The socket of the musConnection.</param>
            internal musConnection(Socket Connector)
            {
                this.Connector = Connector;
                Connector.BeginReceive(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, new AsyncCallback(dataArrival), null);
            }
            /// <summary>
            /// Called when a packet is received, the packet will be processed and the connection will be closed (after processing packet) in all cases. No errors will be thrown.
            /// </summary>
            /// <param name="iAr"></param>
            private void dataArrival(IAsyncResult iAr)
            {
                try
                {
                    int bytesReceived = Connector.EndReceive(iAr);
                    string Data = System.Text.Encoding.ASCII.GetString(dataBuffer, 0, bytesReceived);

                    string musHeader = Data.Substring(0, 4);
                    string[] musData = Data.Substring(4).Split(Convert.ToChar(2));

                    Out.WriteLine("DATA: " + Data);

                    #region MUS trigger commands
                    // Unsafe code, but on any error it jumps to the catch block & disconnects the socket
                    switch (musHeader)
                    {
                        case "HKTM": // Housekeeping - textmessage [BK] :: "HKTM123This is a test message to user with ID 123" 
                            {
                                int userID = int.Parse(musData[0]);
                                string Message = musData[1];
                                userManager.getUser(userID).sendData("BK" + Message);
                                break;
                            }

                        case "HKMW": // Housekeeping - alert user [mod warn] :: "HKMW123This is a test mod warn to user with ID 123" 
                            {
                                int userID = int.Parse(musData[0]);
                                string Message = musData[1];
                                userManager.getUser(userID).sendData("B!" + Message + Convert.ToChar(2));
                                break;
                            }

                        case "HKUK": // Housekeeping - kick user from room [mod warn] :: "HKUK123This is a test kick from room + modwarn for user with ID 123" 
                            {
                                int userID = int.Parse(musData[0]);
                                string Message = musData[1];
                                virtualUser User = userManager.getUser(userID);
                                if (User.Room != null)
                                    User.Room.removeUser(User.roomUser.roomUID, true, Message);
                                break;
                            }

                        case "HKAR": // Housekeeping - alert certain rank with BK message, contains flag to include users with higher rank :: "HKAR11This is a test message for all users with rank 1 and higher, so kindof a Hotel alert :D" 
                            {
                                byte Rank = byte.Parse(musData[0]);
                                bool includeHigher = (musData[1] == "1");
                                string Message = musData[2];
                                userManager.sendToRank(Rank, includeHigher, "BK" + Message);
                                break;
                            }

                        case "HKSB": // Housekeeping - ban user & kick from room :: "HKSB123This is a test ban for user with ID 123" 
                            {
                                int userID = int.Parse(musData[0]);
                                string Message = musData[1];
                                virtualUser User = userManager.getUser(userID);
                                User.sendData("@c" + Message);
                                User.Disconnect(1000);
                                break;
                            }

                        case "HKRC": // Housekeeping - rehash catalogue :: "HKRC"
                            {
                                catalogueManager.Init();
                                break;
                            }

                        case "UPRA": // User profile - reload figure, sex and mission (poof!)
                            {
                                int userID = int.Parse(musData[0]);
                                userManager.getUser(userID).refreshAppearance(true, true, true);
                                break;
                            }

                        case "UPRC": // User profile - reload credits
                            {
                                int userID = int.Parse(musData[0]);
                                userManager.getUser(userID).refreshValueables(true, false);
                                break;
                            }

                        case "UPRT": // User profile - reload tickets
                            {
                                int userID = int.Parse(musData[0]);
                                userManager.getUser(userID).refreshValueables(false, true);
                                break;
                            }

                        case "UPRS": // User profile - reload subscription (and badges)
                            {
                                int userID = int.Parse(musData[0]);
                                virtualUser User = userManager.getUser(userID);
                                User.refreshClub();
                                User.refreshBadges();
                                break;
                            }

                        case "UPRH":// User profile - reload hand
                            {
                                int userID = int.Parse(musData[0]);
                                userManager.getUser(userID).refreshHand("new");
                                break;
                            }
                    }
                    #endregion
                }
                catch { }
                finally {killConnection();}
            }
            /// <summary>
            /// Closes the connection and destroys the object.
            /// </summary>
            private void killConnection()
            {
                try { Connector.Close(); }
                catch { }
            }
        } 
    }
}
