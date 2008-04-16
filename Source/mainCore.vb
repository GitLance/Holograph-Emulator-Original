Imports System.Threading
Module mainCore
    Public Sub Main()
        Console.Title = "Holograph Emulator | BOOTING"
        Console.SetWindowSize(120, 40)

        If HoloRACK.gameSocket_Port = 0 Then Launch()

        While True
            Dim cINPUT As String = Console.ReadLine()
            If Not (cINPUT = vbNullString) Then
                Select Case cINPUT
                    Case "about" '// Show information about Holograph Emulator
                        Call Console.WriteLine("[ABOUT] Holograph is the light-weight open source VB Habbo Hotel emulator, check progress on RaGEZONE MMORPG Development forums.")

                    Case "clear" '// Clear the console and re-print the starting message
                        Console.Clear()
                        printProperties()

                    Case "exit", "shutdown" '// Shutdown the server
                        Shutdown()

                    Case "stats" '// View the stats of your database
                        If HoloRACK.gameSocket_Port > 0 Then
                            Dim dbStatField() As Integer = HoloDB.runReadRow("SELECT users,guestrooms,furnitures FROM system", Nothing)
                            Console.WriteLine("[STATS] Holograph Emulator found " & dbStatField(0) & " users, " & dbStatField(1) & " guestrooms and " & dbStatField(2) & " furnitures.")
                            Console.WriteLine("[STATS] Online users: " & HoloMANAGERS.hookedUsers.Count & ", online peak: " & HoloRACK.onlinePeak & ", accepted connections: " & HoloRACK.acceptedConnections & ".")
                        Else
                            Console.WriteLine("[ERROR] Not connected to database.")
                        End If

                End Select
            End If
        End While
    End Sub
    Private Sub Launch()
        printProperties()
        Console.WriteLine("[SERVER] Starting up server for " & Environment.UserName & "...")
        Console.WriteLine("[SERVER] Checking for \bin\mysql.ini...")
        Console.WriteLine(vbNullString)
        HoloRACK = New clsHoloRACK

        HoloRACK.configFileLocation = My.Application.Info.DirectoryPath & "\bin\mysql.ini"
        If My.Computer.FileSystem.FileExists(HoloRACK.configFileLocation) = False Then
            Console.WriteLine("[SERVER] Couldn't find mysql.ini in " & HoloRACK.configFileLocation)
            Thread.Sleep(1000)
            'Terminate()
        End If

        Console.WriteLine("[SERVER] mysql.ini found at " & HoloRACK.configFileLocation)
        Console.WriteLine(vbNullString)

        Dim dbHost As String = readINI("mysql", "host", HoloRACK.configFileLocation)
        Dim dbPort As Integer = Integer.Parse(readINI("mysql", "port", HoloRACK.configFileLocation))
        Dim dbUsername As String = readINI("mysql", "username", HoloRACK.configFileLocation)
        Dim dbPassword As String = readINI("mysql", "password", HoloRACK.configFileLocation)
        Dim dbName As String = readINI("mysql", "database", HoloRACK.configFileLocation)
        Dim dbPassword2 As String = vbNullString
        For i = 1 To dbPassword.Length : dbPassword2 += "*" : Next
        If dbPassword2.Length = 0 Then dbPassword2 = "EMPTY"
        Console.WriteLine("[MYSQL] Attempting to connect to " & dbName & " on " & dbHost & ":" & dbPort & " with username: " & dbUsername & " and password: " & dbPassword2 & "...")

        HoloDB = New clsHoloDB()
        If HoloDB.openConnection(dbHost, dbPort, dbName, dbUsername, dbPassword) = False Then Return '// Error at connecting, class will sort it out
        Console.WriteLine("[MYSQL] Connection to database established.")

        Dim dbCountCheck(2) As String
        dbCountCheck(0) = HoloDB.runRead("SELECT COUNT(*) FROM users")
        dbCountCheck(1) = HoloDB.runRead("SELECT COUNT(*) FROM guestrooms")
        dbCountCheck(2) = HoloDB.runRead("SELECT COUNT(*) FROM furniture")

        If dbCountCheck(0) = "" Then
            Console.WriteLine("[MYSQL] There is something wrong with database! 'system' table doesn't exist/doesn't contain required fields!")
            Thread.Sleep(1000)
            Shutdown()
        End If

        For i = 1 To 7
            HoloRANK(i) = New clsHoloRANK
            HoloRANK(i).fuseRights = HoloDB.runReadColumn("SELECT fuseright FROM rank_fuserights WHERE minrank <= '" & i & "'", 0)
            For f = 0 To HoloRANK(i).fuseRights.Count - 1
                HoloRANK(i).strFuse += HoloRANK(i).fuseRights(f) & Convert.ToChar(2)
            Next
        Next
        Console.WriteLine("[SERVER] Rank templates loaded.")

        HoloRACK.wordFilter_Words = HoloDB.runReadColumn("SELECT word FROM wordfilter", 0)
        HoloRACK.wordFilter_Replacement = getConfigEntry("wordfilter_censor")

        If getConfigEntry("wordfilter_enable") = "1" Then
            If HoloRACK.wordFilter_Words.Count = 0 Or HoloRACK.wordFilter_Replacement = vbNullString Then
                Console.WriteLine("[SERVER] Word filter was preferred as enabled but no words and/or replacement found, wordfilter disabled.")
            Else
                HoloRACK.wordFilter_Enabled = True
                Console.WriteLine("[SERVER] Word filter enabled, " & HoloRACK.wordFilter_Words.Count & " word(s) found, replacement: " & HoloRACK.wordFilter_Replacement)
            End If
        Else
            Console.WriteLine("[SERVER] Word filter disabled.")
        End If

        If getConfigEntry("welcomemessage_enable") = "1" Then
            HoloRACK.welcMessage = getConfigEntry("welcomemessage_text")
            Console.WriteLine("[SERVER] Welcome message loaded.")
        End If

        If getConfigEntry("chatanims_enable") = "1" Then
            HoloRACK.Chat_Animations = True
            Console.WriteLine("[SERVER] Chat animations set.")
        End If

        If getConfigEntry("trading_enable") = "1" Then
            HoloRACK.enableTrading = True
            Console.WriteLine("[SERVER] Trading enabled.")
        Else
            Console.WriteLine("[SERVER] Trading disabled.")
        End If

        If getConfigEntry("recycler_enable") = "1" Then
            HoloRACK.enableRecycler = True
            HoloCLIENT.recyclerManager.Init()
            Console.WriteLine("[SERVER] Recycler enabled.")
        Else
            Console.WriteLine("[SERVER] Recycler disabled.")
        End If

        HoloRACK.roomModels = New Hashtable
        For i = 1 To 18
            HoloRACK.roomModels.Add(i, Convert.ToChar(i + 96))
            HoloSTATICMODEL(i) = New clsHoloSTATICMODEL

            Dim roomDoor() As String = HoloDB.runRead("SELECT door FROM guestroom_modeldata WHERE model = '" & HoloRACK.roomModels(i) & "'").Split(",")
            With HoloSTATICMODEL(i)
                .doorX = roomDoor(0)
                .doorY = roomDoor(1)
                .doorH = Double.Parse(roomDoor(2))
                .strMap = HoloDB.runRead("SELECT map_height FROM guestroom_modeldata WHERE model = '" & HoloRACK.roomModels(i) & "'")
            End With
        Next
        Console.WriteLine("[SERVER] Room model templates loaded.")

        HoloCLIENT.catalogueManager.Init()
        resetDynamics()

        Dim langExt As String = getConfigEntry("lang")
        If langExt = vbNullString Then
            Console.WriteLine("[ERROR] No valid language extension was set in the system table!")
            Shutdown()
            Return
        End If

        HoloSTRINGS = New clsHoloSTRINGS(langExt)

        Console.WriteLine("[MYSQL] Found " & dbCountCheck(0) & " users, " & dbCountCheck(1) & " guestrooms and " & dbCountCheck(2) & " furnitures.")
        Console.WriteLine(vbNullString)

        Try
            HoloRACK.gameSocket_Port = getConfigEntry("game_port")
            HoloRACK.gameSocket_maxConnections = getConfigEntry("game_maxconnections")
            HoloRACK.musSocket_Port = getConfigEntry("mus_port")
            HoloRACK.musSocket_maxConnections = getConfigEntry("mus_maxconnections")
            HoloRACK.musSocket_Host = getConfigEntry("mus_host")

        Catch
            Console.WriteLine("[SERVER] system_config table contains invalid values for the socket keys!")
            Shutdown()
            Return

        End Try

        '// Set up the game socket listener
        HoloSCKMGR = New clsHoloSCKMGR()
        HoloSCKMGR.listenConnections()

        '// Set up the MUS socket listener
        HoloMUSMGR = New clsHoloMUSMGR()
        HoloMUSMGR.listenConnections()

        HoloMANAGERS = New clsHoloMANAGERS()
        HoloENCODING = New HoloENCODING()
        HoloMISC = New clsHoloMISC()


        serverMonitor.IsBackground = True
        serverMonitor.Priority = ThreadPriority.Lowest
        serverMonitor.Start()

        Console.WriteLine("[SERVER] Ready for connections.")
    End Sub
    Public Sub Shutdown()
        On Error Resume Next
        Console.WriteLine(vbNullString)
        If serverMonitor.IsAlive = True Then serverMonitor.Abort()
        resetDynamics()
        Console.WriteLine("[MYSQL] Closing existing database connection...")
        HoloDB.closeConnection()

        Thread.Sleep(500)
        Environment.Exit(2)
    End Sub
