using System;
using System.Threading;

using Holo.Managers;
using Holo.Socketservers;

namespace Holo
{
    /// <summary>
    /// The core of Holograph Emulator codename "Eucalypt", contains Main() void for booting server, plus monitoring thread and shutdown void.
    /// </summary>
    public class Eucalypt
    {
        private static Thread serverMonitor = new Thread(new ThreadStart(monitorServer));
        /// <summary>
        /// Starts up Holograph Emulator codename "Eucalypt".
        /// </summary>
        private static void Main()
        {
            Console.WindowHeight = Console.LargestWindowHeight - 25;
            Console.WindowWidth = Console.LargestWindowWidth - 25;
            Console.CursorVisible = false;
            Console.Title = "Holograph Emulator";
            Out.WritePlain("HOLOGRAPH EMULATOR#########################################################");
            Out.WritePlain("THE FREE OPEN-SOURCE HABBO HOTEL EMULATOR");
            Out.WritePlain("COPYRIGHT (C) 2007-2008 BY HOLOGRAPH TEAM");
            Out.WritePlain("FOR MORE DETAILS CHECK LEGAL.TXT");
            Out.WriteBlank();
            Out.WritePlain("BUILD");
            Out.WritePlain(" CORE: Eucalypt, C#.NET");
            Out.WritePlain(" CLIENT: V22+");
            Out.WriteBlank();

            Boot();

            //while (true) // Infinite loop, keeping console window open and rejecting all input
            //    Console.ReadKey(true);

        }
        /// <summary>
        /// Boots the emulator.
        /// </summary>
        private static void Boot()
        {
            ThreadPool.SetMaxThreads(300,400);
            DateTime _START = DateTime.Now;
            Out.WriteLine("Starting up Holograph Emulator for " + Environment.UserName + "...");
            Out.WriteLine("Expanded threadpool.");
            Out.WriteLine(@"Checking for \bin\mysql.ini...");

            string sqlConfigLocation = IO.workingDirectory + @"\bin\mysql.ini";
            if (System.IO.File.Exists(sqlConfigLocation) == false)
            {
                Out.WriteError("mysql.ini not found at " + sqlConfigLocation);
                Shutdown();
                return;
            }

            Out.WriteLine("mysql.ini found at " + sqlConfigLocation);
            Out.WriteBlank();

            string dbHost = IO.readINI("mysql", "host", sqlConfigLocation);
            int dbPort = int.Parse(IO.readINI("mysql", "port", sqlConfigLocation));
            string dbUsername = IO.readINI("mysql", "username", sqlConfigLocation);
            string dbPassword = IO.readINI("mysql", "password", sqlConfigLocation);
            string dbName = IO.readINI("mysql", "database", sqlConfigLocation);

            if (DB.openConnection(dbHost, dbPort, dbName, dbUsername, dbPassword) == false)
                return;

            Out.WriteBlank();


            int gamePort;
            int gameMaxConnections;
            int musPort;
            int musMaxConnections;
            string musHost;

            

            try
            {
                gamePort = int.Parse(Config.getTableEntry("server_game_port"));
                gameMaxConnections = int.Parse(Config.getTableEntry("server_game_maxconnections"));
                musPort = int.Parse(Config.getTableEntry("server_mus_port"));
                musMaxConnections = int.Parse(Config.getTableEntry("server_mus_maxconnections"));
                musHost = Config.getTableEntry("server_mus_host");
            }

            catch
            {
                Out.WriteError("system_config table contains invalid values for socket server configuration!");
                Shutdown();
                return;
            }

            string langExt = Config.getTableEntry("lang");
            if (langExt == "")
            {
                Out.WriteError("No valid language extension [field: lang] was set in the system_config table!");
                Shutdown();
                return;
            }

            stringManager.Init(langExt);
            Out.WriteBlank();

            stringManager.initFilter();
            Out.WriteBlank();

            catalogueManager.Init();
            Out.WriteBlank();

            recyclerManager.Init();
            Out.WriteBlank();

            rankManager.Init();
            Out.WriteBlank();

            Config.Init();
            Out.WriteBlank();

            userManager.Init();
            eventManager.Init();

            if (gameSocketServer.Init(gamePort, gameMaxConnections) == false)
            {
                Shutdown();
                return;
            }
            Out.WriteBlank();

            if (musSocketServer.Init(musPort, musHost) == false)
            {
                Shutdown();
                return;
            }
            Out.WriteBlank();

            resetDynamics();
            Out.WriteBlank();

            printDatabaseStats();
            Out.WriteBlank();

            DateTime _STOP = DateTime.Now;
            TimeSpan _TST = _STOP - _START;
            Out.WriteLine("Startup time in fixed milliseconds: " + _TST.TotalMilliseconds.ToString() + ".");
            
            GC.Collect();
            Out.WriteLine("Holograph Emulator ready. Status: idle");
            Out.WriteBlank();

            Out.minimumImportance = Out.logFlags.MehAction; // All logs
            serverMonitor.Priority = ThreadPriority.Lowest;
            serverMonitor.Start();
        }
        /// <summary>
        /// Safely shutdowns Holograph Emulator, closing database and socket connections. Requires key press from user for final abort.
        /// </summary>
        public static void Shutdown()
        {
            Out.WriteBlank();
            if(serverMonitor.IsAlive)
                serverMonitor.Abort();
            
            DB.closeConnection();
            Out.WriteLine("Shutdown completed. Press a key to exit.");
            Console.ReadKey(true);
            Console.Beep(1400,1000);
            Environment.Exit(2);
        }
        /// <summary>
        /// Prints the usercount, guestroomcount and furniturecount in datebase to console.
        /// </summary>
        private static void printDatabaseStats()
        {
            int userCount = DB.runRead("SELECT COUNT(*) FROM users",null);
            int roomCount = DB.runRead("SELECT COUNT(*) FROM rooms",null);
            int itemCount = DB.runRead("SELECT COUNT(*) FROM furniture",null);
            Out.WriteLine("Result: " + userCount + " users, " + roomCount + " rooms and " + itemCount + " furnitures.");
        }
        private static void resetDynamics()
        {
            DB.runQuery("UPDATE system SET onlinecount = '0',onlinecount_peak = '0',connections_accepted = '0',activerooms = '0'");
            DB.runQuery("UPDATE users SET ticket_sso = NULL");
            DB.runQuery("UPDATE rooms SET visitors_now = '0'");

            Out.WriteLine("Client connection statistics reset.");
            Out.WriteLine("Room inside counts reset.");
            Out.WriteLine("Login tickets nulled.");
        }
        /// <summary>
        /// Threaded void. Ran on background thread at lowest priority, interval = 3500 ms. Updates console title and online users count, active rooms count, peak connections count and peak online users count in database.
        /// </summary>
        private static void monitorServer()
        {
            while(true)
            {
                int onlineCount = userManager.userCount;
                int peakOnlineCount = userManager.peakUserCount;
                int roomCount = roomManager.roomCount;
                int peakRoomCount = roomManager.peakRoomCount;
                int acceptedConnections = gameSocketServer.acceptedConnections;
                long memUsage = GC.GetTotalMemory(false) / 1024;

                Console.Title = "Holograph Emulator | online users: " + onlineCount + " | loaded rooms " + roomCount + " | RAM usage: " + memUsage + "KB";
                DB.runQuery("UPDATE system SET onlinecount = '" + onlineCount + "',onlinecount_peak = '" + peakOnlineCount + "',activerooms = '" + roomCount + "',activerooms_peak = '" + peakRoomCount + "',connections_accepted = '" + acceptedConnections + "'");
                Thread.Sleep(6000);
                Out.WriteTrace("Servermonitor loop");
            }
        }
    }
}