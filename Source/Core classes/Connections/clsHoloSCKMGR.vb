Imports System.Net
Imports System.Net.Sockets
Public Class clsHoloSCKMGR
    Friend socketHandler As Socket
    Private maxPacketDelay As Integer
    Private inuseSockets As New Collection
    Public Sub listenConnections()
        Dim localHost As New IPEndPoint(IPAddress.Any, HoloRACK.gameSocket_Port)
        socketHandler = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)

        Console.WriteLine("[SCKMGR] Setting up socket manager on port " & HoloRACK.gameSocket_Port & "...")
        Try '// Try setting up our SCKMGR on the port set in HoloRACK
            socketHandler.Bind(localHost)
            socketHandler.Listen(10) '// Start listening (10 means the max 'queue of connection ones' size)

            '// If it reaches here, setting up has succeeded!
            Console.WriteLine("[SCKMGR] Listening on port " & HoloRACK.gameSocket_Port & ".")
            Console.WriteLine(vbNullString)

            '// Start listening for connection requests
            socketHandler.BeginAccept(New AsyncCallback(AddressOf connectionRequest), socketHandler)

        Catch ex As Exception '// Noes,  setting up failed! Report it and stop server
            Console.WriteLine("[SCKMGR] Error encountered while setting up SCKMGR on port " & HoloRACK.gameSocket_Port & ", probably another application listens on this port already.")
            Shutdown()

        End Try
    End Sub
    Private Sub connectionRequest(ByVal c As IAsyncResult)
        Dim newClassID As Integer = 0
        For s = 1 To HoloRACK.gameSocket_maxConnections
            If inuseSockets.Contains(s.ToString) = False Then newClassID = s : Exit For
        Next

        If newClassID = 0 Then '// If the free socket returned was 0, then there were no sockets left! Change config.ini much?
            Console.WriteLine("[SCKMGR] Connection refused, the SCKMGR has ran out of it's " + HoloRACK.gameSocket_maxConnections & " free slots!")
            Return
        End If

        Dim newClassSocket As Socket = DirectCast(c.AsyncState, Socket).EndAccept(c)

        '# If MsgBox("Dear teh Nillus, do you want to allow " & newClassSocket.RemoteEndPoint.ToString.Split(":")(0) & " to the test server?", MsgBoxStyle.YesNo, "Holograph Emulator - Test server authentication") = MsgBoxResult.No Then
        '# newClassSocket.Close()
        '# newClassSocket = Nothing
        '# Return
        '# End If

        '// Initialize a copy of clsHoloUSER class and set it up
        '// Class contains set of user stats and a socket
        Dim newClient As New clsHoloUSER(newClassID, newClassSocket)
        inuseSockets.Add(newClassID, Nothing)
        HoloRACK.acceptedConnections += 1

        '// Check if more pending connections are waiting for the main socket (socketHandler)
        socketHandler.BeginAccept(New AsyncCallback(AddressOf connectionRequest), socketHandler)
    End Sub
    Friend Sub flagSocketAsFree(ByVal socketID As Integer)
        inuseSockets.Remove(socketID.ToString)
    End Sub
End Class