#Region "Declarements"
    Private serverMonitor As New Thread(AddressOf monitorServer)
    Private itemCache As Hashtable
    Public HoloDB As clsHoloDB
    Public HoloSCKMGR As clsHoloSCKMGR
    Public HoloMUSMGR As clsHoloMUSMGR
    Public HoloRACK As New clsHoloRACK
    Public HoloSTRINGS As clsHoloSTRINGS
    Public HoloMANAGERS As clsHoloMANAGERS
    Public HoloENCODING As HoloENCODING '// Jeax's Habbo encoding class for .NET, featuring B64 and VL64
    Public HoloRANK(7) As clsHoloRANK
    Public HoloMISC As clsHoloMISC
    Public HoloSTATICMODEL(18) As clsHoloSTATICMODEL
    Public HoloBBGAMELOBBY As clsHoloBBGAMELOBBY
    Private Declare Unicode Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringW" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpDefault As String, ByVal lpReturnedString As String, ByVal nSize As Int32, ByVal lpFileName As String) As Int32
    Private Declare Unicode Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringW" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpString As String, ByVal lpFileName As String) As Int32
#End Region
#Region "Private helper subs"
    Private Sub printProperties()
        Console.WriteLine("HOLOGRAPH***********************************************************")
        Console.WriteLine("THE FREE OPEN-SOURCE HABBO HOTEL EMULATOR")
        Console.WriteLine("FOR MORE DETAILS CHECK LEGAL.TXT")
        Console.WriteLine("COPYRIGHT (C) 2007-2008 BY HOLOGRAPH TEAM")
        Console.WriteLine(vbNullString)
        Console.WriteLine("VERSION:")
        Console.WriteLine(" CORE: V" & My.Application.Info.Version.Major)
        Console.WriteLine(" MAJOR FUNCTIONS: LIB " & My.Application.Info.Version.Minor)
        Console.WriteLine(" BUILD: B" & My.Application.Info.Version.Build)
        Console.WriteLine(" CLIENT: V21")
        Console.WriteLine(vbNullString)
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
    Private Sub monitorServer()
        While True
            Dim onlineCount As Integer = HoloMANAGERS.hookedUsers.Count
            Dim memUsage As Integer = GC.GetTotalMemory(False) / 1024
            Console.Title = "Holograph Emulator | online users: " & onlineCount & " | loaded rooms: " & HoloMANAGERS.hookedRooms.Count & " | RAM usage: " & memUsage & "KB"
            HoloDB.runQuery("UPDATE system SET onlinecount = '" & onlineCount & "'")
            If onlineCount > HoloRACK.onlinePeak Then HoloRACK.onlinePeak = onlineCount
            Thread.Sleep(3500)
        End While
    End Sub
