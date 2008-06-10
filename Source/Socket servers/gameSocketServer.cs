using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

using Holo.Virtual.Users;

namespace Holo.Socketservers
{
    /// <summary>
    /// Asynchronous socket server for the game connections.
    /// </summary>
    public static class gameSocketServer
    {
        private static Socket socketHandler;
        private static int _Port;
        private static int _maxConnections;
        private static int _acceptedConnections;
        private static HashSet<int> _activeConnections;

        /// <summary> 
        /// Initializes the socket listener for game connections and starts listening. 
        /// </summary> 
        /// <param name="bindPort">The port where the socket listener should be bound to.</param> 
        /// <param name="maxConnections">The maximum amount of simultaneous connections.</param> 
        /// <remarks></remarks> 
        internal static bool Init(int bindPort, int maxConnections)
        {
            _Port = bindPort;
            _maxConnections = maxConnections;
            _activeConnections = new HashSet<int>();
            socketHandler = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Out.WriteLine("Starting up asynchronous socket server for game connections for port " + bindPort + "...");
            try
            {
                socketHandler.Bind(new IPEndPoint(IPAddress.Any, bindPort));
                socketHandler.Listen(25);
                socketHandler.BeginAccept(new AsyncCallback(connectionRequest), socketHandler);

                Out.WriteLine("Asynchronous socket server for game connections running on port " + bindPort);
                Out.WriteLine("Max simultaneous connections is " + maxConnections);
                return true;
            }

            catch
            {
                Out.WriteError("Error while setting up asynchronous socket server for game connections on port " + bindPort);
                Out.WriteError("Port " + bindPort + " could be invalid or in use already.");
                return false;
            }
        }
        private static void connectionRequest(IAsyncResult iAr)
        {
            try
            {
                int connectionID = 0;
                for (int i = 1; i < _maxConnections; i++)
                {
                    if (_activeConnections.Contains(i) == false)
                    {
                        connectionID = i;
                        break;
                    }
                }

                if (connectionID > 0)
                {
                    Socket connectionSocket = ((Socket)iAr.AsyncState).EndAccept(iAr);
                    Out.WriteLine("Accepted connection [" + connectionID + "] from " + connectionSocket.RemoteEndPoint.ToString().Split(':')[0]);
                    _activeConnections.Add(connectionID);
                    _acceptedConnections++;

                    virtualUser newUser = new virtualUser(connectionID, connectionSocket);
                }
            }
            catch { }
            socketHandler.BeginAccept(new AsyncCallback(connectionRequest), socketHandler);
        }
        /// <summary> 
        /// Flags a connection as free. 
        /// </summary> 
        /// <param name="connectionID">The ID of the connection.</param> 
        internal static void freeConnection(int connectionID)
        {
            if (_activeConnections.Contains(connectionID))
            {
                _activeConnections.Remove(connectionID);
                Out.WriteLine("Flagged connection [" + connectionID + "] as free.");
            }
        }
        internal static int maxConnections
        {
            /// <summary> 
            /// Gets or set an Integer for the maximum amount of connections at the same time. 
            /// </summary> 
            get
            {
                return _maxConnections;
            }
            set 
            {
                _maxConnections = value;
            }
        }
        internal static int acceptedConnections
        {
            /// <summary> 
            /// Returns as integer of the accepted connections count since init. 
            /// </summary> 
            get { return _acceptedConnections; }
        }
    }
}
