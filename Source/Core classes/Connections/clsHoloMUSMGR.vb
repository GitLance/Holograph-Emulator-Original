Imports System.Text
Imports System.Net
Imports System.Net.Sockets
Public Class clsHoloMUSMGR
    Private socketHandler As Socket
    Private hookedConnections As New Hashtable
    Public Sub listenConnections()
        Dim localHost As New IPEndPoint(IPAddress.Any, HoloRACK.musSocket_Port)
        socketHandler = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

        Console.WriteLine("[MUSMGR] Setting up mus socket manager on port " & HoloRACK.musSocket_Port & "...")
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
            Shutdown()

        End Try
    End Sub
    Private Sub connectionRequest(ByVal c As IAsyncResult)
        '// Will do the check for host later, so it allows connections for other hosts if the socket is used by the Camera
        Dim musConnector As Socket = DirectCast(c.AsyncState, Socket).EndAccept(c)

        Dim musHandler As New clsHoloMUSSCK(musConnector) '// Initialize new socket
        socketHandler.BeginAccept(New AsyncCallback(AddressOf connectionRequest), socketHandler) '// Listen for new connections
        ' Else '// The connection requester IP is not the same as the HoloCMS server entered in config.ini, this is obv a cunt trying to mess shizzle up, seriously get a life! :D No connection for this one please
        'musConnector.Close()
        'End If
    End Sub
    Private Class clsHoloMUSSCK
        Private Connector As Socket
        Private dataBuffer(10000) As Byte '//
        Sub New(ByVal newConnector As Socket)
            Me.Connector = newConnector
            Connector.BeginReceive(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, New AsyncCallback(AddressOf dataArrival), Nothing)
        End Sub
        Private Sub dataArrival(ByVal c As IAsyncResult)