#End Region
#Region "Item cache management"
    Public Function HoloITEM(ByVal templateID As Integer) As cachedItem
        Return HoloCLIENT.catalogueManager.getItemTemplate(templateID)
    End Function
    Public Class cachedItem
        Sub Init(ByVal cctName As String, ByVal typeID As Byte, ByVal Colour As String, ByVal Length As Integer, ByVal Width As Integer, ByVal topH As Double, ByVal isDoor As Boolean, ByVal isTradeable As Boolean, ByVal isRecycleable As Boolean)
            If cctName.Contains(" ") = True Then
                Me.cctName = cctName.Split(" ")(0)
                Me.Colour = cctName.Split(" ")(1)
            Else
                Me.cctName = cctName
                Me.Colour = Colour
            End If
            Me.typeID = typeID
            Me.Length = Length
            Me.Width = Width
            Me.topH = topH
            Me.isDoor = isDoor
            Me.isTradeable = isTradeable
            Me.isRecycleable = isRecycleable
            '  Console.WriteLine("[HOLOCACHE] Cached furniture template [" & cctName & "]")
        End Sub
        Friend typeID As Byte '// The type of the item: 1 = solid, 2 = seat, 3 = bed, 4 = rug
        Friend cctName As String '// The name of the CCT of this item
        Friend Colour As String '// The colour for this item
        Friend Length, Width As Integer '// The length and width of this item
        Friend topH As Double '// The offset of the top of this item, so if you stack ontop of this then it's ontop of the current height + this top height
        Friend isDoor As Boolean '// Item is useable as door yes/no
        Friend isTradeable As Boolean '// Item is tradeable yes/no
        Friend isRecycleable As Boolean '// Item is valid for the Ecotron/Recycler
    End Class
