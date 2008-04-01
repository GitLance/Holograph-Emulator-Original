Imports System.Threading
Public Module mainHoloAPP
    Private inpCommand As String
    Private serverMonitor As New Thread(AddressOf monitorServer)
    Sub Main()
        Console.Title = "Holograph Emulator"
        Console.SetWindowPosition(0, 0)
        mainHoloAPP.printHoloProps()

        While True
            inpCommand = Console.ReadLine
            If Not (inpCommand = vbNullString) Then mainHoloAPP.checkCommand(inpCommand)
        End While
    End Sub
    Private Sub printHoloProps()
        Console.WriteLine("HOLOGRAPH***********************************************************")
        Console.WriteLine("THE FREE OPEN-SOURCE HABBO HOTEL EMULATOR")
        Console.WriteLine("FOR MORE DETAILS CHECK LEGAL.TXT")
        Console.WriteLine("COPYRIGHT (C) 2007-2008 BY HOLOGRAPH TEAM")
        Console.WriteLine(vbNullString)
        Console.WriteLine("VERSION:")
        Console.WriteLine(" CORE: V" & My.Application.Info.Version.Major)
        Console.WriteLine(" MAJOR FUNCTIONS: LIB " & My.Application.Info.Version.Minor)
        Console.WriteLine(" REVISION: R" & My.Application.Info.Version.Revision)
        Console.WriteLine(" CLIENT: V21")
        Console.WriteLine(vbNullString)
        If HoloRACK.gamesocket_port = 0 Then startServer()
    End Sub
    Private Sub startServer()
        '// Dimension the variables
        Dim sqlPort As Integer
        Dim sqlHost, sqlDB, sqlUser, sqlPassword, sqlPassword_Hidden As String
        Dim dbCountCheck(2) As String

        With HoloRACK
            '// Set the system characters Chr(0-255) so we don't have to call the function everytime
            For P = 0 To 255
                sysChar(P) = Chr(P)
            Next

            Console.WriteLine("[SERVER] Starting up server for " & Environment.UserName & "...")
            Thread.Sleep(1000)
            Console.WriteLine("[SERVER] Attempting to retrieve settings from config.ini...")
            Console.WriteLine(vbNullString)
            Thread.Sleep(70)

            '// Set the directory where config.ini should be in the settings rack
            .configFileLocation = My.Application.Info.DirectoryPath & "\bin\config.ini"

            If My.Computer.FileSystem.FileExists(.configFileLocation) = False Then '// If the config.file in the /bin/ folder is not found
                '// Shutdown the server because the config.ini was not found
                Console.WriteLine("[SERVER] config.ini not found! Shutting down...")
                Thread.Sleep(1000)
                stopServer()
            End If

            Console.WriteLine("[SERVER] config.ini found at " & .configFileLocation)
            Console.WriteLine(vbNullString)

            '// Read the SQL details from the config.ini with the ReadINI function
            sqlHost = readINI("mysql", "host", .configFileLocation)
            sqlPort = Convert.ToInt16(readINI("mysql", "port", .configFileLocation))
            sqlUser = readINI("mysql", "username", .configFileLocation)
            sqlPassword = readINI("mysql", "password", .configFileLocation)
            sqlDB = readINI("mysql", "database", .configFileLocation)

            '// Make a line of ****s as long as the password, so you can run the server and others don't see the password
            sqlPassword_Hidden = vbNullString
            For P = 1 To sqlPassword.Length
                sqlPassword_Hidden += "*"
            Next

            If sqlPassword_Hidden = vbNullString Then sqlPassword_Hidden = "EMPTY" '// If the password is blank, then show 'EMPTY' as password

            '// Display we're attempting to connect, if it fails (so it returns false) then quit here
            Console.WriteLine("[MYSQL] Attempting to connect " & sqlDB & " on " & sqlHost & ":" & sqlPort & ", with UID: " & sqlUser & " and PWD: " & sqlPassword_Hidden)
            If HoloDB.openConnection(sqlHost, sqlPort, sqlDB, sqlUser, sqlPassword) = False Then Return

            Console.WriteLine("[MYSQL] Connection successfull.")
            Console.WriteLine(vbNullString)

            '// Read users, guestrooms and furnitures count from database
            dbCountCheck(0) = HoloDB.runRead("SELECT COUNT(*) FROM users")
            dbCountCheck(1) = HoloDB.runRead("SELECT COUNT(*) FROM guestrooms")
            dbCountCheck(2) = HoloDB.runRead("SELECT COUNT(*) FROM furniture")

            If dbCountCheck(0) = "" Then '// If there were no fields like that in the system table = something wrong with holodb
                Console.WriteLine("[MySQL] There is something wrong with database! Shutting down...")
                Thread.Sleep(400)
                stopServer()
            End If

            '// Load the settings from the config.ini file
            mainHoloAPP.loadPreferences()
            Console.WriteLine("[SERVER] Preferences succesfully loaded.")
            Console.WriteLine(vbNullString)

            '// Load the static room data (guestrooms)
            mainHoloAPP.loadRoomModels()
            Console.WriteLine("[SERVER] Loaded static guestroom data into memory.")
            Thread.Sleep(100)

            '// Load catalogue pages + item templates
            cacheCatalogue()

            '// Perform some housekeeping
            resetDynamics()

            '// Display the current database counts
            Console.WriteLine("[MYSQL] Found " & dbCountCheck(0) & " users, " & dbCountCheck(1) & " guestrooms and " & dbCountCheck(2) & " furnitures.")
            Console.WriteLine(vbNullString)

            '// Read the socket config from the config.ini file
            .gameSocket_Port = readINI("sockets", "port_game", .configFileLocation)
            .gameSocket_maxConnections = readINI("sockets", "maxconnections_game", .configFileLocation)
            .musSocket_Port = readINI("sockets", "port_mus", .configFileLocation)
            .musSocket_maxConnections = readINI("sockets", "maxconnections_mus", .configFileLocation)
            .musSocket_Host = readINI("sockets", "mus_host", .configFileLocation)

            '// Set up the game socket listener
            HoloSCKMGR.listenConnections()

            '// Set up the MUS socket listener
            HoloMUSMGR.listenConnections()

            serverMonitor.IsBackground = True
            serverMonitor.Priority = ThreadPriority.Lowest
            serverMonitor.Start()

            Console.WriteLine("[SERVER] Ready for connections.")
        End With
    End Sub
    Private Sub resetDynamics()
        HoloDB.runQuery("UPDATE system SET onlinecount = '0'")
        HoloDB.runQuery("UPDATE users SET ticket_sso = NULL")
        HoloDB.runQuery("UPDATE guestrooms SET incnt_now = '0'")
        HoloDB.runQuery("UPDATE publicrooms SET incnt_now = '0'")

        Console.WriteLine("[MYSQL] Online count reset.")
        Console.WriteLine("[MYSQL] Room inside counts reset.")
        Console.WriteLine("[MYSQL] SSO login tickets nulled.")
        Console.WriteLine(vbNullString)
    End Sub
    Friend Sub stopServer()
        Console.WriteLine(vbNullString)

        '// Stop the extra thread(s) if they are started
        If serverMonitor.IsAlive = True Then serverMonitor.Abort()

        '// Reset some stats
        resetDynamics()

        '// Close the database connection
        Console.WriteLine("[MYSQL] Closing existing database connection...")
        HoloDB.closeConnection()

        '// Wait 1,5 seconds so the user can read the messages displayed
        Sleep(1500)

        '// Terminate the process
        Process.GetCurrentProcess.Kill()
    End Sub
    Private Sub checkCommand(ByVal inpCommand As String)
        Dim checkHeader As String

        If inpCommand.Contains(" ") Then checkHeader = inpCommand.Split(" ")(0) Else checkHeader = inpCommand
        Select Case checkHeader

            Case "about" '// Show information about Holograph Emulator
                Call Console.WriteLine("[ABOUT] Holograph is the light-weight open source VB Habbo Hotel emulator, check progress on RaGEZONE MMORPG Development forums.")

            Case "clear" '// Clear the console and re-print the starting message
                Console.Clear()
                printHoloProps()

            Case "exit", "shutdown" '// Shutdown the server
                stopServer()

            Case "stats" '// View the stats of your database
                If HoloRACK.gamesocket_port > 0 Then
                    Dim dbStatField() As String
                    dbStatField = HoloDB.runReadArray("SELECT users,guestrooms,furnitures FROM system")
                    Console.WriteLine("[STATS] Holograph Emulator found " & dbStatField(0) & " users, " & dbStatField(1) & " guestrooms and " & dbStatField(2) & " furnitures.")
                    Console.WriteLine("[STATS] Online users: " & HoloRACK.onlineCount & ", online peak: " & HoloRACK.onlinePeak & ", accepted connections: " & HoloRACK.acceptedConnections & ".")
                Else
                    Console.WriteLine("[ERROR] Not connected to database.")
                End If

            Case "hotelalert"
                Dim hotelMessage As String = inpCommand.Split(" ")(1)
                For Each hotelUser As clsHoloUSER In HoloMANAGERS.hookedUsers.Values
                    hotelUser.transData("BK" & "Holograph Emulator says:\r" & hotelMessage & sysChar(1))
                Next

            Case Else '// Command not found
                Console.WriteLine("[COMMAND] Command '" & checkHeader & "' not found.")

        End Select
    End Sub
    Private Sub loadPreferences()
        '// Load the info for the rank profiles from the rank table in the database
        With HoloRACK

            For R = 1 To 7
                'rRank = HoloDB.runReadArray("SELECT ignorefilter,receivecfh,enterallrooms,seeallowners,admincatalogue,stafffloor,rightseverywhere FROM ranks WHERE rankid = '" & R & "'")
                HoloRANK(R) = New clsHoloRANK
                With HoloRANK(R)
                    .fuseRights = HoloDB.runReadArray("SELECT fuseright FROM rank_fuserights WHERE minrank <= '" & R & "'", True)
                    For f = 0 To .fuseRights.Count - 1
                        .strFuse += .fuseRights(f) & sysChar(2) '// Add this fuseright to the string for fuserights for this rank(all separated by CHAR2)
                    Next
                End With
            Next

            '// Load the wordfilter words
            HoloRACK.wordFilter_Words = HoloDB.runReadArray("SELECT word FROM wordfilter", True)
            HoloRACK.wordFilter_Replacement = HoloDB.runRead("SELECT wordfilter_replacement FROM system")

            '// Reset all preferences
            .wordFilter_Enabled = False
            .ssoLogin = False
            .welcMessage = vbNullString
            .chat_animations = True

            If readINI("game", "wordfilter", .configFileLocation) = "1" Then
                If HoloRACK.wordFilter_Words.Count = 0 Or HoloRACK.wordFilter_Replacement = vbNullString Then
                    Console.WriteLine("[WFILTER] Word filter was preferred as enabled but no words and/or replacement found, wordfilter disabled.")
                Else
                    HoloRACK.wordFilter_Enabled = True
                    Console.WriteLine("[WFILTER] Word filter enabled, " & HoloRACK.wordFilter_Words.Count & " word(s) found, replacement: " & HoloRACK.wordFilter_Replacement)
                End If
            Else
                Console.WriteLine("[WFILTER] Word filter disabled.")
            End If

            '// Load the login choice
            If readINI("login", "sso", .configFileLocation) = "1" Then .ssoLogin = True

            '// Load the welcome message from system table, if welcome messages are enabled
            If readINI("login", "welcome_message", .configFileLocation) = "1" Then .welcMessage = HoloDB.runRead("SELECT welcome_message FROM system")

            '// Load the 'Use animations during chat' choice
            If readINI("game", "chat_animations", .configFileLocation) = "1" Then .Chat_Animations = True

        End With
    End Sub
    Private Sub loadRoomModels()
        Dim m As Integer
        HoloRACK.roomModels = New Hashtable
        For m = 1 To 18
            HoloRACK.roomModels.Add(m, sysChar(m + 96))
            HoloSTATICMODEL(m) = New clsHoloSTATICMODEL

            Dim roomDoor() As String = HoloDB.runRead("SELECT door FROM guestroom_modeldata WHERE model = '" & HoloRACK.roomModels(m) & "' LIMIT 1").Split(",")
            With HoloSTATICMODEL(m)
                .doorX = roomDoor(0)
                .doorY = roomDoor(1)
                .doorH = Double.Parse(roomDoor(2))
                .strMap = HoloDB.runRead("SELECT map_height FROM guestroom_modeldata WHERE model = '" & HoloRACK.roomModels(m) & "' LIMIT 1")
            End With
        Next m
    End Sub
    Public Sub cacheCatalogue()
        Console.WriteLine(vbNullString)
        Console.WriteLine("[HOLOCACHE] Starting caching of catalogue + items, this may take a while...")

        Dim pageIDs() As String = HoloDB.runReadArray("SELECT indexid FROM catalogue_pages ORDER BY indexid", True)

        HoloRACK.cataloguePages = New Hashtable
        For i = 0 To pageIDs.Count - 1
            cacheCatalogue_page(pageIDs(i))
        Next '// Do the next page
        cacheCatalogue_page(-1) '// Cache all the items that aren't on a page, but require to be cached to prevent that already bought instances will appear as PH box. Items like this have catalogue_page_id = -1 in the catalogue_items table

        Console.WriteLine("[HOLOCACHE] Successfully cached the catalogue pages + the item templates!")
        Console.WriteLine(vbNullString)
    End Sub
    Private Sub cacheCatalogue_page(ByVal pageID As Integer)
        Dim pageData() As String = HoloDB.runReadArray("SELECT indexname,minrank,displayname,style_layout,img_header,img_side,label_description,label_misc,label_moredetails FROM catalogue_pages WHERE indexid = '" & pageID & "' LIMIT 1")
        If pageID > 0 Then If pageData.Count = 0 Then Return

        Dim pageBuilder As New System.Text.StringBuilder
        Dim pageIndexName As String = vbNullString
        Dim curPageCache As New clsHoloRACK.cachedCataloguePage '// Create new instance of cached page

        If pageID > 0 Then '// If it's a page + items, and not just not-on-page items, then cache the page
            pageIndexName = pageData(0)
            curPageCache.displayName = pageData(2) '// Set display name for this page
            pageBuilder.Append("i:" & pageIndexName & sysChar(13) & "n:" & pageData(2) & sysChar(13) & "l:" & pageData(3) & sysChar(13)) '// Add the required fields for catalogue page (indexname, showname, page layout style (boxes etc))
            If Not (pageData(4)) = vbNullString Then pageBuilder.Append("g:" & pageData(4) & sysChar(13)) '// If there's a headline image set, add it
            If Not (pageData(5)) = vbNullString Then pageBuilder.Append("e:" & pageData(5) & sysChar(13)) '// If there is/are side image(s) set, add it/them
            If Not (pageData(6)) = vbNullString Then pageBuilder.Append("h:" & pageData(6) & sysChar(13)) '// If there's a description set, add it
            If Not (pageData(8)) = vbNullString Then pageBuilder.Append("w:" & pageData(8) & sysChar(13)) '// If there's a 'Click here for more details' label set, add it
            If Not (pageData(7)) = vbNullString Then '// If the misc additions field is not blank
                Dim miscDetail() As String = pageData(7).Split(vbCrLf) '// Split the misc additions field to string array
                For m = 0 To miscDetail.Count - 1 : pageBuilder.Append(miscDetail(m) & sysChar(13)) : Next '// Go along all misc additions and add them, followed by Char13
            End If
        End If

        Dim pageItems() As String = HoloDB.runReadArray("SELECT catalogue_name,catalogue_description,catalogue_cost,tid,typeid,name_cct,length,width,colour,top FROM catalogue_items WHERE catalogue_id_page = '" & pageID & "' ORDER BY catalogue_id_index ASC", True) '// Get the item data of all the items on this page IN ONE STRING (sorted ascending by the catalogue_id_index field) and split them to a string array
        For c = 0 To pageItems.Count - 1
            Dim itemData() As String = pageItems(c).Split(sysChar(9)) '// Split the current item string to a string array
            Dim templateID As Integer = itemData(3) '// Get the template ID of the current item

            HoloITEM(templateID) = New cachedItemTemplate(itemData(5), itemData(4), itemData(8), itemData(6), itemData(7), itemData(9)) '// Cache the item's details
            If pageID = -1 Then Continue For '// A 'not on page, just for caching'-item, no page is being made, so not adding it to page

            pageBuilder.Append("p:" & itemData(0) & sysChar(9) & itemData(1) & sysChar(9) & itemData(2) & sysChar(9) & sysChar(9)) '// Add the common fields for both wallitem/flooritem
            If HoloITEM(templateID).typeID = 0 Then pageBuilder.Append("i") Else pageBuilder.Append("s") '// Wallitem or flooritem? This will do the trick!!111
            pageBuilder.Append(sysChar(9) & itemData(5) & sysChar(9)) '// Add a char9 + the cctname + char9
            If HoloITEM(templateID).typeID = 0 Then pageBuilder.Append(sysChar(9)) Else pageBuilder.Append("0" & sysChar(9)) '// If wallitem, then just add a char9, if flooritem, then add a 0 + char9
            If HoloITEM(templateID).typeID = 0 Then pageBuilder.Append(sysChar(9)) Else pageBuilder.Append(HoloITEM(templateID).Length & "," & HoloITEM(templateID).Width & sysChar(9)) '// If wallitem, then just add a char9, if flooritem, then add the item's width, item's length and a char9
            pageBuilder.Append(itemData(5) & sysChar(9)) '// Add the cctname again + char9
            If HoloITEM(templateID).typeID > 0 Then pageBuilder.Append(HoloITEM(templateID).Colour) '// If it's a flooritem, then add the colour
            pageBuilder.Append(sysChar(13)) '// Add char13 to mark the end of the current item string
        Next

        If pageID = -1 Then Return '// No page being generated, just caching all items that aren't on pages, so stop here
        curPageCache.strPage = pageBuilder.ToString() '// Unfold the stringbuilder for the current page and stow it in the caching instance of this page his 'strPage' property
        HoloRACK.cataloguePages.Add(pageIndexName, curPageCache) '// Add the current page cache instance to the hashtable
    End Sub
    Private Sub monitorServer()
        While True
            Dim onlineCount As Integer = HoloMANAGERS.hookedUsers.Count
            Dim memUsage As Integer = GC.GetTotalMemory(False) / 1024
            Console.Title = "Holograph Emulator | online users: " & onlineCount & " | loaded rooms: " & HoloMANAGERS.hookedRooms.Count & " | RAM usage: " & memUsage & "KB"
            HoloDB.runQuery("UPDATE system SET onlinecount = '" & onlineCount & "'")
            Thread.Sleep(3500) '// Wait 3,5 seconds before updating stats again
        End While
    End Sub
End Module