lol:
            Try
                Dim bytesReceived As Integer = Connector.EndReceive(c)
                Dim strData As String = Encoding.ASCII.GetString(dataBuffer, 0, bytesReceived)
                Console.WriteLine(strData)

                If strData.Substring(0, 1) = "r" Then '// Camera binary transfer
                    Dim binChunk() As String = strData.Split(Convert.ToChar(0))
                    'Dim resData As String : For i = 0 To binChunk.Count - 1 : resData += binChunk(i) : Next

                    MsgBox(binChunk.Count)
                    Dim toDo As String = strData.Split(Convert.ToChar(0))(15).Substring(1)
                    MsgBox(toDo)
                    Dim doPack As New StringBuilder

                    If toDo = "Logon" Then
                        sendData(Convert.ToChar(114)) '// Send 'r'
                        doPack.Append(Convert.ToChar(0), 4) '// Send 4 nulls
                        doPack.Append("0") '// Send a 0
                        doPack.Append(Convert.ToChar(0), 12)
                        doPack.Append("Logon")
                        doPack.Append(Convert.ToChar(0), 5)
                        doPack.Append("System")
                        doPack.Append(Convert.ToChar(0), 8)
                        doPack.Append("!")
                        doPack.Append(Convert.ToChar(0), 7)
                        doPack.Append("FUSE")
                        doPack.Append(Convert.ToChar(114))
                        doPack.Append(Convert.ToChar(0), 4)
                        doPack.Append("4")
                        doPack.Append(Convert.ToChar(0), 12)
                        doPack.Append("HELLO")
                        doPack.Append(Convert.ToChar(0), 5)
                        doPack.Append("System")
                        doPack.Append(Convert.ToChar(0), 8)
                        doPack.Append("!")
                        doPack.Append(Convert.ToChar(0), 7)
                        doPack.Append("1")
                        doPack.Append(Convert.ToChar(0))
                    End If

                    sendData(doPack.ToString)
                    Threading.Thread.Sleep(1000)
                    killConnection()
                    'Connector.BeginReceive(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, New AsyncCallback(AddressOf dataArrival), Nothing)
                    Return
                Else '// HoloCMS MUS socket handling
                    If Not (Connector.RemoteEndPoint.ToString.Split(":")(0) = HoloRACK.musSocket_Host) Then killConnection() '// Packet comes not from HoloCMS server
                    Dim musHeader As String = strData.Substring(0, 4)
                    Dim musData() As String = strData.Substring(5).Split(Convert.ToChar(2))

                    Dim l As String = strData.Substring(0, 4)
                    MsgBox(l)
                    Dim rofl() As String = strData.Split(Convert.ToChar(5))

                    If 1 = 1 Then GoTo lol
                    Select Case musHeader

                        Case "HKTM" '// Housekeeping - textmessage [BK] :: "HKTM123This is a test message to user with ID 123"
                            Dim userID As Integer = musData(0)
                            Dim strMessage As String = musData(1)
                            HoloMANAGERS.getUserClass(userID).transData("BK" & strMessage)

                        Case "HKMW" '// Housekeeping - alert user [mod warn] :: "HKMW123This is a test mod warn to user with ID 123"
                            Dim userID As Integer = musData(0)
                            Dim strMessage As String = musData(1)
                            HoloMANAGERS.getUserClass(userID).transData("B!" & strMessage & Convert.ToChar(2))

                        Case "HKUK" '// Housekeeping - kick user from room [mod warn] :: "HKUK123This is a test kick from room + modwarn for user with ID 123"
                            Dim userID As Integer = musData(0)
                            Dim strMessage As String = musData(1)
                            Dim userClass As clsHoloUSER = HoloMANAGERS.getUserClass(userID)
                            userClass.roomCommunicator.removeUser(userClass.userDetails, True)
                            userClass.userDetails.Reset() '// Reset users room values
                            userClass.roomCommunicator = Nothing '// Null users room manager class

                        Case "HKAR" '// Housekeeping - alert certain rank with BK message, contains flag to include users with higher rank :: "HKAR11This is a test message for all users with rank 1 and higher, so kindof a Hotel alert :D"
                            Dim toRank As Integer = musData(0)
                            Dim includeHigher As Boolean = (musData(1) = "1")
                            Dim strMessage As String = musData(2)
                            HoloMANAGERS.sendToRank(toRank, includeHigher, "BK" & strMessage)

                        Case "HKSB" '// Housekeeping - ban user & kick from room :: "HKSB123This is a test ban for user with ID 123"
                            Dim userID As Integer = musData(0)
                            Dim strMessage As String = musData(1)
                            HoloMANAGERS.getUserClass(userID).handleBan(strMessage, "User [" & userID & "] was banned via Housekeeping for reason [" & strMessage & "]")

                        Case "HKRC" '// Housekeeping - rehash catalogue :: "HKRC"
                            HoloCLIENT.catalogueManager.Init()

                        Case "UPRA" '// User profile - reload figure, sex and mission
                            Dim userID As Integer = musData(0)
                            HoloMANAGERS.getUserClass(userID).refreshAppearance(True)

                        Case "UPRV" '// User profile - reload valuables (credits, tickets)
                            Dim userID As Integer = musData(0)
                            HoloMANAGERS.getUserClass(userID).refreshValuables()

                    End Select
                End If


            Catch '// Recklessness ftw, the only error that can occur is the user not being online, so a nullreference at the user class, but do we care? Just dump this mus socket
                killConnection()
                Return

            End Try
            killConnection() '// Action successfully processed, dump this mus socket
        End Sub
        Private Sub sendData(ByVal strData As String)
            Dim dataBytes() As Byte = Encoding.ASCII.GetBytes(strData)
            '// NO ASYNC :D | Connector.BeginSend(dataBytes, 0, dataBytes.Length, SocketFlags.None, New AsyncCallback(AddressOf sendData_complete), Nothing)
            Connector.Send(dataBytes, 0, dataBytes.Length, SocketFlags.None)
            Console.WriteLine(strData)
        End Sub
        Private Sub killConnection()
            On Error Resume Next
            Connector.Close()
            Me.Finalize()
        End Sub
    End Class
End Class