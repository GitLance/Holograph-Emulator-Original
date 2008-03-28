Imports System.Text
Imports System.Net
Imports System.Net.Sockets
Public Class clsHoloMUSMGR
    Private socketHandler As Socket
    Private hookedConnections As New Hashtable
    Public Sub listenConnections()
        Dim localHost As New IPEndPoint(IPAddress.Any, HoloRACK.musSocket_Port)
        socketHandler = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

        Try '// Try setting up the MUS socket manager
            socketHandler.Bind(localHost)
            socketHandler.Listen(50) '// Start listening (50 means the connection request que)

            '// MUS socket manager listening!
            Console.WriteLine("[MUSMGR] Listening on port " & HoloRACK.musSocket_Port & " for MUS connections from " & HoloRACK.musSocket_Host)
            Console.WriteLine(vbNullString)

            '// Start listening for connection requests
            socketHandler.BeginAccept(New AsyncCallback(AddressOf connectionRequest), socketHandler)

        Catch '// Setting up failed
            Console.WriteLine("[MUSMGR] Error encountered while setting up MUSMGR on port " & HoloRACK.musSocket_Port & ", probably another application listens on this port already.")
            mainHoloAPP.stopServer()

        End Try
    End Sub
    Private Sub connectionRequest(ByVal c As IAsyncResult)
        '// Will do the check for host later, so it allows connections for other hosts if the socket is used by the Camera
        Dim musConnector As Socket = DirectCast(c.AsyncState, Socket).EndAccept(c)
        If musConnector.RemoteEndPoint.ToString.Split(":")(0) = HoloRACK.musSocket_Host Then
            Dim musHandler As New clsHoloMUSSCK(musConnector) '// Initialize new socket
            socketHandler.BeginAccept(New AsyncCallback(AddressOf connectionRequest), socketHandler) '// Listen for new connections
        Else '// The connection requester IP is not the same as the HoloCMS server entered in config.ini, this is obv a cunt trying to mess shizzle up, seriously get a life! :D No connection for this one please
            musConnector.Close()
        End If
    End Sub
    Private Class clsHoloMUSSCK
        Private Connector As Socket
        Private dataBuffer(10000) As Byte '//
        Sub New(ByVal newConnector As Socket)
            Me.Connector = newConnector
            Connector.BeginReceive(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, New AsyncCallback(AddressOf dataArrival), Nothing)
        End Sub
        Private Sub dataArrival(ByVal c As IAsyncResult)
            Dim bytesReceived As Integer = Connector.EndReceive(c)
            Try
                Dim strData As String = Encoding.ASCII.GetString(dataBuffer, 0, bytesReceived)
                Dim musHeader As String = strData.Substring(0, 4)
                Dim musData() As String = strData.Substring(5).Split(sysChar(2))

                Select Case musHeader

                    Case "HKTM" '// Housekeeping - textmessage [BK] :: "HKTM123This is a test message to user with ID 123"
                        Dim userID As Integer = musData(0)
                        Dim strMessage As String = musData(1)
                        HoloMANAGERS.getUserClass(userID).transData("BK" & strMessage & sysChar(1))

                    Case "HKMW" '// Housekeeping - alert user [mod warn] :: "HKMW123This is a test mod warn to user with ID 123"
                        Dim userID As Integer = musData(0)
                        Dim strMessage As String = musData(1)
                        HoloMANAGERS.getUserClass(userID).transData("@amod_warn/" & strMessage & sysChar(1))

                    Case "HKUK" '// Housekeeping - kick user from room [mod warn] :: "HKUK123This is a test kick from room + modwarn for user with ID 123"
                        Dim userID As Integer = musData(0)
                        Dim strMessage As String = musData(1)
                        Dim userClass As clsHoloUSER = HoloMANAGERS.getUserClass(userID)
                        userClass.Room_noRoom(True, True)
                        userClass.transData("@amod_warn/" & strMessage & sysChar(1))

                    Case "HKAR" '// Housekeeping - alert certain rank with BK message, contains flag to include users with higher rank :: "HKAR11This is a test message for all users with rank 1 and higher, so kindof a Hotel alert :D"
                        Dim toRank As Integer = musData(0)
                        Dim includeHigher As Boolean = (musData(1) = "1")
                        Dim strMessage As String = musData(2)
                        HoloMANAGERS.sendToRank(toRank, includeHigher, "BK" & strMessage & sysChar(1))

                    Case "HKSB" '// Housekeeping - ban user & kick from room :: "HKSB123This is a test ban for user with ID 123"
                        Dim userID As Integer = musData(0)
                        Dim strMessage As String = musData(1)
                        Dim userClass As clsHoloUSER = HoloMANAGERS.getUserClass(userID)
                        userClass.Room_noRoom(True, True)
                        userClass.transData("@c" & strMessage & sysChar(1))

                    Case "HKRC" '// Housekeeping - rehash catalogue :: "HKRC"
                        mainHoloAPP.cacheCatalogue()

                    Case "UPRA" '// User profile - reload figure, sex and mission
                        Dim userID As Integer = musData(0)
                        HoloMANAGERS.getUserClass(userID).refreshAppearance(True)

                    Case "UPRV" '// User profile - reload valuables (credits, tickets)
                        Dim userID As Integer = musData(0)
                        HoloMANAGERS.getUserClass(userID).refreshValuables()

                End Select

            Catch '// Recklessness ftw, the only error that can occur is the user not being online, so a nullreference at the user class, but do we care? Just dump this mus socket
                killConnection()
                Return

            End Try
            killConnection() '// Action successfully processed, dump this mus socket
        End Sub
        Private Sub killConnection()
            On Error Resume Next
            Connector.Close()
            Me.Finalize()
        End Sub
    End Class
End Class