#End Region
#Region "Other functions"
    Private Function readINI(ByVal iniSection As String, ByVal iniKey As String, ByVal iniLocation As String) As String
        Dim retLen As Integer
        Dim retStr As String
        retStr = Space$(1024)
        retLen = GetPrivateProfileString(iniSection, iniKey, vbNullString, retStr, retStr.Length, iniLocation)
        If retLen > 0 Then Return retStr.Substring(0, retLen)
        Return vbNullString
    End Function
    Private Sub modINI(ByVal iniSection As String, ByVal iniKey As String, ByVal strNewContent As String, ByVal iniLocation As String)
        WritePrivateProfileString(iniSection, iniKey, strNewContent, iniLocation)
    End Sub
    Public Function getConfigEntry(ByVal strKey As String) As String
        Return HoloDB.runRead("SELECT sval FROM system_config WHERE skey = '" & strKey & "'")
    End Function
    Public Function rndVal(ByVal minVal As Long, ByVal maxVal As Long) As Integer
        Dim v As New Random
        Return v.Next(minVal, maxVal + 1)
    End Function
    Public Function substringIs(ByVal strInput As String, ByVal strIs As String, ByVal index As Integer, ByVal length As Integer) As Boolean
        Try
            If strInput.Substring(index, length) = strIs Then Return True
            Return False

        Catch
            Return False

        End Try
    End Function
#End Region
End Module
