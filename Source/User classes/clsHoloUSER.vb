Imports System
Imports System.Threading
Imports System.Net.Sockets
Imports System.Text
Imports System.Collections
Public Class clsHoloUSER
#Region "User server class properties"
    Private userSocket As Socket '// Users socket instance
    Friend classID As Integer '// Users socket/class ID
    Friend UserID As Integer '// Users ID in database
    Friend userDetails As clsHoloUSERDETAILS '// Users set of data
    Friend roomCommunicator As clsHoloROOM '// Reference to the room class used to communicate between this user and it's room

    Private dataBuffer(1024) As Byte '// User's current incoming data buffer [byte array]
    Private killedConnection As Boolean '// To prevent multi-use of the killConnection void
    Private timeOut As Byte '// Users connection status
    Private pingManager As Thread '// Thread slot for the pingmanager
    Private isLoggedIn As Boolean '// User logged in yes/no
    Private receivedItemIndex As Boolean '// If the user received that very big Dg packet containing the hof furni folders of all cct's
    Private curHandPage As Integer '// The current hand page the user is on
    Friend ReadOnly Property ipAddress() As String
        Get
            Return userSocket.RemoteEndPoint.ToString.Split(":")(0)
        End Get
    End Property
#End Region
#Region "Constructors"
    Public Sub New(ByVal newClassID As Integer, ByVal newSocket As Socket) '// Create a new instance of HoloUSER and accept connection handling (@@ packet etc)
        ' Try

        userSocket = newSocket '// Setup the socket for this class
        classID = newClassID '// Set the used socket ID for this class
        Dim socketIP As String = Me.ipAddress()
        Console.WriteLine("[SCKMGR] Established new connection with " & socketIP & " for socket [" & classID & "]")

        '// Check if this IP address is banned
        If HoloDB.checkExists("SELECT * FROM users_bans WHERE ipaddress = '" & socketIP & "'") = True Then '// This IP address appears to be banned!
            Dim banDetails() As String = HoloDB.runReadRow("SELECT date_expire,descr FROM users_bans WHERE ipaddress = '" & socketIP & "'")
            If DateTime.Compare(DateTime.Parse(banDetails(0)), DateTime.Now) > 0 Then '// Ban is still active
                handleBan(banDetails(1), "IP address [" & socketIP & "] was banned for reason [" & banDetails(1) & "]. Ban will be lifted at: " & banDetails(0)) '// Handle this ban
                Return '// Stop here
            Else
                HoloDB.runQuery("DELETE FROM user_bans WHERE ipaddress = '" & socketIP & "' LIMIT 1") '// Lift ban
            End If
        End If

        transData("@@") '// Send the 'connection accepted, send packets dear client' packet

        listenDataArrivals() '// Start listening for incoming data from this user

        pingManager = New Thread(AddressOf pingUser)
        pingManager.Start() '// Start the timeout checker thread

        'Catch
        'killConnection("Error at establishing new socket [" & classID & "]")

        'End Try
    End Sub
#End Region
#Region "Socket & packets management"
    Private Sub listenDataArrivals()
        Try
            userSocket.BeginReceive(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, New AsyncCallback(AddressOf dataArrived), 0) '// Set a pending data listener, who hits back to dataArrivalCallback when data is arrived =]
        Catch
            killConnection("Disconnected!")
        End Try
    End Sub
    Private Sub dataArrived(ByVal arrivalCallback As IAsyncResult)
        Dim bytesReceived, curPLength As Integer
        Dim dataPack As String
        Dim currentPacket As String

        Try
            bytesReceived = userSocket.EndReceive(arrivalCallback) '// Stop the callback and get the bytes received
        Catch
            killConnection("Disconnected!")
        End Try

        dataPack = Encoding.ASCII.GetString(dataBuffer, 0, bytesReceived) '// Convert the bytes to a string

        '// Filter exploitable chars, disconnect client if it contains an exploitable char
        If dataPack.Contains(Convert.ToChar(1)) = True Then killConnection("Exploitable characters in datastring.") : Return
        If dataPack.Contains(Convert.ToChar(2)) = True Then killConnection("Exploitable characters in datastring.") : Return
        If dataPack.Contains(Convert.ToChar(5)) = True Then killConnection("Exploitable characters in datastring.") : Return
        If dataPack.Contains(Convert.ToChar(9)) = True Then killConnection("Exploitable characters in datastring.") : Return

        '// Packets are sticked to each other in some cases, decoding the header of them will give us the lenghts of the packets inside
        While (dataPack.Length > 0)
            curPLength = HoloENCODING.decodeB64(dataPack.Substring(1, 2))
            currentPacket = dataPack.Substring(3, curPLength)
            If isLoggedIn = True Then '// User has succesfully logged in, allow execution of 'normal' packets
                handlePacket(currentPacket)
            Else '// Not logged in, only allow the login packets
                handleLoginPacket(currentPacket)
            End If
            dataPack = dataPack.Substring(curPLength + 3)
        End While

        timeOut = 0 '// Everything at packet timing is okay, packet received, update the timeOut byte so the user won't be seen as 'timed out'
        currentPacket = Nothing '// Release the space used for currentPacket string
        listenDataArrivals()
    End Sub
    Friend Sub transData(ByVal strData As String)
        Try
            Dim byteData() As Byte = Encoding.ASCII.GetBytes(strData & Convert.ToChar(1)) '// Add a chr[01] to finalize the packet and hash the bytes out of it
            userSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, New AsyncCallback(AddressOf transDataComplete), 0) '// Start async send action
            Console.WriteLine(DateTime.Now.ToString() & " >> " & strData)

        Catch '// Disconnected
            killConnection("Disconnected!")

        End Try
    End Sub
    Private Sub transDataComplete(ByVal transDataCompletedCallback As IAsyncResult)
        Try
            userSocket.EndSend(transDataCompletedCallback) '// Complete the data sending action

        Catch  '// Disconnected
            killConnection("Disconnected!")

        End Try
    End Sub
    Private Sub pingUser()
        While True
            Threading.Thread.Sleep(60000) '// Wait 60 seconds
            If timeOut = 2 Then '// If the user hasn't send a packet the last two minutes (so it's timeOut has been updated two times (60 seconds * 2 = 2 minutes) and the user hasn't send a packet)
                killConnection("Timeout, no packet received last 2 minutes.") '// Drop this connection
            Else
                timeOut += 1 '// User still active, just set it's timeOut to +1, it'll be set to 0 when new packet comes so don't worry
                transData("@r") '// Send a 'ping', forcing the client to reply something back
            End If
        End While
    End Sub
    Private Sub errorUser()
        Dim messageID As Integer = 0
        transData("DkI" & HoloENCODING.encodeVL64(messageID) & DateTime.Now.ToString & Convert.ToChar(2))
    End Sub
    Private Sub handleLoginPacket(ByVal currentPacket As String)
        Select Case currentPacket.Substring(0, 2) '// Determine header

            Case "CN" '// Encryption status
                transData("DUIH")

            Case "CJ" '// Formats for the client/RC4 encryption key (disabled)
                transData("DAQBHHIIKHJIPAHQAdd-MM-yyyySAHPBhttp://holographemulator.comQBH")

            Case "CL" '// Login - check ticket + bans, and get user details
                Dim ssoTicket As String = HoloDB.safeString(currentPacket.Substring(4)) '// Get the SSO login ticket from the packet

                Dim userID As Integer = HoloDB.runRead("SELECT id FROM users WHERE ticket_sso = '" & ssoTicket & "' AND ipaddress_last = '" & Me.ipAddress & "'", Nothing)
                If userID = 0 Then killConnection("User for SSO ticket [" & ssoTicket & "] and IP address [" & Me.ipAddress & "] not found!") : Return '// This user hasn't signed in via HoloCMS, there is no user with a pending SSO ticket like this, or the user who logged in via this user on HoloCMS doesn't have the same IP as the one actually using the ticket now, maybe SSO login bruter? Disconnect them and stop here

                If HoloDB.checkExists("SELECT * FROM users_bans WHERE userid = '" & userID & "'") = True Then '// Seems user is banned
                    Dim banDetails() As String = HoloDB.runReadRow("SELECT date_expire,descr FROM users_bans WHERE userid = '" & userID & "'")
                    If DateTime.Compare(DateTime.Parse(banDetails(0)), DateTime.Now) > 0 Then '// Ban is still active
                        handleBan(banDetails(1), "User [" & userID & "] was banned for reason [" & banDetails(1) & "]. Ban will be lifted at [" & banDetails(0) & "]")
                        Return
                    Else
                        HoloDB.runQuery("DELETE FROM users_bans WHERE userid = '" & userID & "' LIMIT 1") '// Lift ban
                    End If
                End If
                
                Dim oldSessionUser As clsHoloUSER = HoloMANAGERS.getUserClass(userID)
                If IsNothing(oldSessionUser) = False Then '// Someone already logged in on this account
                    oldSessionUser.killConnection("New instance of this user logged in.")
                    If HoloMANAGERS.hookedUsers.ContainsKey(userID) = True Then HoloMANAGERS.hookedUsers.Remove(userID) '// For some reason kill connection didn't removed the old user from the hooked users hashtable
                End If

                Me.UserID = userID
                Me.isLoggedIn = True
                userDetails = New clsHoloUSERDETAILS(Me)

                Dim userData() As String = HoloDB.runReadRow("SELECT name,figure,sex,mission,rank,consolemission FROM users WHERE id = '" & userID & "'") '// Get users details
                userDetails.UserID = userID
                userDetails.Name = userData(0)
                userDetails.Figure = userData(1)
                userDetails.Sex = Char.Parse(userData(2))
                userDetails.Mission = userData(3)
                userDetails.Rank = userData(4)
                userDetails.consoleMission = userData(5)

                HoloMANAGERS.hookedUsers.Add(userID, Me)
                transData("@B" & HoloRANK(userDetails.Rank).strFuse)
                transData("DbIH")
                transData("@C") '// Let client proceed with login

                '// Background shizzle
                HoloDB.runQuery("UPDATE users SET ticket_sso = NULL WHERE id = '" & userID & "' LIMIT 1") '// Null the users SSO ticket, since it has been used
                Console.WriteLine("[SCKMGR] User [" & userDetails.Name & "] logged in.")

            Case Else
                killConnection("Packet [" & currentPacket & "] not useable by non-logged in users.")

        End Select
    End Sub
    Private Sub handlePacket(ByVal currentPacket As String)
        'Try

        Console.WriteLine(DateTime.Now.ToString() & " << " & currentPacket.Replace(Convert.ToChar(13), "{13}"))
        Select Case currentPacket.Substring(0, 2) '// Determine header

            Case "@G" '// Send users appearance etc + other settings for client
                refreshAppearance(False)

                '// If welcome message enabled, then send it
                If Not (HoloRACK.welcMessage = vbNullString) Then transData("BK" & HoloRACK.welcMessage.Replace("%name%", userDetails.Name).Replace("%release%", My.Application.Info.Version.ToString))

            Case "@H" '// Send users valueables (credits, tickets blablah) and the welcom message)
                refreshValuables()

            Case "B]" '// Get user's badges
                refreshBadges()

            Case "@L" '// Process user's console
                Dim consolePack As New StringBuilder("@L" & userDetails.consoleMission & Convert.ToChar(2) & HoloENCODING.encodeVL64(200) & HoloENCODING.encodeVL64(200) & HoloENCODING.encodeVL64(600))
                Dim userIDs() As Integer = HoloMISC.getFriendIDs(UserID)

                consolePack.Append(HoloENCODING.encodeVL64(userIDs.Count))
                For i = 0 To userIDs.Count - 1
                    Dim friendDetails() As String = HoloDB.runReadRow("SELECT name,consolemission,figure,lastvisit FROM users WHERE id = '" & userIDs(i) & "'")
                    If friendDetails(1) = vbNullString Then friendDetails(1) = "H" Else friendDetails(1) = "I" & friendDetails(1) '// Fix consolemission display option
                    consolePack.Append(HoloENCODING.encodeVL64(userIDs(i)) & friendDetails(0) & Convert.ToChar(2) & friendDetails(1) & Convert.ToChar(2) & HoloMANAGERS.getUserHotelStatus(userIDs(i), friendDetails(3)) & Convert.ToChar(2) & friendDetails(2) & Convert.ToChar(2))
                Next
                transData(consolePack.ToString) '// Send init packet + friendslist

                userIDs = HoloDB.runReadColumn("SELECT friendid FROM messenger_messages WHERE userid = '" & UserID & "' ORDER BY messageid ASC", 0, Nothing)
                consolePack = New StringBuilder("Dy" & HoloENCODING.encodeVL64(userIDs.Count) & HoloENCODING.encodeVL64(userIDs.Count))
                If userIDs.Count > 0 Then
                    Dim messageIDs() As Integer = HoloDB.runReadColumn("SELECT messageid FROM messenger_messages WHERE userid = '" & UserID & "' ORDER BY messageid ASC", 0, Nothing)
                    Dim messageTimeStamps() As String = HoloDB.runReadColumn("SELECT sent_on FROM messenger_messages WHERE userid = '" & UserID & "' ORDER BY messageid ASC", 0)
                    Dim messageTexts() As String = HoloDB.runReadColumn("SELECT message FROM messenger_messages WHERE userid = '" & UserID & "' ORDER BY messageid ASC", 0)
                    For i = 0 To userIDs.Count - 1
                        consolePack.Append(HoloENCODING.encodeVL64(messageIDs(i)) & HoloENCODING.encodeVL64(userIDs(i)) & messageTimeStamps(i) & Convert.ToChar(2) & messageTexts(i) & Convert.ToChar(2))
                    Next
                End If
                transData(consolePack.ToString) '// Send messages

                userIDs = HoloDB.runReadColumn("SELECT userid_from FROM messenger_friendrequests WHERE userid_to = '" & UserID & "' ORDER BY requestid ASC", 0, Nothing)
                consolePack = New StringBuilder("Dz" & HoloENCODING.encodeVL64(userIDs.Count) & HoloENCODING.encodeVL64(userIDs.Count))
                If userIDs.Count > 0 Then
                    Dim requestIDs() As Integer = HoloDB.runReadColumn("SELECT requestid FROM messenger_friendrequests WHERE userid_to = '" & UserID & "' ORDER BY requestid ASC", 0, Nothing)
                    For i = 0 To userIDs.Count - 1
                        consolePack.Append(HoloENCODING.encodeVL64(requestIDs(i)) & HoloDB.runRead("SELECT name FROM users WHERE id = '" & userIDs(i) & "'") & Convert.ToChar(2) & userIDs(i) & Convert.ToChar(2))
                    Next
                End If
                transData(consolePack.ToString) '// Send friendrequests

            Case "@Z" '// Process user's Club subscription stats + badges
                refreshClub()

            Case "@O" '// Update the 'lastvisit' field for user's console
                Dim userIDs() As Integer = HoloMISC.getFriendIDs(UserID)
                Dim consolePack As New StringBuilder("@M" & HoloENCODING.encodeVL64(userIDs.Count))
                For i = 0 To userIDs.Count - 1
                    consolePack.Append(HoloENCODING.encodeVL64(userIDs(i)))
                    If HoloMANAGERS.hookedUsers.ContainsKey(userIDs(i)) = True Then
                        consolePack.Append(HoloMANAGERS.getUserDetails(userIDs(i)).consoleMission & Convert.ToChar(2) & HoloMANAGERS.getUserHotelPosition(userIDs(i)))
                    Else
                        Dim usersDetails() As String = HoloDB.runReadRow("SELECT consolemission,lastvisit FROM users WHERE id = '" & userIDs(i) & "'")
                        consolePack.Append(usersDetails(0) & Convert.ToChar(2) & "H" & usersDetails(1))
                    End If
                    consolePack.Append(Convert.ToChar(2))
                Next
                transData(consolePack.ToString)
                HoloDB.runQuery("UPDATE users SET lastvisit = '" & DateTime.Now.ToString & "' WHERE id = '" & UserID & "' LIMIT 1")

            Case "@d" '// Change user console mission
                Dim newMission As String = HoloMISC.filterWord(currentPacket.Substring(4).Trim)
                transData("BS" & newMission & Convert.ToChar(2))
                userDetails.consoleMission = newMission
                HoloDB.runQuery("UPDATE users SET consolemission = '" & HoloDB.safeString(newMission) & "' WHERE id = '" & UserID & "' LIMIT 1")

            Case "@i" '// User performs a user search at Console
                Dim searchInput As String = HoloDB.safeString(currentPacket.Substring(4, HoloENCODING.decodeB64(currentPacket.Substring(2, 2))))
                Dim searchResult As String() = HoloDB.runReadRow("SELECT id,name,figure,consolemission,lastvisit FROM users WHERE name = '" & searchInput & "'")

                If searchResult.Count > 0 Then '// User found!
                    transData("B@MESSENGER" & Convert.ToChar(2) & HoloENCODING.encodeVL64(searchResult(0)) & searchResult(1) & Convert.ToChar(2) & HoloMISC.consoleMissionFix(searchResult(3)) & Convert.ToChar(2) & HoloMANAGERS.getUserHotelPosition(searchResult(0)) & Convert.ToChar(2) & searchResult(4) & Convert.ToChar(2) & searchResult(2) & Convert.ToChar(2))
                Else
                    transData("B@MESSENGER" & Convert.ToChar(2) & "H")
                End If

            Case "@g" '// User requests someone as friend at Console
                Dim toID As Integer = HoloDB.runRead("SELECT id FROM users WHERE name = '" & HoloDB.safeString(currentPacket.Substring(4)) & "'", Nothing)
                If toID = 0 Then Return '// User not found
                If HoloDB.checkExists("SELECT requestid FROM messenger_friendrequests WHERE userid_to = '" & toID & "' AND userid_from = '" & UserID & "'") = True Then Return

                Dim requestID As Integer = HoloDB.runRead("SELECT MAX(requestid) FROM messenger_friendrequests WHERE userid_to = '" & toID & "'", Nothing)
                HoloDB.runQuery("INSERT INTO messenger_friendrequests(userid_to,userid_from,requestid) VALUES ('" & toID & "','" & UserID & "','" & requestID & "')")

                '// Try sending the requested user the notice
                Try
                    DirectCast(HoloMANAGERS.hookedUsers(toID), clsHoloUSER).transData("BDI" & userDetails.Name & Convert.ToChar(2) & UserID & Convert.ToChar(2))
                Catch
                End Try

            Case "@e" '// User accepts (a) friendrequest(s) on the Console
                Dim cntIDs As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                Dim rID, uID As Integer
                Dim myEntry As String = "BI" & HoloENCODING.encodeVL64(UserID) & userDetails.Name & Convert.ToChar(2) & HoloMISC.consoleMissionFix(userDetails.consoleMission) & Convert.ToChar(2) & HoloMANAGERS.getUserHotelPosition(UserID) & Convert.ToChar(2) & DateTime.Now.ToString & Convert.ToChar(2) & userDetails.Figure & Convert.ToChar(2)

                currentPacket = currentPacket.Substring(HoloENCODING.encodeVL64(cntIDs).Length + 2)
                For i = 0 To cntIDs - 1
                    If currentPacket.Length = 0 Then Return '// Scripter!111
                    rID = HoloENCODING.decodeVL64(currentPacket)
                    uID = HoloDB.runRead("SELECT userid_from FROM messenger_friendrequests WHERE userid_to = '" & UserID & "' AND requestid = '" & rID & "'", Nothing)
                    If uID = 0 Then Return '// Scriper!111 [This friendrequest doesn't exist]

                    HoloDB.runQuery("INSERT INTO messenger_friendships(userid,friendid) VALUES ('" & uID & "','" & UserID & "')")
                    HoloDB.runQuery("DELETE FROM messenger_friendrequests WHERE userid_to = '" & UserID & "' AND requestid = '" & rID & "' LIMIT 1")
                    If HoloMANAGERS.hookedUsers.ContainsKey(uID) = True Then
                        Dim R_USER As clsHoloUSERDETAILS = HoloMANAGERS.getUserDetails(uID)
                        transData("BI" & HoloENCODING.encodeVL64(uID) & R_USER.Name & Convert.ToChar(2) & HoloMISC.consoleMissionFix(R_USER.consoleMission) & Convert.ToChar(2) & HoloMANAGERS.getUserHotelPosition(uID) & Convert.ToChar(2) & DateTime.Now.ToString & Convert.ToChar(2) & R_USER.Figure & Convert.ToChar(2))
                        R_USER.userClass.transData(myEntry)
                    Else
                        Dim R_DETAILS() As String = HoloDB.runReadRow("SELECT name,consolemission,lastvisit,figure FROM users WHERE id = '" & uID & "'")
                        transData("BI" & HoloENCODING.encodeVL64(uID) & R_DETAILS(0) & Convert.ToChar(2) & HoloMISC.consoleMissionFix(R_DETAILS(1)) & Convert.ToChar(2) & "H" & Convert.ToChar(2) & R_DETAILS(2) & Convert.ToChar(2) & R_DETAILS(3) & Convert.ToChar(2))
                    End If

                    currentPacket = currentPacket.Substring(HoloENCODING.encodeVL64(rID).Length)
                Next
                transData("D{H")

            Case "@f" '// User declines (a) friendrequest(s) on the Console
                Dim cntIDs As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                Dim tmpID As Integer
                currentPacket = currentPacket.Substring(HoloENCODING.encodeVL64(cntIDs).Length + 2)
                For i = 0 To cntIDs - 1
                    If currentPacket.Length = 0 Then Return '// Scripter!111
                    tmpID = HoloENCODING.decodeVL64(currentPacket)
                    HoloDB.runRead("DELETE FROM messenger_friendrequests WHERE userid_to = '" & UserID & "' AND requestid = '" & tmpID & "' LIMIT 1")
                    currentPacket = currentPacket.Substring(HoloENCODING.encodeVL64(tmpID).Length)
                Next

            Case "@h" '// User deletes (a) friend(s) on the Console
                Dim delID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(3))
                transData("BJI" & HoloENCODING.encodeVL64(delID))

                If HoloDB.checkExists("SELECT userid FROM messenger_friendships WHERE (userid = '" & UserID & "' AND friendid = '" & delID & "') OR (userid = '" & UserID & "' AND friendid = '" & delID & "')") = True Then
                    If HoloMANAGERS.hookedUsers.ContainsKey(delID) = True Then HoloMANAGERS.getUserClass(delID).transData("BJI" & HoloENCODING.encodeVL64(UserID))

                    HoloDB.runQuery("DELETE FROM messenger_friendships WHERE (userid = '" & UserID & "' AND friendid = '" & delID & "') OR (userid = '" & delID & "' AND friendid = '" & UserID & "')")
                    HoloDB.runQuery("DELETE FROM messenger_messages WHERE (userid = '" & UserID & "' AND friendid = '" & delID & "') OR (userid = '" & delID & "' AND friendid = '" & UserID & "')")
                End If

            Case "@a" '// User sends (a) message(s) on the Console
                Dim cntIDs As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                Dim userIDs(100) As Integer

                currentPacket = currentPacket.Substring(HoloENCODING.encodeVL64(cntIDs).Length + 2)
                For i = 0 To cntIDs - 1
                    If currentPacket.Length = 0 Then Return '// Scripter!111
                    userIDs(i) = HoloENCODING.decodeVL64(currentPacket)
                    currentPacket = currentPacket.Substring(HoloENCODING.encodeVL64(userIDs(i)).Length)
                Next

                Dim messageText As String = HoloMISC.filterWord(currentPacket.Substring(2))
                Dim messageString As String = HoloDB.safeString(messageText)
                Dim messageTimeStamp As String = DateTime.Now.ToString()
                Dim messageID As Integer = 0

                For i = 0 To cntIDs - 1
                    If HoloDB.checkExists("SELECT userid FROM messenger_friendships WHERE (userid = '" & UserID & "' AND friendid = '" & userIDs(i) & "') OR (userid = '" & userIDs(i) & "' AND friendid = '" & UserID & "')") = False Then Continue For
                    messageID = HoloDB.runRead("SELECT MAX(messageid) FROM messenger_messages WHERE userid = '" & userIDs(i) & "'", Nothing) + 1
                    If HoloMANAGERS.hookedUsers.ContainsKey(userIDs(i)) = True Then HoloMANAGERS.getUserClass(userIDs(i)).transData("BF" & HoloENCODING.encodeVL64(messageID) & HoloENCODING.encodeVL64(Me.UserID) & messageTimeStamp & Convert.ToChar(2) & messageText & Convert.ToChar(2))
                    HoloDB.runQuery("INSERT INTO messenger_messages(userid,friendid,messageid,sent_on,message) VALUES ('" & userIDs(i) & "','" & Me.UserID & "','" & messageID & "','" & messageTimeStamp & "','" & messageString & "')")
                Next

            Case "@`" '// User deletes (a) message(s) on the Console
                Dim delID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                HoloDB.runQuery("DELETE FROM messenger_messages WHERE userid = '" & UserID & "' AND messageid = '" & delID & "' LIMIT 1")

            Case "DF" '// User 'stalks' a friend on the Console
                Dim targetID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                If HoloDB.checkExists("SELECT userid FROM messenger_friendships WHERE (userid = '" & UserID & "' AND friendid = '" & targetID & "') OR (userid = '" & targetID & "' AND friendid = '" & UserID & "')") = False Then transData("E]H") : Return

                If HoloMANAGERS.hookedUsers.ContainsKey(targetID) = True Then
                    Dim roomID As Integer = HoloMANAGERS.getUserDetails(targetID).roomID
                    If roomID > 0 Then transData("D^H" & HoloENCODING.encodeVL64(roomID)) Else transData("E]J")
                Else
                    transData("E]I")
                End If

            Case "BV" '// User performs something on the Navigator (browsing etc)
                'handleNavigatorAction()
                'If True Then Return
                '// RECOING TEH NAVIGATORR, I UNDERSTAND STRUCTURE NOW <3
                Dim catID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(3))
                Dim hideFullRooms As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2, 1))
                Dim categoryData() As String = HoloDB.runReadRow("SELECT name,parent,ispubcat FROM nav_categories WHERE id = '" & catID & "'")
                Dim Navigator As New StringBuilder("C\" & HoloENCODING.encodeVL64(hideFullRooms) & HoloENCODING.encodeVL64(catID) & HoloENCODING.encodeVL64(0) & categoryData(0) & Convert.ToChar(2) & HoloENCODING.encodeVL64(0) & HoloENCODING.encodeVL64(1000) & HoloENCODING.encodeVL64(catID))


                Dim hideFullHelper As String = vbNullString : If hideFullRooms = 1 Then hideFullHelper = "AND incnt_now < incnt_max "

                If categoryData(2) = "1" Then '// View publicroom category, get publicrooms
                    Dim roomIDs() As Integer = HoloDB.runReadColumn("SELECT id FROM publicrooms WHERE category_in = '" & catID & "' " & hideFullHelper & "ORDER BY id ASC", 0, Nothing)
                    If roomIDs.Count > 0 Then
                        Dim roomCCTs() As String = HoloDB.runReadColumn("SELECT name_cct FROM publicrooms WHERE category_in = '" & catID & "' " & hideFullHelper & "ORDER BY id ASC", 0)
                        Dim roomIcons() As String = HoloDB.runReadColumn("SELECT name_icon FROM publicrooms WHERE category_in = '" & catID & "' " & hideFullHelper & "ORDER BY id ASC", 0)
                        Dim roomCaptions() As String = HoloDB.runReadColumn("SELECT name_caption FROM publicrooms WHERE category_in = '" & catID & "' " & hideFullHelper & "ORDER BY id ASC", 0)
                        Dim roomNowInside() As Integer = HoloDB.runReadColumn("SELECT incnt_now FROM publicrooms WHERE category_in = '" & catID & "' " & hideFullHelper & "ORDER BY id ASC", 0, Nothing)
                        Dim roomMaxInside() As Integer = HoloDB.runReadColumn("SELECT incnt_max FROM publicrooms WHERE category_in = '" & catID & "' " & hideFullHelper & "ORDER BY id ASC", 0, Nothing)

                        For i = 0 To roomIDs.Count - 1
                            Navigator.Append(HoloENCODING.encodeVL64(roomIDs(i)) & "I" & roomCaptions(i) & Convert.ToChar(2) & HoloENCODING.encodeVL64(roomNowInside(i)) & HoloENCODING.encodeVL64(roomMaxInside(i)) & "K" & roomIcons(i) & Convert.ToChar(2) & HoloENCODING.encodeVL64(roomIDs(i)) & "H" & roomCCTs(i) & Convert.ToChar(2) & "HI")
                        Next
                    End If
                Else '// View guestroom category, get guestrooms

                End If

                '// Get categories inside this category
                Dim catIDs() As Integer = HoloDB.runReadColumn("SELECT id FROM nav_categories WHERE parent = '" & catID & "'", 0, Nothing)
                Dim catNames() As String = HoloDB.runReadColumn("SELECT name FROM nav_categories WHERE parent = '" & catID & "'", 0)

                For i = 0 To catIDs.Count - 1
                    Navigator.Append(HoloENCODING.encodeVL64(catIDs(i)) & "J" & catNames(i) & Convert.ToChar(2) & HoloENCODING.encodeVL64(0) & HoloENCODING.encodeVL64(1000) & HoloENCODING.encodeVL64(catID) & "H")
                Next

                transData(Navigator.ToString & Convert.ToChar(2))

            Case "BW" '// Guestroom category index for create/modify room
                Dim stagePack As New StringBuilder("C]")
                Dim stageIDs() As Integer = HoloDB.runReadColumn("SELECT id FROM nav_categories WHERE ispubcat = '0' AND parent > 0 AND minrank <= " & userDetails.Rank & " ORDER BY id ASC", 0, Nothing)
                Dim stageNames() As String = HoloDB.runReadColumn("SELECT name FROM nav_categories WHERE ispubcat = '0' AND parent > 0 AND minrank <= " & userDetails.Rank & " ORDER BY id ASC", 0)

                stagePack.Append(HoloENCODING.encodeVL64(stageIDs.Count))

                For i = 0 To stageIDs.Count - 1
                    stagePack.Append(HoloENCODING.encodeVL64(stageIDs(i)) & stageNames(i) & Convert.ToChar(2))
                Next

                transData(stagePack.ToString)

            Case "BZ" '// Hotel Navigator - Publicroom - who's in here?
                Try
                    transData("C_" & DirectCast(HoloMANAGERS.hookedRooms(HoloENCODING.decodeVL64(currentPacket.Substring(2))), clsHoloROOM).whosInHereList)
                Catch
                    transData("C_")
                End Try

            Case "DH" '// User refreshes recommended rooms section
                transData("E_" & HoloMANAGERS.getRecommendedRooms)

            Case "BA" '// User tries to redeem a Credit voucher
                Dim voucherCode As String = HoloDB.safeString(currentPacket.Substring(4))
                Dim voucherAmount As Integer = HoloDB.runRead("SELECT credits FROM vouchers WHERE voucher = '" & voucherCode & "'", Nothing)
                If voucherAmount > 0 Then
                    Dim newCredits As Integer = HoloDB.runRead("SELECT credits FROM users WHERE id = '" & UserID & "'", Nothing) + voucherAmount
                    transData("CT" & "@F" & newCredits & ".0")
                    HoloDB.runQuery("DELETE FROM vouchers WHERE voucher = '" & voucherCode & "' LIMIT 1")
                    HoloDB.runQuery("UPDATE users SET credits = '" & newCredits & "' WHERE id = '" & UserID & "' LIMIT 1")
                Else
                    transData("CU1")
                End If

            Case "@Q" '// User searches a guestroom
                Dim searchQuery As String = HoloDB.safeString(currentPacket.Substring(2))
                Dim roomIDs() As String = HoloDB.runReadColumn("SELECT id FROM guestrooms WHERE owner = '" & searchQuery & "' OR name LIKE '%" & searchQuery & "' ORDER BY id ASC", 30)

                If roomIDs.Count > 0 Then
                    Dim searchResult As New StringBuilder("@w")
                    For i = 0 To roomIDs.Count - 1
                        Dim roomData() As String = HoloDB.runReadRow("SELECT name,owner,descr,state,showname,incnt_now,incnt_max FROM guestrooms WHERE id = '" & roomIDs(i) & "'")
                        If roomData(4) = "0" Then If Not (roomData(1) = userDetails.Name) And HoloRANK(userDetails.Rank).containsRight("fuse_enter_locked_rooms") = False Then roomData(1) = "-"
                        searchResult.Append(roomIDs(i) & Convert.ToChar(9) & roomData(0) & Convert.ToChar(9) & roomData(1) & Convert.ToChar(9) & HoloMISC.getRoomState(roomData(3)) & Convert.ToChar(9) & "x" & Convert.ToChar(9) & roomData(5) & Convert.ToChar(9) & roomData(6) & Convert.ToChar(9) & "null" & Convert.ToChar(9) & roomData(2) & Convert.ToChar(9) & Convert.ToChar(13))
                    Next
                    transData(searchResult.ToString)
                Else
                    transData("@z")
                End If

            Case "@P" '// User views his/her own rooms
                Dim roomIDs() As String = HoloDB.runReadColumn("SELECT id FROM guestrooms WHERE owner = '" & userDetails.Name & "' ORDER BY id ASC", 0)
                If roomIDs.Count > 0 Then
                    Dim roomPack As New StringBuilder("@P")
                    For i = 0 To roomIDs.Count - 1
                        Dim roomData() As String = HoloDB.runReadRow("SELECT name,descr,state,showname,incnt_now,incnt_max FROM guestrooms WHERE id = '" & roomIDs(i) & "'")
                        roomPack.Append(roomIDs(i) & Convert.ToChar(9) & roomData(0) & Convert.ToChar(9) & userDetails.Name & Convert.ToChar(9) & HoloMISC.getRoomState(roomData(2)) & Convert.ToChar(9) & "x" & Convert.ToChar(9) & roomData(4) & Convert.ToChar(9) & roomData(5) & Convert.ToChar(9) & "null" & Convert.ToChar(9) & roomData(1) & Convert.ToChar(9) & Convert.ToChar(13))
                    Next

                    transData(roomPack.ToString)
                Else
                    transData("@y" & userDetails.Name)
                End If

            Case "@R" '// User initializes his/her favourite rooms
                Dim deletedFavCount As Integer = 0
                Dim roomPack As New StringBuilder
                Dim roomIDs() As Integer = HoloDB.runReadColumn("SELECT roomid FROM nav_favrooms WHERE userid = '" & UserID & "' ORDER BY ispublicroom DESC", 30, Nothing)
                If roomIDs.Count > 0 Then
                    Dim roomTypes() As Integer = HoloDB.runReadColumn("SELECT ispublicroom FROM nav_favrooms WHERE userid = '" & UserID & "' ORDER BY ispublicroom DESC", 30, Nothing)
                    For i = 0 To roomIDs.Count - 1
                        If roomTypes(i) = 1 Then '// This fav is a publicroom
                            Dim roomData() As String = HoloDB.runReadRow("SELECT name_cct,name_caption,name_icon,incnt_now,incnt_max FROM publicrooms WHERE id = '" & roomIDs(i) & "'")
                            If roomData.Count = 0 Then '// Non-existing room [staff removed publicroom]
                                deletedFavCount += 1
                                HoloDB.runQuery("DELETE FROM nav_favrooms WHERE userid = '" & UserID & "' AND roomid = '" & roomIDs(i) & "' AND ispublicroom = '1' LIMIT 1") '// Remove this favourite room from users list
                            Else '// Room exists, add it's details
                                roomPack.Append(HoloENCODING.encodeVL64(roomIDs(i)) & "I" & roomData(1) & Convert.ToChar(2) & HoloENCODING.encodeVL64(roomData(3)) & HoloENCODING.encodeVL64(roomData(4)) & "I" & roomData(2) & Convert.ToChar(2) & HoloENCODING.encodeVL64(roomIDs(i)) & "H" & roomData(0) & Convert.ToChar(2) & "IH")
                            End If
                        Else '// Guestroom fav! :o
                            Dim roomData() As String = HoloDB.runReadRow("SELECT name,owner,descr,state,showname,incnt_now,incnt_max FROM guestrooms WHERE id = '" & roomIDs(i) & "'")
                            If roomData.Count = 0 Then '// Non-existing room [owner deleted it or w/e]
                                deletedFavCount += 1
                                HoloDB.runQuery("DELETE FROM nav_favrooms WHERE userid = '" & UserID & "' AND roomid = '" & roomIDs(i) & "' AND ispublicroom = '0' LIMIT 1") '// Remove this favourite room from users list
                            Else '// Room exists, add it's details
                                roomPack.Append(HoloENCODING.encodeVL64(roomIDs(i)) & roomData(0) & Convert.ToChar(2) & roomData(1) & Convert.ToChar(2) & HoloMISC.getRoomState(roomData(3)) & Convert.ToChar(2) & HoloENCODING.encodeVL64(roomData(5)) & HoloENCODING.encodeVL64(roomData(6)) & roomData(2) & Convert.ToChar(2))
                            End If
                        End If
                    Next
                End If

                transData("@}HHJ" & Convert.ToChar(2) & "HHH" & HoloENCODING.encodeVL64(roomIDs.Count - deletedFavCount) & roomPack.ToString)

            Case "@S" '// User adds a room to his/her favourite rooms
                Dim isPub As Byte = HoloENCODING.decodeVL64(currentPacket.Substring(2, 1)) '// Check if the user adds a guestroom or a publicroom
                Dim roomID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(3)) '// Get the ID of the room the user wants to add

                If HoloDB.checkExists("SELECT userid FROM nav_favrooms WHERE userid = '" & UserID & "' AND roomid = '" & roomID & "' AND ispublicroom = '" & isPub & "'") = True Then Return '// Already added

                If isPub = 1 Then '// User adds publicroom
                    If HoloDB.checkExists("SELECT id FROM publicrooms WHERE id = '" & roomID & "'") = False Then Return '// This publicroom was not found, stop here
                Else '// User adds a guestroom
                    If HoloDB.checkExists("SELECT id FROM guestrooms WHERE id = '" & roomID & "'") = False Then Return '// This guestroom was not found, stop here
                End If

                If HoloDB.runRead("SELECT COUNT(*) FROM nav_favrooms WHERE userid = '" & UserID & "'", Nothing) >= 30 Then '// The user already has 30 favourite rooms! (or even more for some reason)
                    transData("@a" & "nav_error_toomanyfavrooms") '// Send the message that the users list is full (external_texts)
                Else
                    HoloDB.runQuery("INSERT INTO nav_favrooms(userid,roomid,ispublicroom) VALUES ('" & UserID & "','" & roomID & "','" & isPub & "')") '// Insert this favourite in the database
                End If

            Case "@T" '// User removes a room from his/her favourite rooms
                Dim isPub As Byte = HoloENCODING.decodeVL64(currentPacket.Substring(2, 1)) '// Check if the user deletes a guestroom or a publicroom
                Dim toDeleteID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(3)) '// Get the ID of the room the user wants to delete
                HoloDB.runQuery("DELETE FROM nav_favrooms WHERE userid = '" & UserID & "' AND roomid = '" & toDeleteID & "' AND ispublicroom = '" & isPub & "' LIMIT 1") '// Delete this favourite from the database

            Case "@]" '// Create guestroom - phase 1
                Dim roomSettings() As String = currentPacket.Split("/")
                If HoloDB.runRead("SELECT COUNT(id) FROM guestrooms WHERE owner = '" & userDetails.Name & "'", Nothing) < 15 Then '// User already has less than 15 rooms
                    roomSettings(2) = HoloMISC.filterWord(roomSettings(2)) '// Pass the roomname through the wordfilter
                    roomSettings(3) = HoloMISC.getRoomModelID(Char.Parse(roomSettings(3).Substring(6, 1)))
                    roomSettings(4) = HoloMISC.getRoomState(roomSettings(4), True) '// Get the ID of the state of the room for use with database

                    HoloDB.runQuery("INSERT INTO guestrooms (name,owner,model,state,showname) VALUES ('" & HoloDB.safeString(roomSettings(2)) & "','" & userDetails.Name & "','" & roomSettings(3) & "','" & roomSettings(4) & "','" & roomSettings(5) & "')")
                    Dim roomID As Integer = HoloDB.runRead("SELECT MAX(id) FROM guestrooms WHERE owner = '" & userDetails.Name & "'", Nothing) '// Get the ID of the latest created room [so, this one]
                    transData("@{" & roomID & Convert.ToChar(13) & roomSettings(2))
                Else
                    transData("@a" & "Error creating a private room") '// Alert! JEWS IN THE OVEN! [Oh Josh I always wanted to say that :)]
                End If

            Case "@Y" '// Modify guestroom / create guestroom - phase 2
                Dim roomID As Integer
                If currentPacket.Substring(2, 1) = "/" Then roomID = Integer.Parse(currentPacket.Split("/")(1)) Else roomID = Integer.Parse(currentPacket.Substring(2).Split("/")(0))
                Dim packetContent() As String = HoloDB.safeString(currentPacket).Split(Convert.ToChar(13))
                Dim roomDescription As String = vbNullString
                Dim superUsers As Byte = 0
                Dim maxUsers As Byte = 25
                Dim roomPassword As String = vbNullString

                For i = 1 To packetContent.Count - 1 '// More proper way, thx Jeax
                    Dim updHeader As String = packetContent(i).Split("=")(0)
                    Dim updValue As String = packetContent(i).Substring(updHeader.Length + 1)
                    Select Case updHeader
                        Case "description"
                            roomDescription = HoloMISC.filterWord(updValue)

                        Case "allsuperusers"
                            superUsers = Byte.Parse(updValue)

                        Case "maxvisitors"
                            maxUsers = Byte.Parse(updValue)
                            If maxUsers < 10 Or maxUsers > 25 Then maxUsers = 25

                        Case "password"
                            roomPassword = updValue
                    End Select
                Next
                HoloDB.runQuery("UPDATE guestrooms SET descr = '" & roomDescription & "',superusers = '" & superUsers & "',incnt_max = '" & maxUsers & "',opt_password = '" & roomPassword & "' WHERE id = '" & roomID & "' AND owner = '" & userDetails.Name & "' LIMIT 1") '// Just run update query, if the user is really the owner then stuff will get changed, if not, well then nothing happens :D

            Case "@U" '// Check guestroom in Navigator (send @v packet)
                Dim roomID As Integer = currentPacket.Substring(2)
                Dim roomData() As String = HoloDB.runReadRow("SELECT name,owner,descr,model,state,superusers,showname,category_in,incnt_now,incnt_max FROM guestrooms WHERE id = '" & roomID & "'")
                If Not (roomData.Count = 0) Then '// Guestroom exists
                    Dim allowTrading As Integer = HoloDB.runRead("SELECT trading FROM nav_categories WHERE id = '" & roomData(7) & "'", Nothing)
                    transData("@v" & HoloENCODING.encodeVL64(roomData(5)) & HoloENCODING.encodeVL64(roomData(4)) & HoloENCODING.encodeVL64(roomID) & roomData(1) & Convert.ToChar(2) & "model_" & HoloMISC.getRoomModelChar(Byte.Parse(roomData(3))) & Convert.ToChar(2) & roomData(0) & Convert.ToChar(2) & roomData(2) & Convert.ToChar(2) & HoloENCODING.encodeVL64(roomData(6)) & HoloENCODING.encodeVL64(allowTrading) & "H" & HoloENCODING.encodeVL64(roomData(8)) & HoloENCODING.encodeVL64(roomData(9)))
                End If

            Case "BX" '// Modify guestroom - click button, send category
                Dim roomID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                Dim roomCategory As String = HoloDB.runRead("SELECT category_in FROM guestrooms WHERE id = '" & roomID & "' AND owner = '" & userDetails.Name & "'")
                If roomCategory.Count > 0 Then transData("C^" & HoloENCODING.encodeVL64(roomID) & HoloENCODING.encodeVL64(roomCategory))

            Case "@X" '// '// Modify guestroom - save name, state and show/hide ownername
                Dim packetContent() As String = HoloDB.safeString(currentPacket.Substring(2)).Split("/")
                packetContent(1) = HoloDB.safeString(HoloMISC.filterWord(packetContent(1)))
                packetContent(2) = HoloMISC.getRoomState(packetContent(2), True)
                HoloDB.runQuery("UPDATE guestrooms SET name = '" & packetContent(1) & "',state = '" & packetContent(2) & "',showname = '" & Integer.Parse(packetContent(3)) & "' WHERE id = '" & packetContent(0) & "' AND owner = '" & userDetails.Name & "' LIMIT 1")

            Case "@W" '// Modify guestroom - delete room
                Dim roomID As Integer = currentPacket.Substring(2)
                If HoloDB.checkExists("SELECT id FROM guestrooms WHERE id = '" & roomID & "' AND owner = '" & userDetails.Name & "'") = True Then '// If there exists a room with this ID and the owner is this user [so this user owns this room]
                    HoloDB.runQuery("DELETE FROM guestrooms WHERE id = '" & roomID & "' LIMIT 1") '// Delete this room from database
                    HoloDB.runQuery("DELETE FROM guestroom_rights WHERE roomid = '" & roomID & "'") '// Delete all roomrights entries
                    HoloDB.runQuery("DELETE FROM guestroom_votes WHERE roomid = '" & roomID & "'") '// Delete all roomvotes entries
                    HoloDB.runQuery("DELETE FROM furniture WHERE roomid = '" & roomID & "'") '// Delete all the furniture in this room from database
                    HoloDB.runQuery("DELETE FROM furniture_moodlight WHERE roomid = '" & roomID & "' LIMIT 1") '// Delete the moodlight presets of the moodlight in this room [if any]

                    If HoloMANAGERS.hookedRooms.ContainsKey(roomID) = True Then '// There are people inside the room
                        Dim roomInstance As clsHoloROOM = HoloMANAGERS.hookedRooms(roomID)
                        roomInstance.Unload(True)
                    End If
                End If

            Case "B[" '// Modify guestroom - reset all rights in room
                Dim roomID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                If HoloDB.checkExists("SELECT id FROM guestrooms WHERE id = '" & roomID & "' AND owner = '" & userDetails.Name & "'") = True Then
                    HoloDB.runQuery("DELETE FROM guestroom_rights WHERE roomid = '" & roomID & "'")
                    If HoloMANAGERS.hookedRooms.ContainsKey(roomID) = True Then '// There are people inside the room
                        Dim roomInstance As clsHoloROOM = HoloMANAGERS.hookedRooms(roomID)
                        roomInstance.sendAll("BK" & HoloSTRINGS.getString("room_rightsreset"))
                        roomInstance.Unload(True)
                    End If
                End If

            Case "@u" '// Go to Hotel View (kick!)
                If IsNothing(roomCommunicator) = False Then roomCommunicator.removeUser(userDetails, False)

            Case "Bv" '// Enter room - loading screen advertisement
                Dim roomAdvertisement As String = "http://ads.habbohotel.co.uk/max/adview.php?zoneid=325&n=hhuk	http://ads.habbohotel.co.uk/max/adclick.php?n=hhuk"
                roomAdvertisement = vbNullString
                If roomAdvertisement = vbNullString Then
                    transData("DB" & "0")
                Else
                    transData("DB" & roomAdvertisement)
                End If

            Case "@B" '// Enter room - determine ID and cct name
                Dim isPub As Boolean = currentPacket.Substring(2, 1) = "A" '// Guestroom or publicroom?
                Dim roomID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(3)) '// Get room ID
                If IsNothing(roomCommunicator) = False Then roomCommunicator.removeUser(userDetails, False)

                transData("@S")
                transData("Bf" & "http://holographemulator.com/")
                If isPub = True Then transData("AE" & HoloDB.runRead("SELECT name_ae FROM publicrooms WHERE id = '" & roomID & "'") & " " & roomID)

                userDetails.roomID = roomID
                userDetails.inPublicroom = isPub
                If isPub = True Then userDetails.isAllowedInRoom = True

            Case "@y" '// Enter guestroom - determine state (closed, doorbell etc)end 
                If userDetails.inPublicroom = False Then
                    Dim roomData() As String = HoloDB.runReadRow("SELECT owner,state FROM guestrooms WHERE id = '" & userDetails.roomID & "'")
                    If roomData.Count = 0 Then transData("@R") : Return '// Not bound to entering room for some reason :O

                    If userDetails.Name = roomData(0) Or HoloRANK(userDetails.Rank).containsRight("fuse_any_room_controller") = True Then
                        userDetails.hasRights = True
                        userDetails.isOwner = True
                    Else
                        If roomData(1) = "1" Then '// Room with doorbell
                            If HoloMANAGERS.hookedRooms.ContainsKey(userDetails.roomID) = True Then DirectCast(HoloMANAGERS.hookedRooms(userDetails.roomID), clsHoloROOM).sendToRightHavingUsers("A[" & userDetails.Name)
                            transData("A[")
                            Return '// Wait...
                        ElseIf roomData(1) = "2" Then '// Room with password
                            Dim roomPassword As String = currentPacket.Split("/")(1)
                            If Not (roomPassword = HoloDB.runRead("SELECT opt_password FROM guestrooms WHERE id = '" & userDetails.roomID & "'")) Then transData("@a" & "Incorrect flat password") : Return '// Send wrong password notice and stop here
                        End If
                    End If
                End If
                userDetails.isAllowedInRoom = True '// Don't kick the user when he approaches the room, because he has passed doorbell, password, or the door is open
                transData("@i")

            Case "Ab" '// Guestroom - answer the doorbell
                If userDetails.hasRights = False Then Return '// User doesn't has the right to answer doorbells

                Dim ringingOne As String = currentPacket.Substring(4, HoloENCODING.decodeB64(currentPacket.Substring(2, 2)))
                Dim letIn As Boolean = currentPacket.Substring(currentPacket.Length - 1) = "A"

                Dim ringingUser As clsHoloUSERDETAILS = HoloMANAGERS.getUserDetails(ringingOne)
                If IsNothing(ringingUser) Then Return '// The doorbelling one has gone offline, stop here
                If Not (ringingUser.roomID = Me.userDetails.roomID) Then Return '// The 'inviter' isn't in the same room as the doorbeller, ohh scripters these days, seems SnG still works :(

                If letIn = True Then
                    ringingUser.isAllowedInRoom = True
                    ringingUser.userClass.transData("@i")
                Else
                    ringingUser.userClass.transData("BC")
                    ringingUser.Reset()
                End If

            Case "A~" '// Enter room - room advertisement (publicrooms, CP0 = no advertisement)
                Dim roomAdvertisement As String = vbNullString
                Dim roomAdvertisementURI As String = vbNullString
                If roomAdvertisement = vbNullString Then
                    transData("CP" & "0")
                Else
                    transData("CP" & roomAdvertisement & Convert.ToChar(9) & roomAdvertisementURI)
                End If

            Case "@|" '// Enter room - hook up with class, get heightmap + room AE incase it's a guestroom
                If IsNothing(roomCommunicator) = False Or userDetails.isAllowedInRoom = False Then Return
                If HoloMANAGERS.hookedRooms.ContainsKey(userDetails.roomID) = True Then
                    roomCommunicator = HoloMANAGERS.hookedRooms(userDetails.roomID)
                Else
                    Dim newRoom As New clsHoloROOM(userDetails.roomID, userDetails.inPublicroom) '// Setup new room!
                    HoloMANAGERS.hookedRooms.Add(userDetails.roomID, newRoom)
                    roomCommunicator = newRoom
                End If
                transData("@_" & roomCommunicator.Heightmap)

            Case "@{" '// Get guestroom model type + wallpaper etc
                If userDetails.roomID > 0 And userDetails.inPublicroom = False Then
                    transData("AE" & "model_" & HoloMISC.getRoomModelChar(HoloDB.runRead("SELECT model FROM guestrooms WHERE id = '" & userDetails.roomID & "'", Nothing)) & " " & userDetails.roomID)

                    Dim roomDecor() As Integer = HoloDB.runReadRow("SELECT wallpaper,floor FROM guestrooms WHERE id = '" & userDetails.roomID & "'", Nothing)
                    If roomDecor(0) > 0 Then transData("@nwallpaper/" & roomDecor(0)) '// If the room has wallpaper, send it
                    If roomDecor(1) Then transData("@nfloor/" & roomDecor(1)) '// If the room has a floor carpet, send it
                End If

            Case "@}" '// Enter room - get inside users
                If IsNothing(roomCommunicator) = True Or userDetails.isAllowedInRoom = False Then Return
                userDetails.isAllowedInRoom = True
                transData("@\" & roomCommunicator.insideUsers)
                roomCommunicator.addUser(userDetails)

            Case "@~" '// Enter room - get furni items, wallitems and other publicroom items
                If IsNothing(roomCommunicator) = True Then Return
                transData("@`" & roomCommunicator.Items)
                transData("@^" & roomCommunicator.otherItems)
                transData("@m" & roomCommunicator.wallItems)
                If userDetails.inPublicroom = False Then transData("DiH")
                If receivedItemIndex = False Then
                    transData("Dg[_Dshelves_norjaX~Dshelves_polyfonYmAshelves_siloXQHtable_polyfon_smallYmAchair_polyfonZbBtable_norja_medY_Itable_silo_medX~Dtable_plasto_4legY_Itable_plasto_roundY_Itable_plasto_bigsquareY_Istand_polyfon_zZbBchair_siloX~Dsofa_siloX~Dcouch_norjaX~Dchair_norjaX~Dtable_polyfon_medYmAdoormat_loveZbBdoormat_plainZ[Msofachair_polyfonX~Dsofa_polyfonZ[Msofachair_siloX~Dchair_plastyX~Dchair_plastoYmAtable_plasto_squareY_Ibed_polyfonX~Dbed_polyfon_one[dObed_trad_oneYmAbed_tradYmAbed_silo_oneYmAbed_silo_twoYmAtable_silo_smallX~Dbed_armas_twoYmAbed_budget_oneXQHbed_budgetXQHshelves_armasYmAbench_armasYmAtable_armasYmAsmall_table_armasZbBsmall_chair_armasYmAfireplace_armasYmAlamp_armasYmAbed_armas_oneYmAcarpet_standardY_Icarpet_armasYmAcarpet_polarY_Ifireplace_polyfonY_Itable_plasto_4leg*1Y_Itable_plasto_bigsquare*1Y_Itable_plasto_round*1Y_Itable_plasto_square*1Y_Ichair_plasto*1YmAcarpet_standard*1Y_Idoormat_plain*1Z[Mtable_plasto_4leg*2Y_Itable_plasto_bigsquare*2Y_Itable_plasto_round*2Y_Itable_plasto_square*2Y_Ichair_plasto*2YmAdoormat_plain*2Z[Mcarpet_standard*2Y_Itable_plasto_4leg*3Y_Itable_plasto_bigsquare*3Y_Itable_plasto_round*3Y_Itable_plasto_square*3Y_Ichair_plasto*3YmAcarpet_standard*3Y_Idoormat_plain*3Z[Mtable_plasto_4leg*4Y_Itable_plasto_bigsquare*4Y_Itable_plasto_round*4Y_Itable_plasto_square*4Y_Ichair_plasto*4YmAcarpet_standard*4Y_Idoormat_plain*4Z[Mdoormat_plain*6Z[Mdoormat_plain*5Z[Mcarpet_standard*5Y_Itable_plasto_4leg*5Y_Itable_plasto_bigsquare*5Y_Itable_plasto_round*5Y_Itable_plasto_square*5Y_Ichair_plasto*5YmAtable_plasto_4leg*6Y_Itable_plasto_bigsquare*6Y_Itable_plasto_round*6Y_Itable_plasto_square*6Y_Ichair_plasto*6YmAtable_plasto_4leg*7Y_Itable_plasto_bigsquare*7Y_Itable_plasto_round*7Y_Itable_plasto_square*7Y_Ichair_plasto*7YmAtable_plasto_4leg*8Y_Itable_plasto_bigsquare*8Y_Itable_plasto_round*8Y_Itable_plasto_square*8Y_Ichair_plasto*8YmAtable_plasto_4leg*9Y_Itable_plasto_bigsquare*9Y_Itable_plasto_round*9Y_Itable_plasto_square*9Y_Ichair_plasto*9YmAcarpet_standard*6Y_Ichair_plasty*1X~DpizzaYmAdrinksYmAchair_plasty*2X~Dchair_plasty*3X~Dchair_plasty*4X~Dbar_polyfonY_Iplant_cruddyYmAbottleYmAbardesk_polyfonX~Dbardeskcorner_polyfonX~DfloortileHbar_armasY_Ibartable_armasYmAbar_chair_armasYmAcarpet_softZ@Kcarpet_soft*1Z@Kcarpet_soft*2Z@Kcarpet_soft*3Z@Kcarpet_soft*4Z@Kcarpet_soft*5Z@Kcarpet_soft*6Z@Kred_tvY_Iwood_tvYmAcarpet_polar*1Y_Ichair_plasty*5X~Dcarpet_polar*2Y_Icarpet_polar*3Y_Icarpet_polar*4Y_Ichair_plasty*6X~Dtable_polyfonYmAsmooth_table_polyfonYmAsofachair_polyfon_girlX~Dbed_polyfon_girl_one[dObed_polyfon_girlX~Dsofa_polyfon_girlZ[Mbed_budgetb_oneXQHbed_budgetbXQHplant_pineappleYmAplant_fruittreeY_Iplant_small_cactusY_Iplant_bonsaiY_Iplant_big_cactusY_Iplant_yukkaY_Icarpet_standard*7Y_Icarpet_standard*8Y_Icarpet_standard*9Y_Icarpet_standard*aY_Icarpet_standard*bY_Iplant_sunflowerY_Iplant_roseY_Itv_luxusY_IbathZ\BsinkY_ItoiletYmAduckYmAtileYmAtoilet_redYmAtoilet_yellYmAtile_redYmAtile_yellYmApresent_gen[~Npresent_gen1[~Npresent_gen2[~Npresent_gen3[~Npresent_gen4[~Npresent_gen5[~Npresent_gen6[~Nbar_basicY_Ishelves_basicXQHsoft_sofachair_norjaX~Dsoft_sofa_norjaX~Dlamp_basicXQHlamp2_armasYmAfridgeY_Idoor[dOdoorB[dOdoorC[dOpumpkinYmAskullcandleYmAdeadduckYmAdeadduck2YmAdeadduck3YmAmenorahYmApuddingYmAhamYmAturkeyYmAxmasduckY_IhouseYmAtriplecandleYmAtree3YmAtree4YmAtree5X~Dham2YmAwcandlesetYmArcandlesetYmAstatueYmAheartY_IvaleduckYmAheartsofaX~DthroneYmAsamovarY_IgiftflowersY_IhabbocakeYmAhologramYmAeasterduckY_IbunnyYmAbasketY_IbirdieYmAediceX~Dclub_sofaZ[Mprize1YmAprize2YmAprize3YmAdivider_poly3X~Ddivider_arm1YmAdivider_arm2YmAdivider_arm3YmAdivider_nor1X~Ddivider_silo1X~Ddivider_nor2X~Ddivider_silo2Z[Mdivider_nor3X~Ddivider_silo3X~DtypingmachineYmAspyroYmAredhologramYmAcameraHjoulutahtiYmAhyacinth1YmAhyacinth2YmAchair_plasto*10YmAchair_plasto*11YmAbardeskcorner_polyfon*12X~Dbardeskcorner_polyfon*13X~Dchair_plasto*12YmAchair_plasto*13YmAchair_plasto*14YmAtable_plasto_4leg*14Y_ImocchamasterY_Icarpet_legocourtYmAbench_legoYmAlegotrophyYmAvalentinescreenYmAedicehcYmArare_daffodil_rugYmArare_beehive_bulbY_IhcsohvaYmAhcammeYmArare_elephant_statueYmArare_fountainY_Irare_standYmArare_globeYmArare_hammockYmArare_elephant_statue*1YmArare_elephant_statue*2YmArare_fountain*1Y_Irare_fountain*2Y_Irare_fountain*3Y_Irare_beehive_bulb*1Y_Irare_beehive_bulb*2Y_Irare_xmas_screenY_Irare_parasol*1Y_Irare_parasol*2Y_Irare_parasol*3Y_Itree1X~Dtree2ZmBwcandleYxBrcandleYxBsoft_jaggara_norjaYmAhouse2YmAdjesko_turntableYmAmd_sofaZ[Mmd_limukaappiY_Itable_plasto_4leg*10Y_Itable_plasto_4leg*15Y_Itable_plasto_bigsquare*14Y_Itable_plasto_bigsquare*15Y_Itable_plasto_round*14Y_Itable_plasto_round*15Y_Itable_plasto_square*14Y_Itable_plasto_square*15Y_Ichair_plasto*15YmAchair_plasty*7X~Dchair_plasty*8X~Dchair_plasty*9X~Dchair_plasty*10X~Dchair_plasty*11X~Dchair_plasto*16YmAtable_plasto_4leg*16Y_Ihockey_scoreY_Ihockey_lightYmAdoorD[dOprizetrophy2*3[rIprizetrophy3*3XrIprizetrophy4*3[rIprizetrophy5*3[rIprizetrophy6*3[rIprizetrophy*1Y_Iprizetrophy2*1[rIprizetrophy3*1XrIprizetrophy4*1[rIprizetrophy5*1[rIprizetrophy6*1[rIprizetrophy*2Y_Iprizetrophy2*2[rIprizetrophy3*2XrIprizetrophy4*2[rIprizetrophy5*2[rIprizetrophy6*2[rIprizetrophy*3Y_Irare_parasol*0Hhc_lmp[fBhc_tblYmAhc_chrYmAhc_dskXQHnestHpetfood1ZvCpetfood2ZvCpetfood3ZvCwaterbowl*4XICwaterbowl*5XICwaterbowl*2XICwaterbowl*1XICwaterbowl*3XICtoy1XICtoy1*1XICtoy1*2XICtoy1*3XICtoy1*4XICgoodie1ZvCgoodie1*1ZvCgoodie1*2ZvCgoodie2X~Dprizetrophy7*3[rIprizetrophy7*1[rIprizetrophy7*2[rIscifiport*0Y_Iscifiport*9Y_Iscifiport*8Y_Iscifiport*7Y_Iscifiport*6Y_Iscifiport*5Y_Iscifiport*4Y_Iscifiport*3Y_Iscifiport*2Y_Iscifiport*1Y_Iscifirocket*9Y_Iscifirocket*8Y_Iscifirocket*7Y_Iscifirocket*6Y_Iscifirocket*5Y_Iscifirocket*4Y_Iscifirocket*3Y_Iscifirocket*2Y_Iscifirocket*1Y_Iscifirocket*0Y_Iscifidoor*10Y_Iscifidoor*9Y_Iscifidoor*8Y_Iscifidoor*7Y_Iscifidoor*6Y_Iscifidoor*5Y_Iscifidoor*4Y_Iscifidoor*3Y_Iscifidoor*2Y_Iscifidoor*1Y_Ipillow*5YmApillow*8YmApillow*0YmApillow*1YmApillow*2YmApillow*7YmApillow*9YmApillow*4YmApillow*6YmApillow*3YmAmarquee*1Y_Imarquee*2Y_Imarquee*7Y_Imarquee*aY_Imarquee*8Y_Imarquee*9Y_Imarquee*5Y_Imarquee*4Y_Imarquee*6Y_Imarquee*3Y_Iwooden_screen*1Y_Iwooden_screen*2Y_Iwooden_screen*7Y_Iwooden_screen*0Y_Iwooden_screen*8Y_Iwooden_screen*5Y_Iwooden_screen*9Y_Iwooden_screen*4Y_Iwooden_screen*6Y_Iwooden_screen*3Y_Ipillar*6Y_Ipillar*1Y_Ipillar*9Y_Ipillar*0Y_Ipillar*8Y_Ipillar*2Y_Ipillar*5Y_Ipillar*4Y_Ipillar*7Y_Ipillar*3Y_Irare_dragonlamp*4Y_Irare_dragonlamp*0Y_Irare_dragonlamp*5Y_Irare_dragonlamp*2Y_Irare_dragonlamp*8Y_Irare_dragonlamp*9Y_Irare_dragonlamp*7Y_Irare_dragonlamp*6Y_Irare_dragonlamp*1Y_Irare_dragonlamp*3Y_Irare_icecream*1Y_Irare_icecream*7Y_Irare_icecream*8Y_Irare_icecream*2Y_Irare_icecream*6Y_Irare_icecream*9Y_Irare_icecream*3Y_Irare_icecream*0Y_Irare_icecream*4Y_Irare_icecream*5Y_Irare_fan*7YxBrare_fan*6YxBrare_fan*9YxBrare_fan*3YxBrare_fan*0YxBrare_fan*4YxBrare_fan*5YxBrare_fan*1YxBrare_fan*8YxBrare_fan*2YxBqueue_tile1*3X~Dqueue_tile1*6X~Dqueue_tile1*4X~Dqueue_tile1*9X~Dqueue_tile1*8X~Dqueue_tile1*5X~Dqueue_tile1*7X~Dqueue_tile1*2X~Dqueue_tile1*1X~Dqueue_tile1*0X~DticketHrare_snowrugX~Dcn_lampZxIcn_sofaYmAsporttrack1*1YmAsporttrack1*3YmAsporttrack1*2YmAsporttrack2*1[~Nsporttrack2*2[~Nsporttrack2*3[~Nsporttrack3*1YmAsporttrack3*2YmAsporttrack3*3YmAfootylampX~Dbarchair_siloX~Ddivider_nor4*4X~Dtraffic_light*1ZxItraffic_light*2ZxItraffic_light*3ZxItraffic_light*4ZxItraffic_light*6ZxIrubberchair*1X~Drubberchair*2X~Drubberchair*3X~Drubberchair*4X~Drubberchair*5X~Drubberchair*6X~Dbarrier*1X~Dbarrier*2X~Dbarrier*3X~Drubberchair*7X~Drubberchair*8X~Dtable_norja_med*2Y_Itable_norja_med*3Y_Itable_norja_med*4Y_Itable_norja_med*5Y_Itable_norja_med*6Y_Itable_norja_med*7Y_Itable_norja_med*8Y_Itable_norja_med*9Y_Icouch_norja*2X~Dcouch_norja*3X~Dcouch_norja*4X~Dcouch_norja*5X~Dcouch_norja*6X~Dcouch_norja*7X~Dcouch_norja*8X~Dcouch_norja*9X~Dshelves_norja*2X~Dshelves_norja*3X~Dshelves_norja*4X~Dshelves_norja*5X~Dshelves_norja*6X~Dshelves_norja*7X~Dshelves_norja*8X~Dshelves_norja*9X~Dchair_norja*2X~Dchair_norja*3X~Dchair_norja*4X~Dchair_norja*5X~Dchair_norja*6X~Dchair_norja*7X~Dchair_norja*8X~Dchair_norja*9X~Ddivider_nor1*2X~Ddivider_nor1*3X~Ddivider_nor1*4X~Ddivider_nor1*5X~Ddivider_nor1*6X~Ddivider_nor1*7X~Ddivider_nor1*8X~Ddivider_nor1*9X~Dsoft_sofa_norja*2X~Dsoft_sofa_norja*3X~Dsoft_sofa_norja*4X~Dsoft_sofa_norja*5X~Dsoft_sofa_norja*6X~Dsoft_sofa_norja*7X~Dsoft_sofa_norja*8X~Dsoft_sofa_norja*9X~Dsoft_sofachair_norja*2X~Dsoft_sofachair_norja*3X~Dsoft_sofachair_norja*4X~Dsoft_sofachair_norja*5X~Dsoft_sofachair_norja*6X~Dsoft_sofachair_norja*7X~Dsoft_sofachair_norja*8X~Dsoft_sofachair_norja*9X~Dsofachair_silo*2X~Dsofachair_silo*3X~Dsofachair_silo*4X~Dsofachair_silo*5X~Dsofachair_silo*6X~Dsofachair_silo*7X~Dsofachair_silo*8X~Dsofachair_silo*9X~Dtable_silo_small*2X~Dtable_silo_small*3X~Dtable_silo_small*4X~Dtable_silo_small*5X~Dtable_silo_small*6X~Dtable_silo_small*7X~Dtable_silo_small*8X~Dtable_silo_small*9X~Ddivider_silo1*2X~Ddivider_silo1*3X~Ddivider_silo1*4X~Ddivider_silo1*5X~Ddivider_silo1*6X~Ddivider_silo1*7X~Ddivider_silo1*8X~Ddivider_silo1*9X~Ddivider_silo3*2X~Ddivider_silo3*3X~Ddivider_silo3*4X~Ddivider_silo3*5X~Ddivider_silo3*6X~Ddivider_silo3*7X~Ddivider_silo3*8X~Ddivider_silo3*9X~Dtable_silo_med*2X~Dtable_silo_med*3X~Dtable_silo_med*4X~Dtable_silo_med*5X~Dtable_silo_med*6X~Dtable_silo_med*7X~Dtable_silo_med*8X~Dtable_silo_med*9X~Dsofa_silo*2X~Dsofa_silo*3X~Dsofa_silo*4X~Dsofa_silo*5X~Dsofa_silo*6X~Dsofa_silo*7X~Dsofa_silo*8X~Dsofa_silo*9X~Dsofachair_polyfon*2X~Dsofachair_polyfon*3X~Dsofachair_polyfon*4X~Dsofachair_polyfon*6X~Dsofachair_polyfon*7X~Dsofachair_polyfon*8X~Dsofachair_polyfon*9X~Dsofa_polyfon*2Z[Msofa_polyfon*3Z[Msofa_polyfon*4Z[Msofa_polyfon*6Z[Msofa_polyfon*7Z[Msofa_polyfon*8Z[Msofa_polyfon*9Z[Mbed_polyfon*2X~Dbed_polyfon*3X~Dbed_polyfon*4X~Dbed_polyfon*6X~Dbed_polyfon*7X~Dbed_polyfon*8X~Dbed_polyfon*9X~Dbed_polyfon_one*2[dObed_polyfon_one*3[dObed_polyfon_one*4[dObed_polyfon_one*6[dObed_polyfon_one*7[dObed_polyfon_one*8[dObed_polyfon_one*9[dObardesk_polyfon*2X~Dbardesk_polyfon*3X~Dbardesk_polyfon*4X~Dbardesk_polyfon*5X~Dbardesk_polyfon*6X~Dbardesk_polyfon*7X~Dbardesk_polyfon*8X~Dbardesk_polyfon*9X~Dbardeskcorner_polyfon*2X~Dbardeskcorner_polyfon*3X~Dbardeskcorner_polyfon*4X~Dbardeskcorner_polyfon*5X~Dbardeskcorner_polyfon*6X~Dbardeskcorner_polyfon*7X~Dbardeskcorner_polyfon*8X~Dbardeskcorner_polyfon*9X~Ddivider_poly3*2X~Ddivider_poly3*3X~Ddivider_poly3*4X~Ddivider_poly3*5X~Ddivider_poly3*6X~Ddivider_poly3*7X~Ddivider_poly3*8X~Ddivider_poly3*9X~Dchair_silo*2X~Dchair_silo*3X~Dchair_silo*4X~Dchair_silo*5X~Dchair_silo*6X~Dchair_silo*7X~Dchair_silo*8X~Dchair_silo*9X~Ddivider_nor3*2X~Ddivider_nor3*3X~Ddivider_nor3*4X~Ddivider_nor3*5X~Ddivider_nor3*6X~Ddivider_nor3*7X~Ddivider_nor3*8X~Ddivider_nor3*9X~Ddivider_nor2*2X~Ddivider_nor2*3X~Ddivider_nor2*4X~Ddivider_nor2*5X~Ddivider_nor2*6X~Ddivider_nor2*7X~Ddivider_nor2*8X~Ddivider_nor2*9X~Dsilo_studydeskX~Dsolarium_norjaY_Isolarium_norja*1Y_Isolarium_norja*2Y_Isolarium_norja*3Y_Isolarium_norja*5Y_Isolarium_norja*6Y_Isolarium_norja*7Y_Isolarium_norja*8Y_Isolarium_norja*9Y_IsandrugX~Drare_moonrugYmAchair_chinaYmAchina_tableYmAsleepingbag*1YmAsleepingbag*2YmAsleepingbag*3YmAsleepingbag*4YmAsafe_siloY_Isleepingbag*7YmAsleepingbag*9YmAsleepingbag*5YmAsleepingbag*10YmAsleepingbag*6YmAsleepingbag*8YmAchina_shelveX~Dtraffic_light*5ZxIdivider_nor4*2X~Ddivider_nor4*3X~Ddivider_nor4*5X~Ddivider_nor4*6X~Ddivider_nor4*7X~Ddivider_nor4*8X~Ddivider_nor4*9X~Ddivider_nor5*2X~Ddivider_nor5*3X~Ddivider_nor5*4X~Ddivider_nor5*5X~Ddivider_nor5*6X~Ddivider_nor5*7X~Ddivider_nor5*8X~Ddivider_nor5*9X~Ddivider_nor5X~Ddivider_nor4X~Dwall_chinaYmAcorner_chinaYmAbarchair_silo*2X~Dbarchair_silo*3X~Dbarchair_silo*4X~Dbarchair_silo*5X~Dbarchair_silo*6X~Dbarchair_silo*7X~Dbarchair_silo*8X~Dbarchair_silo*9X~Dsafe_silo*2Y_Isafe_silo*3Y_Isafe_silo*4Y_Isafe_silo*5Y_Isafe_silo*6Y_Isafe_silo*7Y_Isafe_silo*8Y_Isafe_silo*9Y_Iglass_shelfY_Iglass_chairY_Iglass_stoolY_Iglass_sofaY_Iglass_tableY_Iglass_table*2Y_Iglass_table*3Y_Iglass_table*4Y_Iglass_table*5Y_Iglass_table*6Y_Iglass_table*7Y_Iglass_table*8Y_Iglass_table*9Y_Iglass_chair*2Y_Iglass_chair*3Y_Iglass_chair*4Y_Iglass_chair*5Y_Iglass_chair*6Y_Iglass_chair*7Y_Iglass_chair*8Y_Iglass_chair*9Y_Iglass_sofa*2Y_Iglass_sofa*3Y_Iglass_sofa*4Y_Iglass_sofa*5Y_Iglass_sofa*6Y_Iglass_sofa*7Y_Iglass_sofa*8Y_Iglass_sofa*9Y_Iglass_stool*2Y_Iglass_stool*4Y_Iglass_stool*5Y_Iglass_stool*6Y_Iglass_stool*7Y_Iglass_stool*8Y_Iglass_stool*3Y_Iglass_stool*9Y_ICFC_100_coin_goldZvCCFC_10_coin_bronzeZvCCFC_200_moneybagZvCCFC_500_goldbarZvCCFC_50_coin_silverZvCCF_10_coin_goldZvCCF_1_coin_bronzeZvCCF_20_moneybagZvCCF_50_goldbarZvCCF_5_coin_silverZvChc_crptYmAhc_tvZ\BgothgateX~DgothiccandelabraYxBgothrailingX~Dgoth_tableYmAhc_bkshlfYmAhc_btlrY_Ihc_crtnYmAhc_djsetYmAhc_frplcZbBhc_lmpstYmAhc_machineYmAhc_rllrXQHhc_rntgnX~Dhc_trllYmAgothic_chair*1X~Dgothic_sofa*1X~Dgothic_stool*1X~Dgothic_chair*2X~Dgothic_sofa*2X~Dgothic_stool*2X~Dgothic_chair*3X~Dgothic_sofa*3X~Dgothic_stool*3X~Dgothic_chair*4X~Dgothic_sofa*4X~Dgothic_stool*4X~Dgothic_chair*5X~Dgothic_sofa*5X~Dgothic_stool*5X~Dgothic_chair*6X~Dgothic_sofa*6X~Dgothic_stool*6X~Dval_cauldronX~Dsound_machineX~Dromantique_pianochair*3Y_Iromantique_pianochair*5Y_Iromantique_pianochair*2Y_Iromantique_pianochair*4Y_Iromantique_pianochair*1Y_Iromantique_divan*3Y_Iromantique_divan*5Y_Iromantique_divan*2Y_Iromantique_divan*4Y_Iromantique_divan*1Y_Iromantique_chair*3Y_Iromantique_chair*5Y_Iromantique_chair*2Y_Iromantique_chair*4Y_Iromantique_chair*1Y_Irare_parasolY_Iplant_valentinerose*3XICplant_valentinerose*5XICplant_valentinerose*2XICplant_valentinerose*4XICplant_valentinerose*1XICplant_mazegateYeCplant_mazeZcCplant_bulrushXICpetfood4Y_Icarpet_valentineZ|Egothic_carpetXICgothic_carpet2Z|Egothic_chairX~Dgothic_sofaX~Dgothic_stoolX~Dgrand_piano*3Z|Egrand_piano*5Z|Egrand_piano*2Z|Egrand_piano*4Z|Egrand_piano*1Z|Etheatre_seatZ@Kromantique_tray2Y_Iromantique_tray1Y_Iromantique_smalltabl*3Y_Iromantique_smalltabl*5Y_Iromantique_smalltabl*2Y_Iromantique_smalltabl*4Y_Iromantique_smalltabl*1Y_Iromantique_mirrortablY_Iromantique_divider*3Z[Mromantique_divider*2Z[Mromantique_divider*4Z[Mromantique_divider*1Z[Mjp_tatami2YGGjp_tatamiYGGhabbowood_chairYGGjp_bambooYGGjp_iroriXQHjp_pillowYGGsound_set_1Y_Isound_set_2Y_Isound_set_3Y_Isound_set_4Y_Isound_set_5Z@Ksound_set_6Y_Isound_set_7Y_Isound_set_8Y_Isound_set_9Y_Isound_machine*1ZIPspotlightY_Isound_machine*2ZIPsound_machine*3ZIPsound_machine*4ZIPsound_machine*5ZIPsound_machine*6ZIPsound_machine*7ZIProm_lampZ|Erclr_sofaXQHrclr_gardenXQHrclr_chairZ|Esound_set_28Y_Isound_set_27Y_Isound_set_26Y_Isound_set_25Y_Isound_set_24Y_Isound_set_23Y_Isound_set_22Y_Isound_set_21Y_Isound_set_20Z@Ksound_set_19Z@Ksound_set_18Y_Isound_set_17Y_Isound_set_16Y_Isound_set_15Y_Isound_set_14Y_Isound_set_13Y_Isound_set_12Y_Isound_set_11Y_Isound_set_10Y_Irope_dividerXQHromantique_clockY_Irare_icecream_campaignY_Ipura_mdl5*1XQHpura_mdl5*2XQHpura_mdl5*3XQHpura_mdl5*4XQHpura_mdl5*5XQHpura_mdl5*6XQHpura_mdl5*7XQHpura_mdl5*8XQHpura_mdl5*9XQHpura_mdl4*1XQHpura_mdl4*2XQHpura_mdl4*3XQHpura_mdl4*4XQHpura_mdl4*5XQHpura_mdl4*6XQHpura_mdl4*7XQHpura_mdl4*8XQHpura_mdl4*9XQHpura_mdl3*1XQHpura_mdl3*2XQHpura_mdl3*3XQHpura_mdl3*4XQHpura_mdl3*5XQHpura_mdl3*6XQHpura_mdl3*7XQHpura_mdl3*8XQHpura_mdl3*9XQHpura_mdl2*1XQHpura_mdl2*2XQHpura_mdl2*3XQHpura_mdl2*4XQHpura_mdl2*5XQHpura_mdl2*6XQHpura_mdl2*7XQHpura_mdl2*8XQHpura_mdl2*9XQHpura_mdl1*1XQHpura_mdl1*2XQHpura_mdl1*3XQHpura_mdl1*4XQHpura_mdl1*5XQHpura_mdl1*6XQHpura_mdl1*7XQHpura_mdl1*8XQHpura_mdl1*9XQHjp_lanternXQHchair_basic*1XQHchair_basic*2XQHchair_basic*3XQHchair_basic*4XQHchair_basic*5XQHchair_basic*6XQHchair_basic*7XQHchair_basic*8XQHchair_basic*9XQHbed_budget*1XQHbed_budget*2XQHbed_budget*3XQHbed_budget*4XQHbed_budget*5XQHbed_budget*6XQHbed_budget*7XQHbed_budget*8XQHbed_budget*9XQHbed_budget_one*1XQHbed_budget_one*2XQHbed_budget_one*3XQHbed_budget_one*4XQHbed_budget_one*5XQHbed_budget_one*6XQHbed_budget_one*7XQHbed_budget_one*8XQHbed_budget_one*9XQHjp_drawerXQHtile_stellaZ[Mtile_marbleZ[Mtile_brownZ[Msummer_grill*1Y_Isummer_grill*2Y_Isummer_grill*3Y_Isummer_grill*4Y_Isummer_chair*1Y_Isummer_chair*2Y_Isummer_chair*3Y_Isummer_chair*4Y_Isummer_chair*5Y_Isummer_chair*6Y_Isummer_chair*7Y_Isummer_chair*8Y_Isummer_chair*9Y_Isound_set_36ZfIsound_set_35ZfIsound_set_34ZfIsound_set_33ZfIsound_set_32Y_Isound_set_31Y_Isound_set_30Y_Isound_set_29Y_Isound_machine_pro[~Nrare_mnstrY_Ione_way_door*1XQHone_way_door*2XQHone_way_door*3XQHone_way_door*4XQHone_way_door*5XQHone_way_door*6XQHone_way_door*7XQHone_way_door*8XQHone_way_door*9XQHexe_rugZ[Mexe_s_tableZGRsound_set_37ZfIsummer_pool*1ZlIsummer_pool*2ZlIsummer_pool*3ZlIsummer_pool*4ZlIsong_diskY_Ijukebox*1[~Ncarpet_soft_tut[~Nsound_set_44Z@Ksound_set_43Z@Ksound_set_42Z@Ksound_set_41Z@Ksound_set_40Z@Ksound_set_39Z@Ksound_set_38Z@Kgrunge_chairZ@Kgrunge_mattressZ@Kgrunge_radiatorZ@Kgrunge_shelfZ@Kgrunge_signZ@Kgrunge_tableZ@Khabboween_crypt[uKhabboween_grassZ@Khal_cauldronZ@Khal_graveZ@Ksound_set_52ZuKsound_set_51ZuKsound_set_50ZuKsound_set_49ZuKsound_set_48ZuKsound_set_47ZuKsound_set_46ZuKsound_set_45ZuKxmas_icelampZ[Mxmas_cstl_wallZ[Mxmas_cstl_twrZ[Mxmas_cstl_gate[~Ntree7Z[Mtree6Z[Msound_set_54Z[Msound_set_53Z[Msafe_silo_pb[dOplant_mazegate_snowZ[Mplant_maze_snowZ[Mchristmas_sleighZ[Mchristmas_reindeer[~Nchristmas_poopZ[Mexe_bardeskZ[Mexe_chairZ[Mexe_chair2Z[Mexe_cornerZ[Mexe_drinksZ[Mexe_sofaZ[Mexe_tableZ[Msound_set_59[~Nsound_set_58[~Nsound_set_57[~Nsound_set_56[~Nsound_set_55[~Nnoob_table*1[~Nnoob_table*2[~Nnoob_table*3[~Nnoob_table*4[~Nnoob_table*5[~Nnoob_table*6[~Nnoob_stool*1[~Nnoob_stool*2[~Nnoob_stool*3[~Nnoob_stool*4[~Nnoob_stool*5[~Nnoob_stool*6[~Nnoob_rug*1[~Nnoob_rug*2[~Nnoob_rug*3[~Nnoob_rug*4[~Nnoob_rug*5[~Nnoob_rug*6[~Nnoob_lamp*1[dOnoob_lamp*2[dOnoob_lamp*3[dOnoob_lamp*4[dOnoob_lamp*5[dOnoob_lamp*6[dOnoob_chair*1[~Nnoob_chair*2[~Nnoob_chair*3[~Nnoob_chair*4[~Nnoob_chair*5[~Nnoob_chair*6[~Nexe_globe[~Nexe_plantZ[Mval_teddy*1[dOval_teddy*2[dOval_teddy*3[dOval_teddy*4[dOval_teddy*5[dOval_teddy*6[dOval_randomizer[dOval_choco[dOteleport_door[dOsound_set_61[dOsound_set_60[dOfortune[dOsw_tableZIPsw_raven[cQsw_chestZIPsand_cstl_wallZIPsand_cstl_twrZIPsand_cstl_gateZIPgrunge_candleZIPgrunge_benchZIPgrunge_barrelZIPrclr_lampZGRprizetrophy9*1ZGRprizetrophy8*1ZGRnouvelle_traxYcPmd_rugZGRjp_tray6ZGRjp_tray5ZGRjp_tray4ZGRjp_tray3ZGRjp_tray2ZGRjp_tray1ZGRarabian_teamkZGRarabian_snakeZGRarabian_rugZGRarabian_pllwZGRarabian_divdrZGRarabian_chairZGRarabian_bigtbZGRarabian_tetblZGRarabian_tray1ZGRarabian_tray2ZGRarabian_tray3ZGRarabian_tray4ZGRPIpost.itHpost.it.vdHphotoHChessHTicTacToeHBattleShipHPokerHwallpaperHfloorHposterZ@KgothicfountainYxBhc_wall_lampZbBindustrialfanZ`BtorchZ\Bval_heartXBCwallmirrorZ|Ejp_ninjastarsXQHhabw_mirrorXQHhabbowheelZ[Mguitar_skullZ@Kguitar_vZ@Kxmas_light[~Nhrella_poster_3[Nhrella_poster_2ZIPhrella_poster_1[Nsw_swordsZIPsw_stoneZIPsw_holeZIProomdimmerZGRmd_logo_wallZGRmd_canZGRjp_sheet3ZGRjp_sheet2ZGRjp_sheet1ZGRarabian_swordsZGRarabian_wndwZGR")
                    receivedItemIndex = True
                End If

            Case "A@" '// Enter room - get rights + refresh users
                If IsNothing(roomCommunicator) = True Then Return
                If userDetails.inPublicroom = False Then '// Only hand out rights when in guestroom
                    If userDetails.hasRights = False Then userDetails.hasRights = HoloDB.checkExists("SELECT userid FROM guestroom_rights WHERE userid = '" & UserID & "' AND roomid = '" & userDetails.roomID & "'")
                    If userDetails.isOwner = True Then userDetails.addStatus("flatctrl", "useradmin") : transData("@o")
                    If userDetails.hasRights = True Then
                        If userDetails.containsStatus("flatctrl") = False Then userDetails.addStatus("flatctrl", "onlyfurniture")
                        transData("@j")
                    End If
                End If
                transData("@D" & HoloENCODING.encodeVL64(99)) '// Some camera pictures :D
                transData("@b" & roomCommunicator.insideUsersDynamics)

                'Case "bbShizzle" '// ._.
                '   userDetails.inBBLobby = False
                '  resetGameStatuses()

                ' If userDetails.inPublicroom = True And userDetails.roomID = 17 Then
                'If IsNothing(HoloBBGAMELOBBY) Then HoloBBGAMELOBBY = New clsHoloBBGAMELOBBY(roomCommunicator)
                'userDetails.inBBLobby = True
                'transData("Cg" & HoloENCODING.encodeVL64(0) & "BattleBall leet" & Convert.ToChar(2) & HoloENCODING.encodeVL64(1000000) )
                'End If

            Case "AO" '// User looks to something (rotate body + head)
                If userDetails.containsStatus("sit") = True Then Return
                Dim ToX As Integer = currentPacket.Substring(2).Split(" ")(0)
                Dim ToY As Integer = currentPacket.Split(" ")(1)
                With userDetails
                    If .PosY > ToY Then .rotHead = 0
                    If .PosX < ToX Then .rotHead = 2
                    If .PosY < ToY Then .rotHead = 4
                    If .PosX > ToX Then .rotHead = 6
                    If .PosX < ToX And .PosY > ToY Then .rotHead = 1
                    If .PosX < ToX And .PosY < ToY Then .rotHead = 3
                    If .PosX > ToX And .PosY < ToY Then .rotHead = 5
                    If .PosX > ToX And .PosY > ToY Then .rotHead = 7
                    .rotBody = .rotHead
                End With
                roomCommunicator.refreshUser(userDetails)

            Case "CW" '// Sprite index
                If receivedItemIndex = False Then
                    transData("Dg[_Dshelves_norjaX~Dshelves_polyfonYmAshelves_siloXQHtable_polyfon_smallYmAchair_polyfonZbBtable_norja_medY_Itable_silo_medX~Dtable_plasto_4legY_Itable_plasto_roundY_Itable_plasto_bigsquareY_Istand_polyfon_zZbBchair_siloX~Dsofa_siloX~Dcouch_norjaX~Dchair_norjaX~Dtable_polyfon_medYmAdoormat_loveZbBdoormat_plainZ[Msofachair_polyfonX~Dsofa_polyfonZ[Msofachair_siloX~Dchair_plastyX~Dchair_plastoYmAtable_plasto_squareY_Ibed_polyfonX~Dbed_polyfon_one[dObed_trad_oneYmAbed_tradYmAbed_silo_oneYmAbed_silo_twoYmAtable_silo_smallX~Dbed_armas_twoYmAbed_budget_oneXQHbed_budgetXQHshelves_armasYmAbench_armasYmAtable_armasYmAsmall_table_armasZbBsmall_chair_armasYmAfireplace_armasYmAlamp_armasYmAbed_armas_oneYmAcarpet_standardY_Icarpet_armasYmAcarpet_polarY_Ifireplace_polyfonY_Itable_plasto_4leg*1Y_Itable_plasto_bigsquare*1Y_Itable_plasto_round*1Y_Itable_plasto_square*1Y_Ichair_plasto*1YmAcarpet_standard*1Y_Idoormat_plain*1Z[Mtable_plasto_4leg*2Y_Itable_plasto_bigsquare*2Y_Itable_plasto_round*2Y_Itable_plasto_square*2Y_Ichair_plasto*2YmAdoormat_plain*2Z[Mcarpet_standard*2Y_Itable_plasto_4leg*3Y_Itable_plasto_bigsquare*3Y_Itable_plasto_round*3Y_Itable_plasto_square*3Y_Ichair_plasto*3YmAcarpet_standard*3Y_Idoormat_plain*3Z[Mtable_plasto_4leg*4Y_Itable_plasto_bigsquare*4Y_Itable_plasto_round*4Y_Itable_plasto_square*4Y_Ichair_plasto*4YmAcarpet_standard*4Y_Idoormat_plain*4Z[Mdoormat_plain*6Z[Mdoormat_plain*5Z[Mcarpet_standard*5Y_Itable_plasto_4leg*5Y_Itable_plasto_bigsquare*5Y_Itable_plasto_round*5Y_Itable_plasto_square*5Y_Ichair_plasto*5YmAtable_plasto_4leg*6Y_Itable_plasto_bigsquare*6Y_Itable_plasto_round*6Y_Itable_plasto_square*6Y_Ichair_plasto*6YmAtable_plasto_4leg*7Y_Itable_plasto_bigsquare*7Y_Itable_plasto_round*7Y_Itable_plasto_square*7Y_Ichair_plasto*7YmAtable_plasto_4leg*8Y_Itable_plasto_bigsquare*8Y_Itable_plasto_round*8Y_Itable_plasto_square*8Y_Ichair_plasto*8YmAtable_plasto_4leg*9Y_Itable_plasto_bigsquare*9Y_Itable_plasto_round*9Y_Itable_plasto_square*9Y_Ichair_plasto*9YmAcarpet_standard*6Y_Ichair_plasty*1X~DpizzaYmAdrinksYmAchair_plasty*2X~Dchair_plasty*3X~Dchair_plasty*4X~Dbar_polyfonY_Iplant_cruddyYmAbottleYmAbardesk_polyfonX~Dbardeskcorner_polyfonX~DfloortileHbar_armasY_Ibartable_armasYmAbar_chair_armasYmAcarpet_softZ@Kcarpet_soft*1Z@Kcarpet_soft*2Z@Kcarpet_soft*3Z@Kcarpet_soft*4Z@Kcarpet_soft*5Z@Kcarpet_soft*6Z@Kred_tvY_Iwood_tvYmAcarpet_polar*1Y_Ichair_plasty*5X~Dcarpet_polar*2Y_Icarpet_polar*3Y_Icarpet_polar*4Y_Ichair_plasty*6X~Dtable_polyfonYmAsmooth_table_polyfonYmAsofachair_polyfon_girlX~Dbed_polyfon_girl_one[dObed_polyfon_girlX~Dsofa_polyfon_girlZ[Mbed_budgetb_oneXQHbed_budgetbXQHplant_pineappleYmAplant_fruittreeY_Iplant_small_cactusY_Iplant_bonsaiY_Iplant_big_cactusY_Iplant_yukkaY_Icarpet_standard*7Y_Icarpet_standard*8Y_Icarpet_standard*9Y_Icarpet_standard*aY_Icarpet_standard*bY_Iplant_sunflowerY_Iplant_roseY_Itv_luxusY_IbathZ\BsinkY_ItoiletYmAduckYmAtileYmAtoilet_redYmAtoilet_yellYmAtile_redYmAtile_yellYmApresent_gen[~Npresent_gen1[~Npresent_gen2[~Npresent_gen3[~Npresent_gen4[~Npresent_gen5[~Npresent_gen6[~Nbar_basicY_Ishelves_basicXQHsoft_sofachair_norjaX~Dsoft_sofa_norjaX~Dlamp_basicXQHlamp2_armasYmAfridgeY_Idoor[dOdoorB[dOdoorC[dOpumpkinYmAskullcandleYmAdeadduckYmAdeadduck2YmAdeadduck3YmAmenorahYmApuddingYmAhamYmAturkeyYmAxmasduckY_IhouseYmAtriplecandleYmAtree3YmAtree4YmAtree5X~Dham2YmAwcandlesetYmArcandlesetYmAstatueYmAheartY_IvaleduckYmAheartsofaX~DthroneYmAsamovarY_IgiftflowersY_IhabbocakeYmAhologramYmAeasterduckY_IbunnyYmAbasketY_IbirdieYmAediceX~Dclub_sofaZ[Mprize1YmAprize2YmAprize3YmAdivider_poly3X~Ddivider_arm1YmAdivider_arm2YmAdivider_arm3YmAdivider_nor1X~Ddivider_silo1X~Ddivider_nor2X~Ddivider_silo2Z[Mdivider_nor3X~Ddivider_silo3X~DtypingmachineYmAspyroYmAredhologramYmAcameraHjoulutahtiYmAhyacinth1YmAhyacinth2YmAchair_plasto*10YmAchair_plasto*11YmAbardeskcorner_polyfon*12X~Dbardeskcorner_polyfon*13X~Dchair_plasto*12YmAchair_plasto*13YmAchair_plasto*14YmAtable_plasto_4leg*14Y_ImocchamasterY_Icarpet_legocourtYmAbench_legoYmAlegotrophyYmAvalentinescreenYmAedicehcYmArare_daffodil_rugYmArare_beehive_bulbY_IhcsohvaYmAhcammeYmArare_elephant_statueYmArare_fountainY_Irare_standYmArare_globeYmArare_hammockYmArare_elephant_statue*1YmArare_elephant_statue*2YmArare_fountain*1Y_Irare_fountain*2Y_Irare_fountain*3Y_Irare_beehive_bulb*1Y_Irare_beehive_bulb*2Y_Irare_xmas_screenY_Irare_parasol*1Y_Irare_parasol*2Y_Irare_parasol*3Y_Itree1X~Dtree2ZmBwcandleYxBrcandleYxBsoft_jaggara_norjaYmAhouse2YmAdjesko_turntableYmAmd_sofaZ[Mmd_limukaappiY_Itable_plasto_4leg*10Y_Itable_plasto_4leg*15Y_Itable_plasto_bigsquare*14Y_Itable_plasto_bigsquare*15Y_Itable_plasto_round*14Y_Itable_plasto_round*15Y_Itable_plasto_square*14Y_Itable_plasto_square*15Y_Ichair_plasto*15YmAchair_plasty*7X~Dchair_plasty*8X~Dchair_plasty*9X~Dchair_plasty*10X~Dchair_plasty*11X~Dchair_plasto*16YmAtable_plasto_4leg*16Y_Ihockey_scoreY_Ihockey_lightYmAdoorD[dOprizetrophy2*3[rIprizetrophy3*3XrIprizetrophy4*3[rIprizetrophy5*3[rIprizetrophy6*3[rIprizetrophy*1Y_Iprizetrophy2*1[rIprizetrophy3*1XrIprizetrophy4*1[rIprizetrophy5*1[rIprizetrophy6*1[rIprizetrophy*2Y_Iprizetrophy2*2[rIprizetrophy3*2XrIprizetrophy4*2[rIprizetrophy5*2[rIprizetrophy6*2[rIprizetrophy*3Y_Irare_parasol*0Hhc_lmp[fBhc_tblYmAhc_chrYmAhc_dskXQHnestHpetfood1ZvCpetfood2ZvCpetfood3ZvCwaterbowl*4XICwaterbowl*5XICwaterbowl*2XICwaterbowl*1XICwaterbowl*3XICtoy1XICtoy1*1XICtoy1*2XICtoy1*3XICtoy1*4XICgoodie1ZvCgoodie1*1ZvCgoodie1*2ZvCgoodie2X~Dprizetrophy7*3[rIprizetrophy7*1[rIprizetrophy7*2[rIscifiport*0Y_Iscifiport*9Y_Iscifiport*8Y_Iscifiport*7Y_Iscifiport*6Y_Iscifiport*5Y_Iscifiport*4Y_Iscifiport*3Y_Iscifiport*2Y_Iscifiport*1Y_Iscifirocket*9Y_Iscifirocket*8Y_Iscifirocket*7Y_Iscifirocket*6Y_Iscifirocket*5Y_Iscifirocket*4Y_Iscifirocket*3Y_Iscifirocket*2Y_Iscifirocket*1Y_Iscifirocket*0Y_Iscifidoor*10Y_Iscifidoor*9Y_Iscifidoor*8Y_Iscifidoor*7Y_Iscifidoor*6Y_Iscifidoor*5Y_Iscifidoor*4Y_Iscifidoor*3Y_Iscifidoor*2Y_Iscifidoor*1Y_Ipillow*5YmApillow*8YmApillow*0YmApillow*1YmApillow*2YmApillow*7YmApillow*9YmApillow*4YmApillow*6YmApillow*3YmAmarquee*1Y_Imarquee*2Y_Imarquee*7Y_Imarquee*aY_Imarquee*8Y_Imarquee*9Y_Imarquee*5Y_Imarquee*4Y_Imarquee*6Y_Imarquee*3Y_Iwooden_screen*1Y_Iwooden_screen*2Y_Iwooden_screen*7Y_Iwooden_screen*0Y_Iwooden_screen*8Y_Iwooden_screen*5Y_Iwooden_screen*9Y_Iwooden_screen*4Y_Iwooden_screen*6Y_Iwooden_screen*3Y_Ipillar*6Y_Ipillar*1Y_Ipillar*9Y_Ipillar*0Y_Ipillar*8Y_Ipillar*2Y_Ipillar*5Y_Ipillar*4Y_Ipillar*7Y_Ipillar*3Y_Irare_dragonlamp*4Y_Irare_dragonlamp*0Y_Irare_dragonlamp*5Y_Irare_dragonlamp*2Y_Irare_dragonlamp*8Y_Irare_dragonlamp*9Y_Irare_dragonlamp*7Y_Irare_dragonlamp*6Y_Irare_dragonlamp*1Y_Irare_dragonlamp*3Y_Irare_icecream*1Y_Irare_icecream*7Y_Irare_icecream*8Y_Irare_icecream*2Y_Irare_icecream*6Y_Irare_icecream*9Y_Irare_icecream*3Y_Irare_icecream*0Y_Irare_icecream*4Y_Irare_icecream*5Y_Irare_fan*7YxBrare_fan*6YxBrare_fan*9YxBrare_fan*3YxBrare_fan*0YxBrare_fan*4YxBrare_fan*5YxBrare_fan*1YxBrare_fan*8YxBrare_fan*2YxBqueue_tile1*3X~Dqueue_tile1*6X~Dqueue_tile1*4X~Dqueue_tile1*9X~Dqueue_tile1*8X~Dqueue_tile1*5X~Dqueue_tile1*7X~Dqueue_tile1*2X~Dqueue_tile1*1X~Dqueue_tile1*0X~DticketHrare_snowrugX~Dcn_lampZxIcn_sofaYmAsporttrack1*1YmAsporttrack1*3YmAsporttrack1*2YmAsporttrack2*1[~Nsporttrack2*2[~Nsporttrack2*3[~Nsporttrack3*1YmAsporttrack3*2YmAsporttrack3*3YmAfootylampX~Dbarchair_siloX~Ddivider_nor4*4X~Dtraffic_light*1ZxItraffic_light*2ZxItraffic_light*3ZxItraffic_light*4ZxItraffic_light*6ZxIrubberchair*1X~Drubberchair*2X~Drubberchair*3X~Drubberchair*4X~Drubberchair*5X~Drubberchair*6X~Dbarrier*1X~Dbarrier*2X~Dbarrier*3X~Drubberchair*7X~Drubberchair*8X~Dtable_norja_med*2Y_Itable_norja_med*3Y_Itable_norja_med*4Y_Itable_norja_med*5Y_Itable_norja_med*6Y_Itable_norja_med*7Y_Itable_norja_med*8Y_Itable_norja_med*9Y_Icouch_norja*2X~Dcouch_norja*3X~Dcouch_norja*4X~Dcouch_norja*5X~Dcouch_norja*6X~Dcouch_norja*7X~Dcouch_norja*8X~Dcouch_norja*9X~Dshelves_norja*2X~Dshelves_norja*3X~Dshelves_norja*4X~Dshelves_norja*5X~Dshelves_norja*6X~Dshelves_norja*7X~Dshelves_norja*8X~Dshelves_norja*9X~Dchair_norja*2X~Dchair_norja*3X~Dchair_norja*4X~Dchair_norja*5X~Dchair_norja*6X~Dchair_norja*7X~Dchair_norja*8X~Dchair_norja*9X~Ddivider_nor1*2X~Ddivider_nor1*3X~Ddivider_nor1*4X~Ddivider_nor1*5X~Ddivider_nor1*6X~Ddivider_nor1*7X~Ddivider_nor1*8X~Ddivider_nor1*9X~Dsoft_sofa_norja*2X~Dsoft_sofa_norja*3X~Dsoft_sofa_norja*4X~Dsoft_sofa_norja*5X~Dsoft_sofa_norja*6X~Dsoft_sofa_norja*7X~Dsoft_sofa_norja*8X~Dsoft_sofa_norja*9X~Dsoft_sofachair_norja*2X~Dsoft_sofachair_norja*3X~Dsoft_sofachair_norja*4X~Dsoft_sofachair_norja*5X~Dsoft_sofachair_norja*6X~Dsoft_sofachair_norja*7X~Dsoft_sofachair_norja*8X~Dsoft_sofachair_norja*9X~Dsofachair_silo*2X~Dsofachair_silo*3X~Dsofachair_silo*4X~Dsofachair_silo*5X~Dsofachair_silo*6X~Dsofachair_silo*7X~Dsofachair_silo*8X~Dsofachair_silo*9X~Dtable_silo_small*2X~Dtable_silo_small*3X~Dtable_silo_small*4X~Dtable_silo_small*5X~Dtable_silo_small*6X~Dtable_silo_small*7X~Dtable_silo_small*8X~Dtable_silo_small*9X~Ddivider_silo1*2X~Ddivider_silo1*3X~Ddivider_silo1*4X~Ddivider_silo1*5X~Ddivider_silo1*6X~Ddivider_silo1*7X~Ddivider_silo1*8X~Ddivider_silo1*9X~Ddivider_silo3*2X~Ddivider_silo3*3X~Ddivider_silo3*4X~Ddivider_silo3*5X~Ddivider_silo3*6X~Ddivider_silo3*7X~Ddivider_silo3*8X~Ddivider_silo3*9X~Dtable_silo_med*2X~Dtable_silo_med*3X~Dtable_silo_med*4X~Dtable_silo_med*5X~Dtable_silo_med*6X~Dtable_silo_med*7X~Dtable_silo_med*8X~Dtable_silo_med*9X~Dsofa_silo*2X~Dsofa_silo*3X~Dsofa_silo*4X~Dsofa_silo*5X~Dsofa_silo*6X~Dsofa_silo*7X~Dsofa_silo*8X~Dsofa_silo*9X~Dsofachair_polyfon*2X~Dsofachair_polyfon*3X~Dsofachair_polyfon*4X~Dsofachair_polyfon*6X~Dsofachair_polyfon*7X~Dsofachair_polyfon*8X~Dsofachair_polyfon*9X~Dsofa_polyfon*2Z[Msofa_polyfon*3Z[Msofa_polyfon*4Z[Msofa_polyfon*6Z[Msofa_polyfon*7Z[Msofa_polyfon*8Z[Msofa_polyfon*9Z[Mbed_polyfon*2X~Dbed_polyfon*3X~Dbed_polyfon*4X~Dbed_polyfon*6X~Dbed_polyfon*7X~Dbed_polyfon*8X~Dbed_polyfon*9X~Dbed_polyfon_one*2[dObed_polyfon_one*3[dObed_polyfon_one*4[dObed_polyfon_one*6[dObed_polyfon_one*7[dObed_polyfon_one*8[dObed_polyfon_one*9[dObardesk_polyfon*2X~Dbardesk_polyfon*3X~Dbardesk_polyfon*4X~Dbardesk_polyfon*5X~Dbardesk_polyfon*6X~Dbardesk_polyfon*7X~Dbardesk_polyfon*8X~Dbardesk_polyfon*9X~Dbardeskcorner_polyfon*2X~Dbardeskcorner_polyfon*3X~Dbardeskcorner_polyfon*4X~Dbardeskcorner_polyfon*5X~Dbardeskcorner_polyfon*6X~Dbardeskcorner_polyfon*7X~Dbardeskcorner_polyfon*8X~Dbardeskcorner_polyfon*9X~Ddivider_poly3*2X~Ddivider_poly3*3X~Ddivider_poly3*4X~Ddivider_poly3*5X~Ddivider_poly3*6X~Ddivider_poly3*7X~Ddivider_poly3*8X~Ddivider_poly3*9X~Dchair_silo*2X~Dchair_silo*3X~Dchair_silo*4X~Dchair_silo*5X~Dchair_silo*6X~Dchair_silo*7X~Dchair_silo*8X~Dchair_silo*9X~Ddivider_nor3*2X~Ddivider_nor3*3X~Ddivider_nor3*4X~Ddivider_nor3*5X~Ddivider_nor3*6X~Ddivider_nor3*7X~Ddivider_nor3*8X~Ddivider_nor3*9X~Ddivider_nor2*2X~Ddivider_nor2*3X~Ddivider_nor2*4X~Ddivider_nor2*5X~Ddivider_nor2*6X~Ddivider_nor2*7X~Ddivider_nor2*8X~Ddivider_nor2*9X~Dsilo_studydeskX~Dsolarium_norjaY_Isolarium_norja*1Y_Isolarium_norja*2Y_Isolarium_norja*3Y_Isolarium_norja*5Y_Isolarium_norja*6Y_Isolarium_norja*7Y_Isolarium_norja*8Y_Isolarium_norja*9Y_IsandrugX~Drare_moonrugYmAchair_chinaYmAchina_tableYmAsleepingbag*1YmAsleepingbag*2YmAsleepingbag*3YmAsleepingbag*4YmAsafe_siloY_Isleepingbag*7YmAsleepingbag*9YmAsleepingbag*5YmAsleepingbag*10YmAsleepingbag*6YmAsleepingbag*8YmAchina_shelveX~Dtraffic_light*5ZxIdivider_nor4*2X~Ddivider_nor4*3X~Ddivider_nor4*5X~Ddivider_nor4*6X~Ddivider_nor4*7X~Ddivider_nor4*8X~Ddivider_nor4*9X~Ddivider_nor5*2X~Ddivider_nor5*3X~Ddivider_nor5*4X~Ddivider_nor5*5X~Ddivider_nor5*6X~Ddivider_nor5*7X~Ddivider_nor5*8X~Ddivider_nor5*9X~Ddivider_nor5X~Ddivider_nor4X~Dwall_chinaYmAcorner_chinaYmAbarchair_silo*2X~Dbarchair_silo*3X~Dbarchair_silo*4X~Dbarchair_silo*5X~Dbarchair_silo*6X~Dbarchair_silo*7X~Dbarchair_silo*8X~Dbarchair_silo*9X~Dsafe_silo*2Y_Isafe_silo*3Y_Isafe_silo*4Y_Isafe_silo*5Y_Isafe_silo*6Y_Isafe_silo*7Y_Isafe_silo*8Y_Isafe_silo*9Y_Iglass_shelfY_Iglass_chairY_Iglass_stoolY_Iglass_sofaY_Iglass_tableY_Iglass_table*2Y_Iglass_table*3Y_Iglass_table*4Y_Iglass_table*5Y_Iglass_table*6Y_Iglass_table*7Y_Iglass_table*8Y_Iglass_table*9Y_Iglass_chair*2Y_Iglass_chair*3Y_Iglass_chair*4Y_Iglass_chair*5Y_Iglass_chair*6Y_Iglass_chair*7Y_Iglass_chair*8Y_Iglass_chair*9Y_Iglass_sofa*2Y_Iglass_sofa*3Y_Iglass_sofa*4Y_Iglass_sofa*5Y_Iglass_sofa*6Y_Iglass_sofa*7Y_Iglass_sofa*8Y_Iglass_sofa*9Y_Iglass_stool*2Y_Iglass_stool*4Y_Iglass_stool*5Y_Iglass_stool*6Y_Iglass_stool*7Y_Iglass_stool*8Y_Iglass_stool*3Y_Iglass_stool*9Y_ICFC_100_coin_goldZvCCFC_10_coin_bronzeZvCCFC_200_moneybagZvCCFC_500_goldbarZvCCFC_50_coin_silverZvCCF_10_coin_goldZvCCF_1_coin_bronzeZvCCF_20_moneybagZvCCF_50_goldbarZvCCF_5_coin_silverZvChc_crptYmAhc_tvZ\BgothgateX~DgothiccandelabraYxBgothrailingX~Dgoth_tableYmAhc_bkshlfYmAhc_btlrY_Ihc_crtnYmAhc_djsetYmAhc_frplcZbBhc_lmpstYmAhc_machineYmAhc_rllrXQHhc_rntgnX~Dhc_trllYmAgothic_chair*1X~Dgothic_sofa*1X~Dgothic_stool*1X~Dgothic_chair*2X~Dgothic_sofa*2X~Dgothic_stool*2X~Dgothic_chair*3X~Dgothic_sofa*3X~Dgothic_stool*3X~Dgothic_chair*4X~Dgothic_sofa*4X~Dgothic_stool*4X~Dgothic_chair*5X~Dgothic_sofa*5X~Dgothic_stool*5X~Dgothic_chair*6X~Dgothic_sofa*6X~Dgothic_stool*6X~Dval_cauldronX~Dsound_machineX~Dromantique_pianochair*3Y_Iromantique_pianochair*5Y_Iromantique_pianochair*2Y_Iromantique_pianochair*4Y_Iromantique_pianochair*1Y_Iromantique_divan*3Y_Iromantique_divan*5Y_Iromantique_divan*2Y_Iromantique_divan*4Y_Iromantique_divan*1Y_Iromantique_chair*3Y_Iromantique_chair*5Y_Iromantique_chair*2Y_Iromantique_chair*4Y_Iromantique_chair*1Y_Irare_parasolY_Iplant_valentinerose*3XICplant_valentinerose*5XICplant_valentinerose*2XICplant_valentinerose*4XICplant_valentinerose*1XICplant_mazegateYeCplant_mazeZcCplant_bulrushXICpetfood4Y_Icarpet_valentineZ|Egothic_carpetXICgothic_carpet2Z|Egothic_chairX~Dgothic_sofaX~Dgothic_stoolX~Dgrand_piano*3Z|Egrand_piano*5Z|Egrand_piano*2Z|Egrand_piano*4Z|Egrand_piano*1Z|Etheatre_seatZ@Kromantique_tray2Y_Iromantique_tray1Y_Iromantique_smalltabl*3Y_Iromantique_smalltabl*5Y_Iromantique_smalltabl*2Y_Iromantique_smalltabl*4Y_Iromantique_smalltabl*1Y_Iromantique_mirrortablY_Iromantique_divider*3Z[Mromantique_divider*2Z[Mromantique_divider*4Z[Mromantique_divider*1Z[Mjp_tatami2YGGjp_tatamiYGGhabbowood_chairYGGjp_bambooYGGjp_iroriXQHjp_pillowYGGsound_set_1Y_Isound_set_2Y_Isound_set_3Y_Isound_set_4Y_Isound_set_5Z@Ksound_set_6Y_Isound_set_7Y_Isound_set_8Y_Isound_set_9Y_Isound_machine*1ZIPspotlightY_Isound_machine*2ZIPsound_machine*3ZIPsound_machine*4ZIPsound_machine*5ZIPsound_machine*6ZIPsound_machine*7ZIProm_lampZ|Erclr_sofaXQHrclr_gardenXQHrclr_chairZ|Esound_set_28Y_Isound_set_27Y_Isound_set_26Y_Isound_set_25Y_Isound_set_24Y_Isound_set_23Y_Isound_set_22Y_Isound_set_21Y_Isound_set_20Z@Ksound_set_19Z@Ksound_set_18Y_Isound_set_17Y_Isound_set_16Y_Isound_set_15Y_Isound_set_14Y_Isound_set_13Y_Isound_set_12Y_Isound_set_11Y_Isound_set_10Y_Irope_dividerXQHromantique_clockY_Irare_icecream_campaignY_Ipura_mdl5*1XQHpura_mdl5*2XQHpura_mdl5*3XQHpura_mdl5*4XQHpura_mdl5*5XQHpura_mdl5*6XQHpura_mdl5*7XQHpura_mdl5*8XQHpura_mdl5*9XQHpura_mdl4*1XQHpura_mdl4*2XQHpura_mdl4*3XQHpura_mdl4*4XQHpura_mdl4*5XQHpura_mdl4*6XQHpura_mdl4*7XQHpura_mdl4*8XQHpura_mdl4*9XQHpura_mdl3*1XQHpura_mdl3*2XQHpura_mdl3*3XQHpura_mdl3*4XQHpura_mdl3*5XQHpura_mdl3*6XQHpura_mdl3*7XQHpura_mdl3*8XQHpura_mdl3*9XQHpura_mdl2*1XQHpura_mdl2*2XQHpura_mdl2*3XQHpura_mdl2*4XQHpura_mdl2*5XQHpura_mdl2*6XQHpura_mdl2*7XQHpura_mdl2*8XQHpura_mdl2*9XQHpura_mdl1*1XQHpura_mdl1*2XQHpura_mdl1*3XQHpura_mdl1*4XQHpura_mdl1*5XQHpura_mdl1*6XQHpura_mdl1*7XQHpura_mdl1*8XQHpura_mdl1*9XQHjp_lanternXQHchair_basic*1XQHchair_basic*2XQHchair_basic*3XQHchair_basic*4XQHchair_basic*5XQHchair_basic*6XQHchair_basic*7XQHchair_basic*8XQHchair_basic*9XQHbed_budget*1XQHbed_budget*2XQHbed_budget*3XQHbed_budget*4XQHbed_budget*5XQHbed_budget*6XQHbed_budget*7XQHbed_budget*8XQHbed_budget*9XQHbed_budget_one*1XQHbed_budget_one*2XQHbed_budget_one*3XQHbed_budget_one*4XQHbed_budget_one*5XQHbed_budget_one*6XQHbed_budget_one*7XQHbed_budget_one*8XQHbed_budget_one*9XQHjp_drawerXQHtile_stellaZ[Mtile_marbleZ[Mtile_brownZ[Msummer_grill*1Y_Isummer_grill*2Y_Isummer_grill*3Y_Isummer_grill*4Y_Isummer_chair*1Y_Isummer_chair*2Y_Isummer_chair*3Y_Isummer_chair*4Y_Isummer_chair*5Y_Isummer_chair*6Y_Isummer_chair*7Y_Isummer_chair*8Y_Isummer_chair*9Y_Isound_set_36ZfIsound_set_35ZfIsound_set_34ZfIsound_set_33ZfIsound_set_32Y_Isound_set_31Y_Isound_set_30Y_Isound_set_29Y_Isound_machine_pro[~Nrare_mnstrY_Ione_way_door*1XQHone_way_door*2XQHone_way_door*3XQHone_way_door*4XQHone_way_door*5XQHone_way_door*6XQHone_way_door*7XQHone_way_door*8XQHone_way_door*9XQHexe_rugZ[Mexe_s_tableZGRsound_set_37ZfIsummer_pool*1ZlIsummer_pool*2ZlIsummer_pool*3ZlIsummer_pool*4ZlIsong_diskY_Ijukebox*1[~Ncarpet_soft_tut[~Nsound_set_44Z@Ksound_set_43Z@Ksound_set_42Z@Ksound_set_41Z@Ksound_set_40Z@Ksound_set_39Z@Ksound_set_38Z@Kgrunge_chairZ@Kgrunge_mattressZ@Kgrunge_radiatorZ@Kgrunge_shelfZ@Kgrunge_signZ@Kgrunge_tableZ@Khabboween_crypt[uKhabboween_grassZ@Khal_cauldronZ@Khal_graveZ@Ksound_set_52ZuKsound_set_51ZuKsound_set_50ZuKsound_set_49ZuKsound_set_48ZuKsound_set_47ZuKsound_set_46ZuKsound_set_45ZuKxmas_icelampZ[Mxmas_cstl_wallZ[Mxmas_cstl_twrZ[Mxmas_cstl_gate[~Ntree7Z[Mtree6Z[Msound_set_54Z[Msound_set_53Z[Msafe_silo_pb[dOplant_mazegate_snowZ[Mplant_maze_snowZ[Mchristmas_sleighZ[Mchristmas_reindeer[~Nchristmas_poopZ[Mexe_bardeskZ[Mexe_chairZ[Mexe_chair2Z[Mexe_cornerZ[Mexe_drinksZ[Mexe_sofaZ[Mexe_tableZ[Msound_set_59[~Nsound_set_58[~Nsound_set_57[~Nsound_set_56[~Nsound_set_55[~Nnoob_table*1[~Nnoob_table*2[~Nnoob_table*3[~Nnoob_table*4[~Nnoob_table*5[~Nnoob_table*6[~Nnoob_stool*1[~Nnoob_stool*2[~Nnoob_stool*3[~Nnoob_stool*4[~Nnoob_stool*5[~Nnoob_stool*6[~Nnoob_rug*1[~Nnoob_rug*2[~Nnoob_rug*3[~Nnoob_rug*4[~Nnoob_rug*5[~Nnoob_rug*6[~Nnoob_lamp*1[dOnoob_lamp*2[dOnoob_lamp*3[dOnoob_lamp*4[dOnoob_lamp*5[dOnoob_lamp*6[dOnoob_chair*1[~Nnoob_chair*2[~Nnoob_chair*3[~Nnoob_chair*4[~Nnoob_chair*5[~Nnoob_chair*6[~Nexe_globe[~Nexe_plantZ[Mval_teddy*1[dOval_teddy*2[dOval_teddy*3[dOval_teddy*4[dOval_teddy*5[dOval_teddy*6[dOval_randomizer[dOval_choco[dOteleport_door[dOsound_set_61[dOsound_set_60[dOfortune[dOsw_tableZIPsw_raven[cQsw_chestZIPsand_cstl_wallZIPsand_cstl_twrZIPsand_cstl_gateZIPgrunge_candleZIPgrunge_benchZIPgrunge_barrelZIPrclr_lampZGRprizetrophy9*1ZGRprizetrophy8*1ZGRnouvelle_traxYcPmd_rugZGRjp_tray6ZGRjp_tray5ZGRjp_tray4ZGRjp_tray3ZGRjp_tray2ZGRjp_tray1ZGRarabian_teamkZGRarabian_snakeZGRarabian_rugZGRarabian_pllwZGRarabian_divdrZGRarabian_chairZGRarabian_bigtbZGRarabian_tetblZGRarabian_tray1ZGRarabian_tray2ZGRarabian_tray3ZGRarabian_tray4ZGRPIpost.itHpost.it.vdHphotoHChessHTicTacToeHBattleShipHPokerHwallpaperHfloorHposterZ@KgothicfountainYxBhc_wall_lampZbBindustrialfanZ`BtorchZ\Bval_heartXBCwallmirrorZ|Ejp_ninjastarsXQHhabw_mirrorXQHhabbowheelZ[Mguitar_skullZ@Kguitar_vZ@Kxmas_light[~Nhrella_poster_3[Nhrella_poster_2ZIPhrella_poster_1[Nsw_swordsZIPsw_stoneZIPsw_holeZIProomdimmerZGRmd_logo_wallZGRmd_canZGRjp_sheet3ZGRjp_sheet2ZGRjp_sheet1ZGRarabian_swordsZGRarabian_wndwZGR")
                    receivedItemIndex = True
                End If

            Case "B^" '// User modifies something with badge
                Dim badgeLen As Integer = HoloENCODING.decodeB64(currentPacket.Substring(2, 2))
                Dim badgeEnabled As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(badgeLen + 4, 1))
                Dim pBadge As String = HoloDB.safeString(currentPacket.Substring(4, badgeLen))

                Dim myBadges() As String = HoloDB.runReadColumn("SELECT badgeid FROM users_badges WHERE userid = '" & UserID & "'", 0)

                For i = 0 To myBadges.Count - 1
                    If myBadges(i) = pBadge Then Exit For '// Badge found, stop searching
                    If i = myBadges.Count - 1 Then If Not (myBadges(i) = pBadge) Then Return '// We are at last badge in the badges the user owns, and still not found, the user does not have this badge!
                Next

                Dim updatePacket As String = "Cd" & HoloENCODING.encodeVL64(userDetails.roomUID)
                If badgeEnabled = 1 Then
                    userDetails.nowBadge = pBadge
                    updatePacket += pBadge
                Else
                    userDetails.nowBadge = vbNullString
                End If

                roomCommunicator.sendAll(updatePacket)
                HoloDB.runQuery("UPDATE users SET badgestatus = '" & badgeEnabled & "," & pBadge & "' WHERE id = '" & UserID & "' LIMIT 1")

            Case "@t", "@x", "@w" '// User speaks/shouts/whispers
                Dim talkType As Char
                Dim talkMessage As String = currentPacket.Substring(4)

                Select Case currentPacket.Substring(1, 1)
                    Case "t"
                        talkType = "X"

                    Case "x"
                        talkType = "Y"

                    Case "w"
                        talkType = "Z"
                    Case Else : Return '// None of them =[
                End Select

                If talkMessage.Substring(0, 1) = ":" Then
                    If handleSpeechCommand(talkMessage) = True Then Return
                End If

                '// Deal with the room about the message
                talkMessage = HoloMISC.filterWord(talkMessage)
                roomCommunicator.doChat(userDetails.roomUID, talkType, talkMessage)

                If HoloRACK.Chat_Animations = True Then '// Chat animations wanted?
                    Dim persnlGesture As String = vbNullString
                    talkMessage = talkMessage.ToLower

                    If talkMessage.Contains(":)") Or talkMessage.Contains(":-)") Or talkMessage.Contains("=]") Or talkMessage.Contains(";)") Or talkMessage.Contains(";-)") Or talkMessage.Contains(":d") Or talkMessage.Contains(":p") Then
                        persnlGesture = "sml"
                    ElseIf talkMessage.Contains(":(") Or talkMessage.Contains(":-(") Or talkMessage.Contains(":s") Then
                        persnlGesture = "sad"
                    ElseIf talkMessage.Contains(":o") Or talkMessage.Contains(":-o") Then
                        persnlGesture = "srp"
                    ElseIf talkMessage.Contains(":@") Or talkMessage.Contains(":-@") Then
                        persnlGesture = "agr"
                    End If

                    userDetails.showTalkAnimation((talkMessage.Length + 50) * 30, persnlGesture)
                End If

            Case "D}" '// Show 'talking...' speech bubble
                roomCommunicator.sendAll("Ei" & HoloENCODING.encodeVL64(userDetails.roomUID) & "I")

            Case "D~" '// Hide 'talking...' speech bubble
                roomCommunicator.sendAll("Ei" & HoloENCODING.encodeVL64(userDetails.roomUID) & "H")

            Case "A^" '// User waves (bad habit =[)
                userDetails.Wave()

            Case "A]" '// User dances
                If currentPacket.Length = 2 Then userDetails.Dance(0) Else userDetails.Dance(HoloENCODING.decodeVL64(currentPacket.Substring(2)))

            Case "Ah" '// User votes in the Lido
                userDetails.showLidoVote(currentPacket.Substring(2))

            Case "As" '// User clicks door of the room
                If IsNothing(roomCommunicator) = True Then Return
                userDetails.DestX = roomCommunicator.doorX
                userDetails.DestY = roomCommunicator.doorY
                userDetails.walkLock = True

            Case "AX" '// Stop an action/status
                Dim toStop As String = currentPacket.Substring(2)
                If toStop = "CarryItem" Then
                    userDetails.removeStatus("drink")
                    userDetails.removeStatus("carryd")
                ElseIf toStop = "Dance" Then
                    userDetails.removeStatus("dance")
                End If

                Try
                    roomCommunicator.refreshUser(userDetails)
                Catch
                End Try

            Case "AP", "AW" '// Start carrying an item
                userDetails.CarryItem(currentPacket.Substring(2))

            Case "AK" '// Walking
                If userDetails.walkLock = True Then Return
                userDetails.DestX = HoloENCODING.decodeB64(currentPacket.Substring(2, 2))
                userDetails.DestY = HoloENCODING.decodeB64(currentPacket.Substring(4, 2))

            Case "Bk" '// Game walking/actioning [BB/SS]
                Dim coX As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                Dim coY As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(HoloENCODING.encodeVL64(coX).Length + 2))

            Case "DG" '// Tags?
                'transData("E^" & HoloENCODING.encodeVL64(UserID) & HoloENCODING.encodeVL64(2) & "Tag1" & Convert.ToChar(2) & "Tag2" & Convert.ToChar(2) )

            Case "Ai" '// Buy tickets
                Dim ticketAmount As Integer = 2
                Dim Price As Integer = 1
                Dim forUser As String = currentPacket.Substring(5).ToLower
                Dim nowCredits As Integer = HoloDB.runRead("SELECT credits FROM users WHERE id = '" & UserID & "'", Nothing)

                If currentPacket.Substring(2, 1) = "J" Then ticketAmount = 20 : Price = 6
                If Price > nowCredits Then transData("AD") : Return
                nowCredits -= Price

                If Not (forUser) = userDetails.Name.ToLower Then If HoloDB.checkExists("SELECT id FROM users WHERE name = '" & forUser & "'") = False Then transData("AL" & forUser) : Return

                HoloDB.runQuery("UPDATE users SET credits = '" & nowCredits & "' WHERE id = '" & UserID & "' LIMIT 1")

                If forUser = userDetails.Name.ToLower Then
                    HoloDB.runQuery("UPDATE users SET tickets = tickets + " & ticketAmount & " WHERE id = '" & UserID & "' LIMIT 1")
                    transData("A|" & HoloDB.runRead("SELECT tickets FROM users WHERE id = '" & UserID & "'"))
                Else
                    HoloDB.runQuery("UPDATE users SET tickets = tickets + " & ticketAmount & " WHERE name = '" & forUser & "' LIMIT 1")
                    Dim receiverID As Integer = HoloDB.runRead("SELECT id FROM users WHERE name = '" & forUser & "'", Nothing)
                    If HoloMANAGERS.isOnline(receiverID) Then HoloMANAGERS.getUserClass(receiverID).transData("A|" & HoloDB.runRead("SELECT tickets FROM users WHERE id = '" & receiverID & "'"))
                End If

                transData("@F" & nowCredits & ".0")

            Case "B_" '// Game - New Ch packet (current game list) request
                If userDetails.inBBLobby = True Then transData(HoloBBGAMELOBBY.getGameList)

            Case "Bb" '// Game - create game request
                If userDetails.inBBLobby = True Then
                    transData("CkPDfieldTypeHJIIIIQAmaximumSimultaneousPowerupsHIKIIIRBpowerupCreateChanceHIRLIIIPYnumTeamsHJJIJIPAcoloringForOpponentTimePulsesHISCIIIPYgameLengthSecondsHIPmIHHallowedPowerupsIJ" & HoloBBGAMELOBBY.allowedPowerUps & ",9HcleaningTilesTimePulsesHISCIIIPYpowerupCreateFirstTimePulsesHIHIHIPYsecondsUntilRestartHIRGIHHpowerupTimeToLivePulsesHIPOIQAIPYpowerupCreateIntervalPulsesHIPEIQAIPYstunTimePulsesHIRBIIIPYhighJumpsTimePulsesHISCIIIPYnameIJHsecondsUntilStartHISCIHH")
                End If

            Case "Bc" '// Game - created game
                If userDetails.inBBLobby = True Then HoloBBGAMELOBBY.createGame(userDetails, currentPacket)

            Case "B`" '// Game - enter game sub
                If userDetails.inBBLobby = True And userDetails.Game_withState = -1 Then DirectCast(HoloBBGAMELOBBY.Games(HoloENCODING.decodeVL64(currentPacket.Substring(2))), clsHoloBBGAME).modViewers(userDetails, False)

            Case "Bg" '// Game - leave sub
                If userDetails.inBBLobby = True Then
                    If userDetails.Game_owns = True Then
                        HoloBBGAMELOBBY.destroyGame(userDetails.Game_ID)
                    Else
                        Dim gameInstance As clsHoloBBGAME = HoloBBGAMELOBBY.Games(userDetails.Game_ID)

                        If userDetails.Game_withState >= 0 Then '// User is in a team; .Game_withState is the team ID
                            gameInstance.modTeam(userDetails, userDetails.Game_withState, -1)
                        ElseIf userDetails.Game_withState = -2 Then '// User is just viewing the sub
                            gameInstance.modViewers(userDetails, False, True)
                        ElseIf userDetails.Game_withState = -3 Then '// User is a spectator
                            gameInstance.modViewers(userDetails, True, True)
                        End If

                        transData("CmH")
                    End If
                End If

            Case "Be" '// Game - join/leave teams
                If userDetails.inBBLobby = True And Not (userDetails.Game_withState = -1) Then
                    If HoloDB.runRead("SELECT tickets FROM users WHERE id = '" & UserID & "'", Nothing) < 2 Then
                        transData("ClJ")
                    Else
                        Dim CBA As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                        Dim newTeamID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(HoloENCODING.encodeVL64(CBA).Length + 2))

                        DirectCast(HoloBBGAMELOBBY.Games(userDetails.Game_ID), clsHoloBBGAME).modTeam(userDetails, userDetails.Game_withState, newTeamID)
                    End If
                End If

            Case "Bj" '// Game - attempt to start game
                DirectCast(HoloBBGAMELOBBY.Games(userDetails.Game_ID), clsHoloBBGAME).startGame()

            Case "Ae" '// User opens catalogue, send pages index
                transData("A~" & HoloCLIENT.catalogueManager.getPageIndex(userDetails.Rank))

            Case "Af" '// User opens catalogue page
                Dim pageIndexName As String = currentPacket.Split("/")(1)
                transData("A" & HoloCLIENT.catalogueManager.getPage(pageIndexName, userDetails.Rank))

            Case "AA" '// User uses the Hand (not to bring a nazi greet, but you know...)
                If IsNothing(roomCommunicator) = True Then Return
                If userDetails.inPublicroom = True Then Return
                refreshHand(currentPacket.Substring(2))

            Case "Ad" '// User buys something out of the catalogue
                Dim packetContent() As String = HoloDB.safeString(currentPacket).Split(Convert.ToChar(13)) '// Make string 'safe' so it can be used in MySQL queries, and split it to string array
                Dim fromPage As String = packetContent(1)
                Dim wantedItem As String = packetContent(3)

                Dim templateID As Integer = HoloDB.runRead("SELECT tid FROM catalogue_items WHERE name_cct = '" & wantedItem & "'", Nothing)
                Dim pageID As Integer = HoloDB.runRead("SELECT indexid FROM catalogue_pages WHERE indexname = '" & fromPage & "' AND minrank <= '" & userDetails.Rank & "'", Nothing)
                Dim itemCost As Integer = HoloDB.runRead("SELECT catalogue_cost FROM catalogue_items WHERE catalogue_id_page = '" & pageID & "' AND tid = '" & templateID & "'", Nothing)
                If itemCost = 0 Then Return '// Item not found on this page! =O
                Dim myCredits As Integer = HoloDB.runRead("SELECT credits FROM users WHERE id = '" & UserID & "'", Nothing)

                If itemCost > myCredits Then transData("AD") : Return '// Not enough credits! Stop here

                Dim receiverID As Integer = UserID '// In most cases users just buy items for themselves
                Dim presentBoxID As Integer = 0
                Dim roomID As Integer = 0 '// If this is set to -1, then the item isn't visible in hand yet, so a present

                If packetContent(5) = "1" Then '// Ohh! Such a goodwilling user! He's *beep*ing buying a *beep*ing item for another user! A present! ZOMG!
                    If Not (packetContent(6) = userDetails.Name) Then '// No need for checking 'does user exist' if the user is buying the present for himself, oh they are so stackable <3
                        Dim testID As Integer = HoloDB.runRead("SELECT id FROM users WHERE name = '" & HoloDB.safeString(packetContent(6)) & "'", Nothing)
                        If testID > 0 Then receiverID = testID Else transData("AL" & packetContent(6)) : Return '// If user exists, set the receiver ID to that users's ID, if it doesn't exist, alert the buyer and return
                    End If

                    '// Create a presentbox
                    Dim boxTemplateID As String = HoloDB.runRead("SELECT tid FROM catalogue_items WHERE name_cct = '" & "present_gen" & New Random().Next(1, 7) & "'")
                    Dim boxNote As String = HoloMISC.filterWord(packetContent(7))
                    HoloDB.runQuery("INSERT INTO furniture(tid,ownerid,opt_var) VALUES ('" & boxTemplateID & "','" & receiverID & "','!" & boxNote & "')")
                    presentBoxID = HoloCLIENT.catalogueManager.lastItemID()
                    roomID = -1
                End If

                myCredits -= itemCost
                transData("@F" & myCredits & ".0")

                If substringIs(wantedItem, "deal", 0, 4) = True Then
                    Dim dealID As Integer = wantedItem.Substring(4)
                    Dim itemIDs() As Integer = HoloDB.runReadColumn("SELECT tid FROM catalogue_deals WHERE id = '" & dealID & "'", 0, Nothing)
                    Dim itemAmounts() As Integer = HoloDB.runReadColumn("SELECT amount FROM catalogue_deals WHERE id = '" & dealID & "'", 0, Nothing)

                    For i = 0 To itemIDs.Count - 1
                        For j = 1 To itemAmounts(i)
                            HoloDB.runQuery("INSERT INTO furniture(tid,ownerid,roomid) VALUES ('" & itemIDs(i) & "','" & receiverID & "','" & roomID & "')")
                            HoloCLIENT.catalogueManager.handleCatalogueSpecialItemAddition(itemIDs(i), receiverID, roomID, presentBoxID)
                        Next
                    Next
                Else
                    HoloDB.runQuery("INSERT INTO furniture(tid,ownerid,roomid) VALUES ('" & templateID & "','" & receiverID & "','" & roomID & "')")
                    If HoloITEM(templateID).cctName = "wallpaper" Or HoloITEM(templateID).cctName = "floor" Then
                        HoloCLIENT.catalogueManager.handleCatalogueSpecialItemAddition(templateID, receiverID, Integer.Parse(packetContent(4)), presentBoxID)
                    Else
                        HoloCLIENT.catalogueManager.handleCatalogueSpecialItemAddition(templateID, receiverID, roomID, presentBoxID)
                    End If
                End If

                If receiverID = UserID Then
                    refreshHand("last")
                Else
                    If HoloMANAGERS.hookedUsers.ContainsKey(receiverID) = True Then HoloMANAGERS.getUserClass(receiverID).refreshHand("last")
                End If

            Case "AZ" '// User places item down
                If IsNothing(roomCommunicator) Then Return '// User not in room
                If userDetails.isOwner = False Then Return '// No placedown rights for this room
                roomCommunicator.placeItem(UserID, currentPacket.Substring(2))

            Case "AC" '// User picks item up
                If IsNothing(roomCommunicator) Then Return '// User not in room
                If userDetails.isOwner = False Then Return '// No pickup rights for this room
                roomCommunicator.removeItem(UserID, Integer.Parse(currentPacket.Split(" ")(2)))
                refreshHand("last")

            Case "AI" '// User rotates/moves item
                If IsNothing(roomCommunicator) Then Return '// User not in room
                If userDetails.hasRights = False Then Return '// No rotate/move rights for this room

                Dim packetContent() As String = currentPacket.Substring(2).Split(" ")
                roomCommunicator.relocateItem(packetContent(0), packetContent(1), packetContent(2), packetContent(3))

            Case "Ac" '// User deletes item [requires mod in external_variables but has been common on retros]
                If IsNothing(roomCommunicator) Then Return '// User not in room
                If userDetails.isOwner = False Then Return '// No pickup rights for this room

                Dim itemID As Integer = currentPacket.Substring(2)
                roomCommunicator.removeItem(0, itemID)
                transData("BK" & "Oh noes!\rIt's gone!")

            Case "Bw" '// User redeems a Habbo Bank item [coin, sack with credits or w/e]
                If IsNothing(roomCommunicator) Then Return '// User not in room
                If userDetails.isOwner = False Then Return '// No pickup rights for this room

                Dim itemID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                Dim spritePart() As String = HoloITEM(HoloDB.runRead("SELECT tid FROM furniture WHERE id = '" & itemID & "' AND roomid = '" & userDetails.roomID & "'", Nothing)).cctName.Split("_")
                If spritePart(0) = "CF" And IsNumeric(spritePart(1)) Then '// If the item was found in this room and it's a Habbo Exchange item
                    Dim newCredits As Integer = HoloDB.runRead("SELECT credits FROM users WHERE id = '" & UserID & "'", Nothing) + Integer.Parse(spritePart(1))
                    transData("@F" & newCredits & ".0")
                    roomCommunicator.removeItem(0, itemID)
                    HoloDB.runQuery("UPDATE users SET credits = credits + " & spritePart(1) & " WHERE id = '" & UserID & "' LIMIT 1")
                End If

            Case "AN" '// User opens presentbox
                If userDetails.isOwner = False Then Return '// User is not in a room/not owner or staff
                Dim itemID As Integer = currentPacket.Substring(2)
                If roomCommunicator.itemInside(itemID) = False Then Return '// Since when was this presentbox in this room, maybe it even doesn't exist! :o

                Dim itemIDs() As Integer = HoloDB.runReadColumn("SELECT itemid FROM furniture_presents WHERE id = '" & itemID & "'", 0, Nothing)
                For i = 0 To itemIDs.Count - 1 '// Make all the items 'visible' in your hand by setting the roomid to 0 [our hand only picks 'with roomid = 0', for presents it's -1, but the ownerid is set already]
                    HoloDB.runQuery("UPDATE furniture SET roomid = '0' WHERE id = '" & itemIDs(i) & "' LIMIT 1")
                Next
                roomCommunicator.removeItem(0, itemID) '// Remove the item from the room

                If itemIDs.Count > 0 Then '// If there were items inside [if not, the box is removed from room and nothing happens now]
                    Dim topTID As Integer = HoloDB.runRead("SELECT tid FROM furniture WHERE id = '" & itemIDs(itemIDs.Count - 1) & "'", Nothing)

                    If HoloITEM(topTID).typeID = 0 Then
                        transData("BA" & HoloITEM(topTID).cctName & Convert.ToChar(13) & HoloITEM(topTID).cctName & " " & HoloITEM(topTID).Colour & Convert.ToChar(13))
                    Else
                        transData("BA" & HoloITEM(topTID).cctName & Convert.ToChar(13) & HoloITEM(topTID).cctName & Convert.ToChar(13) & HoloITEM(topTID).Length & Convert.ToChar(30) & HoloITEM(topTID).Width & Convert.ToChar(30) & HoloITEM(topTID).Colour)
                    End If

                    HoloDB.runQuery("DELETE FROM furniture_presents WHERE id = '" & itemID & "' LIMIT " & itemIDs.Count)
                    HoloDB.runQuery("DELETE FROM furniture WHERE id = '" & itemID & "' LIMIT 1")
                    refreshHand("last")
                End If

            Case "C^" '// Recycler - initialize status
                transData("Do" & HoloCLIENT.recyclerManager.setupString)
                
            Case "C_" '// Recycler - initialize session
                transData("Dp" & HoloCLIENT.recyclerManager.sessionString(UserID))
                If userDetails.roomID > 0 Then refreshHand("new")

            Case "Ca" '// Recycler - proceed input items
                If HoloDB.checkExists("SELECT userid FROM users_recycler WHERE userid = '" & UserID & "'") = True Then Return '// Already a session, stop here

                Dim itemCount As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                If HoloCLIENT.recyclerManager.rewardExists(itemCount) = True Then
                    HoloCLIENT.recyclerManager.createSession(UserID, itemCount)

                    currentPacket = currentPacket.Substring(HoloENCODING.encodeVL64(itemCount).ToString.Length + 2)
                    For i = 0 To itemCount - 1
                        Dim itemID As Integer = HoloENCODING.decodeVL64(currentPacket)
                        If HoloDB.checkExists("SELECT id FROM furniture WHERE ownerid = '" & UserID & "' AND roomid = '0'") = True Then
                            HoloDB.runQuery("UPDATE furniture SET roomid = '-2' WHERE id = '" & itemID & "' LIMIT 1")
                            HoloDB.runQuery("INSERT INTO furniture_recycler(userid,itemid) VALUES ('" & UserID & "','" & itemID & "')")
                            currentPacket = currentPacket.Substring(HoloENCODING.encodeVL64(itemID).ToString.Length)
                        Else '// Item invalid, user does not own this item, drop session + all items that were valid
                            HoloCLIENT.recyclerManager.dropSession(UserID, True)
                            transData("DpH")
                            Return
                        End If
                    Next

                    transData("Dp" & HoloCLIENT.recyclerManager.sessionString(UserID))
                    refreshHand("update")
                End If

            Case "Cb" '// Recycler - redeem/cancel session
                If HoloCLIENT.recyclerManager.sessionExists(UserID) = False Then Return

                Dim redeemSession As Boolean = (currentPacket.Substring(2) = "I")
                If redeemSession = True Then If HoloCLIENT.recyclerManager.sessionReady(UserID) = True Then HoloCLIENT.recyclerManager.rewardSession(UserID)
                HoloCLIENT.recyclerManager.dropSession(UserID, redeemSession)
                transData("Dp" & HoloCLIENT.recyclerManager.sessionString(UserID))

                If redeemSession = True Then refreshHand("last") Else refreshHand("new")

            Case "Cm" '// User wants to send a CFH message
                Dim cfhStats() As String = HoloDB.runReadRow("SELECT id,date,message FROM cms_help WHERE username = '" & userDetails.Name & "'")
                If cfhStats.Count = 0 Then
                    transData("D" & "H")
                Else
                    transData("D" & "I" & cfhStats(0) & Convert.ToChar(2) & cfhStats(1) & Convert.ToChar(2) & cfhStats(2) & Convert.ToChar(2))
                End If

            Case "Cn" '// User deletes his pending CFH message
                HoloDB.runQuery("DELETE FROM cms_help WHERE username = '" & userDetails.Name & "' LIMIT 1")
                transData("DH")

            Case "AV" '// User sends CFH message
                If HoloDB.checkExists("SELECT id FROM cms_help WHERE username = '" & userDetails.Name & "'") = True Then Return
                Dim messageLength As Integer = HoloENCODING.decodeB64(currentPacket.Substring(2, 2))
                Dim cfhMessage As String = currentPacket.Substring(4, messageLength)
                If cfhMessage.Length = 0 Then Return

                HoloDB.runQuery("INSERT INTO cms_help (username,ip,message,date,picked_up,subject,roomid) VALUES ('" & userDetails.Name & "','" & userSocket.RemoteEndPoint.ToString.Split(":")(0) & "','" & HoloDB.safeString(cfhMessage) & "','" & DateTime.Now & "','0','CFH message [Hotel]','" & userDetails.roomID & "')")
                Dim cfhID As Integer = HoloDB.runRead("SELECT id FROM cms_help WHERE username = '" & userDetails.Name & "'", Nothing)
                Dim roomName As String
                If userDetails.inPublicroom = True Then
                    roomName = HoloDB.runRead("SELECT name_caption FROM publicrooms WHERE id = '" & userDetails.roomID & "'")
                Else
                    roomName = HoloDB.runRead("SELECT name FROM guestrooms WHERE id = '" & userDetails.roomID & "'")
                End If

                transData("EAH")
                HoloMANAGERS.sendToRank(6, True, "BT" & HoloENCODING.encodeVL64(cfhID) & Convert.ToChar(2) & "I" & DateTime.Now.ToString & Convert.ToChar(2) & userDetails.Name & Convert.ToChar(2) & cfhMessage & Convert.ToChar(2) & HoloENCODING.encodeVL64(userDetails.roomID) & Convert.ToChar(2) & roomName & Convert.ToChar(2) & "I" & Convert.ToChar(2) & HoloENCODING.encodeVL64(userDetails.roomID))

            Case "CG" ' // CFH center - reply call
                If HoloRANK(userDetails.Rank).containsRight("fuse_receive_calls_for_help") = False Then Return
                Dim cfhID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(4, HoloENCODING.decodeB64(currentPacket.Substring(2, 2))))
                Dim cfhReply As String = currentPacket.Substring(cfhID.ToString.Length + 6)

                Dim toUserName As String = HoloDB.runRead("SELECT username FROM cms_help WHERE id = '" & cfhID & "'")
                If toUserName = vbNullString Then
                    transData("BK" & "Call already handled by you/other Staff members and flagged as 'completed'.")
                Else
                    Dim toUserID As Integer = HoloDB.runRead("SELECT id FROM users WHERE name = '" & toUserName & "'", Nothing)
                    If HoloMANAGERS.isOnline(toUserID) = True Then HoloMANAGERS.getUserClass(toUserID).transData("DR" & cfhReply & Convert.ToChar(2))
                End If

            Case "CF" '// CFH center - 'release' call, other MOD's will see it and the first one who assings it (@p) to itself, handles it
                If HoloRANK(userDetails.Rank).containsRight("fuse_receive_calls_for_help") = False Then Return
                Dim cfhID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(4))
                Dim cfhStats() As String = HoloDB.runReadRow("SELECT username,message,date,roomid FROM cms_help WHERE id = '" & cfhID & "'")
                If cfhStats.Count = 0 Then
                    transData("BK" & "Call [" & cfhID & "] handled by you/other Staff members and flagged as 'completed'.")
                Else
                    HoloMANAGERS.sendToRank(6, True, "BT" & HoloENCODING.encodeVL64(cfhID) & Convert.ToChar(2) & "I" & cfhStats(2) & Convert.ToChar(2) & cfhStats(0) & Convert.ToChar(2) & cfhStats(1) & Convert.ToChar(2) & HoloENCODING.encodeVL64(cfhStats(3)) & Convert.ToChar(2) & "-" & Convert.ToChar(2) & "I" & Convert.ToChar(2) & HoloENCODING.encodeVL64(cfhStats(3)))
                End If

            Case "@p" '// CFH center - assign call to yourself, you'll sort it out, don't let a Habboon in the cold!
                If HoloRANK(userDetails.Rank).containsRight("fuse_receive_calls_for_help") = False Then Return '// Fuck off...

                Dim cfhID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(4))
                If HoloDB.checkExists("SELECT id FROM cms_help WHERE id = '" & cfhID & "'") = False Then
                    transData("BK" & "Call [" & cfhID & "] already handled by you/other Staff members and flagged as 'completed'.")
                Else
                    HoloDB.runQuery("UPDATE cms_help SET picked_up = '" & userDetails.Name & "' WHERE id = '" & cfhID & "' LIMIT 1")
                    ' HoloDB.runQuery("DELETE FROM cms_help WHERE id = '" & cfhID & "' LIMIT 1")
                    ' transData("BK" & "Call [" & cfhID & "] succesfully dropped." )
                End If

            Case "CH" '// MOD-Tool
                Dim messageLength As Integer = 0
                Dim strMessage As String = vbNullString
                Dim staffNoteLength As Integer = 0
                Dim staffNote As String = vbNullString
                Dim targetUser As String = vbNullString

                Select Case currentPacket.Substring(2, 2)
                    Case "HH" '// Alert single user
                        If HoloRANK(userDetails.Rank).containsRight("fuse_alert") = False Then modToolAccessError() : Return
                        messageLength = HoloENCODING.decodeB64(currentPacket.Substring(4, 2))
                        strMessage = currentPacket.Substring(6, messageLength)
                        staffNoteLength = HoloENCODING.decodeB64(currentPacket.Substring(messageLength + 6, 2))
                        staffNote = currentPacket.Substring(messageLength + 8, staffNoteLength)
                        targetUser = currentPacket.Substring(messageLength + staffNoteLength + 10)
                        If strMessage = vbNullString Or targetUser = vbNullString Then Return
                        Try
                            Dim targetUserClass As clsHoloUSER = HoloMANAGERS.getUserClass(HoloDB.safeString(targetUser))
                            targetUserClass.transData("B!" & strMessage & Convert.ToChar(2))
                            HoloMANAGERS.addStaffNote("alert", UserID, targetUserClass.UserID, strMessage, staffNote)

                        Catch
                            transData("BK" & HoloSTRINGS.getString("modtool_actionfail") & "\r" & HoloSTRINGS.getString("modtool_usernotfound"))

                        End Try

                    Case "HI" '// Kick single user from room
                        If HoloRANK(userDetails.Rank).containsRight("fuse_kick") = False Then modToolAccessError() : Return
                        messageLength = HoloENCODING.decodeB64(currentPacket.Substring(4, 2))
                        strMessage = currentPacket.Substring(6, messageLength)
                        staffNoteLength = HoloENCODING.decodeB64(currentPacket.Substring(messageLength + 6, 2))
                        staffNote = currentPacket.Substring(messageLength + 8, staffNoteLength)
                        targetUser = currentPacket.Substring(messageLength + staffNoteLength + 10)
                        If targetUser = vbNullString Then Return
                        Try
                            Dim targetUserClass As clsHoloUSER = HoloMANAGERS.getUserClass(HoloDB.safeString(targetUser))
                            If targetUserClass.userDetails.Rank >= userDetails.Rank Then transData("BK" & HoloSTRINGS.getString("modtool_actionfail") & "\r" & HoloSTRINGS.getString("modtool_rankerror")) : Return
                            targetUserClass.roomCommunicator.removeUser(targetUserClass.userDetails, True, strMessage)
                            HoloMANAGERS.addStaffNote("kick", UserID, targetUserClass.UserID, strMessage, staffNote)

                        Catch
                            transData("BK" & HoloSTRINGS.getString("modtool_actionfail") & "\r" & HoloSTRINGS.getString("modtool_usernotfound"))

                        End Try

                    Case "HJ" '// Ban user
                        If HoloRANK(userDetails.Rank).containsRight("fuse_ban") = False Then modToolAccessError() : Return
                        Dim targetUserLength As Integer = 0
                        Dim banHours As Integer = 0
                        Dim banExpireMoment As String = vbNullString
                        Dim banIP As Boolean = (currentPacket.Substring(currentPacket.Length - 1, 1) = "I")
                        messageLength = HoloENCODING.decodeB64(currentPacket.Substring(4, 2))
                        strMessage = currentPacket.Substring(6, messageLength)
                        staffNoteLength = HoloENCODING.decodeB64(currentPacket.Substring(messageLength + 6, 2))
                        staffNote = currentPacket.Substring(messageLength + 8, staffNoteLength)
                        targetUserLength = HoloENCODING.decodeB64(currentPacket.Substring(messageLength + staffNoteLength + 8, 2))
                        targetUser = currentPacket.Substring(messageLength + staffNoteLength + 10, targetUserLength)
                        banHours = HoloENCODING.decodeVL64(currentPacket.Substring(messageLength + staffNoteLength + targetUserLength + 10))
                        If strMessage = vbNullString Or targetUser = vbNullString Or banHours = 0 Then Return '// No ban without message, 0 hours, or no username

                        Dim targetUserDetails() As String = HoloDB.runReadRow("SELECT id,name,rank,ipaddress_last FROM users WHERE name = '" & HoloDB.safeString(targetUser) & "'")
                        If targetUserDetails.Count = 0 Then transData("BK" & HoloSTRINGS.getString("modtool_actionfail") & "\r" & HoloSTRINGS.getString("modtool_usernotfound")) : Return
                        If Byte.Parse(targetUserDetails(2)) >= userDetails.Rank Then transData("BK" & HoloSTRINGS.getString("modtool_actionfail") & "\r" & HoloSTRINGS.getString("modtool_rankerror"))

                        Dim targetUserClass As clsHoloUSER = HoloMANAGERS.getUserClass(targetUser)
                        If IsNothing(targetUserClass) = False Then targetUserClass.handleBan(strMessage, "User [" & targetUserClass.userDetails.Name & "] banned by [" & userDetails.Name & "] via MOD-Tool")

                        banExpireMoment = DateTime.Now.AddHours(banHours).ToString()
                        HoloDB.runQuery("INSERT INTO users_bans (userid,date_expire,descr,note) VALUES ('" & targetUserDetails(0) & "','" & banExpireMoment & "','" & HoloDB.safeString(strMessage) & "','" & HoloDB.safeString(staffNote) & "')")
                        If banIP = True And HoloRANK(userDetails.Rank).containsRight("fuse_superban") = True Then
                            HoloDB.runQuery("UPDATE users_bans SET ipaddress = '" & targetUserDetails(3) & "' WHERE userid = '" & targetUserDetails(0) & "' LIMIT 1")
                        Else
                            banIP = False
                        End If

                        transData("BK" & "Ban report for " & targetUserDetails(1) & " [" & targetUserDetails(0) & "]\rRank: " & targetUserDetails(2) & "\rOnline: " & (IsNothing(targetUserClass) = True).ToString.ToLower & "\r\rIP address: " & targetUserDetails(3) & "\rDate the ban will be lifted: " & banExpireMoment & "\rIP ban applied: " & banIP.ToString.ToLower & "\rBan reason: " & strMessage & "Note left for staff: " & staffNote)
                        HoloMANAGERS.addStaffNote("ban", UserID, targetUserClass.UserID, strMessage, staffNote)

                    Case "IH" '// Alert all users in current room
                        If IsNothing(roomCommunicator) = True Then Return '// Why bother alerting the room when you are not inside heh?
                        If HoloRANK(userDetails.Rank).containsRight("fuse_room_alert") = False Then modToolAccessError() : Return
                        messageLength = HoloENCODING.decodeB64(currentPacket.Substring(4, 2))
                        strMessage = currentPacket.Substring(6, messageLength)
                        staffNoteLength = HoloENCODING.decodeB64(currentPacket.Substring(messageLength + 6, 2))
                        staffNote = currentPacket.Substring(messageLength + 8, staffNoteLength)

                        If strMessage = vbNullString Then Return '// Empty alert? No wai!
                        roomCommunicator.sendAll("B!" & strMessage & Convert.ToChar(2))
                        HoloMANAGERS.addStaffNote("ralert", UserID, roomCommunicator.roomID, strMessage, staffNote)

                    Case "II" '// Kick all users in current room
                        If IsNothing(roomCommunicator) = True Then Return '// Why bother kicking the room when you are not inside heh?
                        If HoloRANK(userDetails.Rank).containsRight("fuse_room_kick") = False Then modToolAccessError() : Return
                        messageLength = HoloENCODING.decodeB64(currentPacket.Substring(4, 2))
                        strMessage = currentPacket.Substring(6, messageLength)
                        staffNoteLength = HoloENCODING.decodeB64(currentPacket.Substring(messageLength + 6, 2))
                        staffNote = currentPacket.Substring(messageLength + 8, staffNoteLength)

                        roomCommunicator.castRoomKick(userDetails.Rank, strMessage)
                        HoloMANAGERS.addStaffNote("rkick", UserID, roomCommunicator.roomID, strMessage, staffNote)

                End Select

            Case "A`" '// Give rights to someone in your guestroom, you need to be roomowner
                If userDetails.isOwner = False Then Return
                Dim toUser As String = currentPacket.Substring(2)
                roomCommunicator.modRights(toUser, True)

            Case "Aa" '// Remove rights from someone in your guestroom, you need to be roomowner
                If userDetails.isOwner = False Then Return
                Dim toUser As String = currentPacket.Substring(2)
                roomCommunicator.modRights(toUser, False)

            Case "AB" '// User applies wallpaper/floor to his/her room
                If userDetails.isOwner = False Then Return '// Since when do we put wallpaper/floor in someone elses room? Go paint ya own crib biatch :(
                Dim itemID As Integer = currentPacket.Split("/")(1)
                Dim decorItem As String = currentPacket.Substring(2).Split("/")(0)
                If Not (decorItem = "wallpaper" Or decorItem = "floor") Then Return '// Not a floor/wallpaper, what'cha gonna do?

                Dim templateID As Integer = HoloDB.runRead("SELECT tid FROM furniture WHERE id = '" & itemID & "' AND ownerid = '" & UserID & "'", Nothing)
                If Not (HoloITEM(templateID).cctName = decorItem) Then Return '// Item not found / not in your hand, or the item isn't a wallpaper/floor

                Dim decorID As Integer = HoloDB.runRead("SELECT opt_var FROM furniture WHERE id = '" & itemID & "'")
                roomCommunicator.sendAll("@n" & decorItem & "/" & decorID) '// Make the 'flash' in room
                HoloDB.runQuery("UPDATE guestrooms SET " & decorItem & " = '" & decorID & "' WHERE id = '" & userDetails.roomID & "' LIMIT 1") '// Update the wallpaper/floor field for this room in the database
                HoloDB.runQuery("DELETE FROM furniture WHERE id = '" & itemID & "' LIMIT 1") '// Drop the item from the database!

            Case "A_" '// Kick a user from guestoom, must have rights, or be roomowner/staff
                If userDetails.hasRights = False Then Return
                Dim kickTarget As String = currentPacket.Substring(2)
                roomCommunicator.kickUser(kickTarget, userDetails.Rank)

            Case "Cw" '// User spins Habbowheel/wheel of fortune
                If userDetails.hasRights = False Then Return
                Dim itemID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                roomCommunicator.spinHabbowheel(itemID)

            Case "DE" '// User casts vote on guestroom
                Dim castedVote As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2))
                If Not (castedVote = 1 Or castedVote = -1) Then Return
                roomCommunicator.castVote(UserID, castedVote)

            Case "EU" '// Load moodlight settings
                If userDetails.isOwner = False Then Return '// Only room owners [includes staff] are allowed to adjust the moodlight, so if they aren't owner then they don't need the settings!
                Dim settingData As String = roomCommunicator.moodLight_GetSettings
                If Not (settingData = vbNullString) Then transData("Em" & settingData)

            Case "EV" '// Apply modified moodlight settings
                If userDetails.isOwner = False Then Return '// Only room owners [includes staff] are allowed to adjust the moodlight
                Dim presetID As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(2, 1))
                Dim bgState As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(3, 1))
                currentPacket = HoloDB.safeString(currentPacket)
                Dim presetColour As String = currentPacket.Substring(6, HoloENCODING.decodeB64(currentPacket.Substring(4, 2)))
                Dim presetDarkF As Integer = HoloENCODING.decodeVL64(currentPacket.Substring(presetColour.Length + 6))
                roomCommunicator.moodLight_SetSettings(True, presetID, bgState, presetColour, presetDarkF)

            Case "EW" '// Turn moodlight on/off
                If userDetails.isOwner = False Then Return
                roomCommunicator.moodLight_SetSettings(False, 0, 0, vbNullString, 0)

            Case "CV" '// Put wallitem on/off [signing]
                Dim itemID As String = currentPacket.Substring(4, HoloENCODING.decodeB64(currentPacket.Substring(2, 2)))
                Dim toStatus As Integer = HoloDB.safeString(HoloENCODING.decodeVL64(currentPacket.Substring(itemID.Length + 4)))
                roomCommunicator.signWallitem(itemID, toStatus)

            Case "AJ" '// Put floor item on/off [signing]
                Dim itemID As String = currentPacket.Substring(4, HoloENCODING.decodeB64(currentPacket.Substring(2, 2)))
                Dim toStatus As String = HoloDB.safeString(currentPacket.Substring(itemID.Length + 6))
                roomCommunicator.signItem(itemID, toStatus, userDetails.hasRights)

            Case "AG" '// Trading - request trade with someone
                If IsNothing(roomCommunicator) = True Then Return '// User is not in room
                If HoloRACK.enableTrading = False Then transData("BK" & HoloSTRINGS.getString("trading_disabled")) : Return '// Adminstrator has disabled trading, alert & stop here
                If userDetails.containsStatus("trd") Then Return '// This user is already trading

                Dim partnerUID As Integer = currentPacket.Substring(2)
                Dim partnerClass As clsHoloUSERDETAILS = roomCommunicator.roomUserDetails(partnerUID)
                If IsNothing(partnerClass) = True Then Return '// Partner was not in room/not found
                If partnerClass.containsStatus("trd") = True Then Return '// Partner is already trading

                '// Create is-trading status in room
                userDetails.addStatus("trd", vbNullString)
                partnerClass.addStatus("trd", vbNullString)
                Me.Refresh()
                partnerClass.userClass.Refresh()

                userDetails.tradePartnerUID = partnerUID
                partnerClass.tradePartnerUID = userDetails.roomUID
                userDetails.refreshTradeBoxes() : partnerClass.refreshTradeBoxes()

            Case "AH" '// Trading - offer an item
                Dim partnerClass As clsHoloUSERDETAILS = roomCommunicator.roomUserDetails(userDetails.tradePartnerUID)
                If IsNothing(partnerClass) = True Then Return '// Not trading

                Dim itemID As Integer = currentPacket.Substring(2) '// Get the ID of the item the user offers
                Dim templateID As Integer = HoloDB.runRead("SELECT id FROM furniture WHERE id = '" & itemID & "' AND ownerid = '" & UserID & "' AND roomid = '0'", Nothing) '// Get the template ID of the item you want to offer [for use with the item cache]
                If templateID = 0 Then Return '// You don't have this item, dont'cha try to trade someone else's items 
                If HoloITEM(templateID).isTradeable = False Then transData("BK" & HoloSTRINGS.getString("trading_nottradeable")) : Return '// This item is not tradeable, so the 'tradeable=0' in the catalogue_items_table for this template ID. Alert the user and stop here
                userDetails.tradeAccept = False : partnerClass.tradeAccept = False '// Reset 'accept trade' boxes
                userDetails.tradeItems(userDetails.tradeItemCount) = itemID '// Add this item to the slot in the integer array with offered items
                userDetails.tradeItemCount += 1 '// Increment trading count for this users items plus one
                userDetails.refreshTradeBoxes() : partnerClass.refreshTradeBoxes() '// Refresh the tradeboxes

            Case "AD" '// Trading - decline trade
                Dim partnerClass As clsHoloUSERDETAILS = roomCommunicator.roomUserDetails(userDetails.tradePartnerUID)
                If IsNothing(partnerClass) = True Then Return '// Not trading
                userDetails.tradeAccept = False
                userDetails.refreshTradeBoxes() : partnerClass.refreshTradeBoxes()

            Case "AE" '// Trading - accept trade [if both partners have accept then proceed the trading]
                Dim partnerClass As clsHoloUSERDETAILS = roomCommunicator.roomUserDetails(userDetails.tradePartnerUID)
                If IsNothing(partnerClass) = True Then Return '// Not trading
                userDetails.tradeAccept = True
                userDetails.refreshTradeBoxes() : partnerClass.refreshTradeBoxes()

                If partnerClass.tradeAccept = True Then '// The other partner has already accepted, trade the items in database
                    For i = 0 To userDetails.tradeItemCount - 1 '// Swap the items from this user > the partner
                        If userDetails.tradeItems(i) = 0 Then Continue For '// Empty slot
                        HoloDB.runQuery("UPDATE furniture SET ownerid = '" & partnerClass.UserID & "',roomid = '0' WHERE id = '" & userDetails.tradeItems(i) & "' LIMIT 1")
                    Next
                    For i = 0 To partnerClass.tradeItemCount - 1 '// Swap the items from the partner > this user
                        If partnerClass.tradeItems(i) = 0 Then Continue For '// Empty slot
                        HoloDB.runQuery("UPDATE furniture SET ownerid = '" & Me.UserID & "',roomid = '0' WHERE id = '" & partnerClass.tradeItems(i) & "' LIMIT 1")
                    Next

                    '// Handle the abort of the trade
                    userDetails.abortTrade()
                End If

            Case "AF" '// Trading - leave trade
                userDetails.abortTrade() '// Just invoke the abort method, if the user isn't trading at all or something then nothing happens
                refreshHand("update") '// Update the hand [so all the non-traded items are back! :D]

        End Select

        'Catch ex As Exception
        'Console.WriteLine("[ERROR] " & ex.Message & " at packet " & currentPacket & ".")

        'End Try
    End Sub
#End Region
#Region "Public helpers"
    Friend Sub killConnection(Optional ByRef debugMessage As String = vbNullString)
        If killedConnection = True Then Return '// Already killed connnection

        On Error Resume Next '// Just let it handle this procedure, we're not interested in any error catching or w/e
        userSocket.Close()

        If HoloMANAGERS.hookedUsers.ContainsKey(UserID) = True Then HoloMANAGERS.hookedUsers.Remove(UserID)
        If pingManager.IsAlive = True Then pingManager.Abort() '// If the userpinger is running (obv in most cases, DING DING DING) then abort it
        If IsNothing(roomCommunicator) = False Then roomCommunicator.removeUser(userDetails, False)
        HoloSCKMGR.flagSocketAsFree(classID) '// Flag this socket as free again for the socket manager
        userDetails = Nothing

        killedConnection = True '// Flag this class as destroyed already so it doesn't get caught by close following killConnection()'s
        Me.Finalize() '// Destroy this class and make it available to .NET's garabage collector
        Console.WriteLine("[SCKMGR] [" & classID & "] dumped and all familar resources destroyed.") '// Print that the clearup has succeeded
        If Not (debugMessage = vbNullString) Then Console.WriteLine("[SCKMGR] Reason: " & debugMessage)
    End Sub
    Friend Sub Refresh()
        If IsNothing(roomCommunicator) = False Then roomCommunicator.refreshUser(userDetails)
    End Sub
    Friend Sub refreshClub()
        '// Thanks to Jeax for date comparing example in JASE, I am bad at doing stuff with dates/months w/e
        Dim restingDays, passedMonths, restingMonths As Integer
        Dim dbRow() As String = HoloDB.runReadRow("SELECT months_expired,months_left,date_monthstarted FROM users_club WHERE userid = '" & UserID & "'")
        If dbRow.Count > 0 Then '// If the user has subscribed to Club
            passedMonths = Integer.Parse(dbRow(0)) '// Get the amount of expired months
            restingMonths = Integer.Parse(dbRow(1)) - 1 '// Get the amount of resting months and manipulate this count just for correct packet
            restingDays = (DateTime.Parse(dbRow(2))).Subtract(DateTime.Now).TotalDays + 31 '// Get the amount of resting days
            If userDetails.Rank = 1 Then '// If user is normal user and club, but hasn't received the club rank
                HoloDB.runQuery("UPDATE users SET rank = '2' WHERE id = '" & UserID & "' LIMIT 1") '// Update rank in database
                userDetails.Rank = 2
            End If
            userDetails.clubMember = True
        End If

        transData("@Gclub_habbo" & Convert.ToChar(2) & HoloENCODING.encodeVL64(restingDays) & HoloENCODING.encodeVL64(passedMonths) & HoloENCODING.encodeVL64(restingMonths) & "I")
    End Sub
    Friend Sub refreshBadges()
        Dim b, activeBadgeSlot As Integer
        Dim myBadges(), myBadgeStatus() As String

        myBadges = HoloDB.runReadColumn("SELECT badgeid FROM users_badges WHERE userid = '" & UserID & "'", 0)

        If myBadges.Count > 0 Then '// If this user has badges
            Dim badgePacketBuilder As New StringBuilder("Ce" & HoloENCODING.encodeVL64(myBadges.Count))
            myBadgeStatus = HoloDB.runRead("SELECT badgestatus FROM users WHERE id = '" & UserID & "'").Split(",")

            For b = 0 To myBadges.Count - 1
                badgePacketBuilder.Append(myBadges(b) & Convert.ToChar(2))
                If activeBadgeSlot = 0 Then If myBadges(b) = myBadgeStatus(1) Then activeBadgeSlot = b
            Next

            If Integer.Parse(myBadgeStatus(0)) = 1 Then userDetails.nowBadge = myBadgeStatus(1)
            transData(badgePacketBuilder.ToString & HoloENCODING.encodeVL64(activeBadgeSlot) & HoloENCODING.encodeVL64(myBadgeStatus(0)))
        Else
            transData("CeHH")
        End If
    End Sub
    Friend Sub refreshAppearance(ByVal reloadFromDB As Boolean)
        If reloadFromDB = True Then
            Dim userData() As String = HoloDB.runReadRow("SELECT figure,sex,mission FROM users WHERE id = '" & UserID & "'")
            userDetails.Figure = userData(0)
            userDetails.Sex = Char.Parse(userData(1))
            userDetails.Mission = userData(2)
        End If
        transData("@E" & classID & Convert.ToChar(2) & userDetails.Name & Convert.ToChar(2) & userDetails.Figure & Convert.ToChar(2) & userDetails.Sex & Convert.ToChar(2) & userDetails.Mission & Convert.ToChar(2) & "Hch=s02/253,146,160" & Convert.ToChar(2) & "HI")
        If IsNothing(roomCommunicator) = False Then roomCommunicator.sendAll("DJ" & HoloENCODING.encodeVL64(userDetails.roomUID) & userDetails.Figure & Convert.ToChar(2) & userDetails.Sex & Convert.ToChar(2) & userDetails.Mission & Convert.ToChar(2)) '// Poof and refresh users look in room [only if the user is in room, thus the roomCommunicator is not nulled]
    End Sub
    Friend Sub refreshValuables()
        Dim valData() As String = HoloDB.runReadRow("SELECT credits,tickets FROM users WHERE id = '" & UserID & "'")
        transData("@F" & valData(0) & Convert.ToChar(1) & "A|" & valData(1))
    End Sub
    Friend Sub refreshHand(ByVal strMode As String)
        Dim startID, stopID As Integer
        Dim itemIDs() As Integer = HoloDB.runReadColumn("SELECT id FROM furniture WHERE ownerid = '" & UserID & "' AND roomid = '0' ORDER BY id ASC", 0, Nothing)

        Dim handPack As New StringBuilder("BL")

        stopID = itemIDs.Count
        Select Case strMode '// If strMode is 'update', then it doesn't do anything here and the current pagenumber will stay the same, which is what we want :D
            Case "next"
                curHandPage += 1
            Case "prev"
                curHandPage -= 1
            Case "last"
                curHandPage = (stopID - 1) / 9
            Case "new"
                curHandPage = 0
        End Select

        Try
            If itemIDs.Count > 0 Then
reCount:
                startID = curHandPage * 9
                If stopID > (startID + 9) Then stopID = startID + 9
                If (startID > stopID) Or (startID = stopID) Then curHandPage -= 1 : GoTo reCount '// Jew :(

                For i = startID To stopID - 1
                    Dim templateID As Integer = HoloDB.runRead("SELECT tid FROM furniture WHERE id = '" & itemIDs(i) & "'", Nothing)
                    handPack.Append("SI" & Convert.ToChar(30) & itemIDs(i) & Convert.ToChar(30) & i & Convert.ToChar(30))
                    If HoloITEM(templateID).typeID = 0 Then handPack.Append("I") Else handPack.Append("S")
                    handPack.Append(Convert.ToChar(30) & itemIDs(i) & Convert.ToChar(30) & HoloITEM(templateID).cctName & Convert.ToChar(30))
                    If HoloITEM(templateID).typeID > 0 Then handPack.Append(HoloITEM(templateID).Length & Convert.ToChar(30) & HoloITEM(templateID).Width & Convert.ToChar(30) & HoloDB.runRead("SELECT opt_var FROM furniture WHERE id = '" & itemIDs(i) & "'") & Convert.ToChar(30))
                    handPack.Append(HoloITEM(templateID).Colour & Convert.ToChar(30))
                    If HoloITEM(templateID).isRecycleable = True Then handPack.Append("1") Else handPack.Append("0")
                    handPack.Append(Convert.ToChar(30) & "/")
                Next
            End If
            handPack.Append(Convert.ToChar(13) & itemIDs.Count)
            transData(handPack.ToString)

        Catch
            transData("BL" & Convert.ToChar(13) & "0")

        End Try
        '// DEBUG! :D
        '//    '// SI+ {30} + -ID + {30} + iI + {30} + TYPE + {30} + ID + {30} + CCT + {30} + Len + {30} + Wid + {30} + VAR + {30} + COLOR + {30} + iI + {30} + CCT + {30} + "/"
        '// Console.WriteLine(startID & " - " & stopID & " |HANDPAGE: " & curHandPage)
    End Sub
    Friend Sub resetRoomStatus()
        userDetails.Reset()
        roomCommunicator = Nothing
    End Sub
    Friend Sub handleBan(ByVal banMessage As String, ByVal dcMessage As String)
        Try
            transData("@c" & banMessage)
            If IsNothing(roomCommunicator) = False Then roomCommunicator.removeUser(userDetails, True)
            Thread.Sleep(1000)
            killConnection(dcMessage)
        Catch
        End Try
    End Sub
#End Region
#Region "Private helpers"
    Private Function handleSpeechCommand(ByVal talkMessage As String) As Boolean
        Dim commandPart() = talkMessage.Split(" ")
        Dim theCommand As String = commandPart(0).Substring(1)
        If commandPart.Count = 1 Then
            Select Case theCommand

                Case ":about"
                    MsgBox(userDetails.roomUID)
                    transData("BK" & "Hey " & userDetails.Name & ", you currently are on a Holograph Emulator for Habbo Hotel!\r\rWe forgot something? What the hell?\rOh yes!\r'Hello world!\r\r- Nillus and co")
                    Return True

            End Select
        Else
            Select Case theCommand
                Case "hello"
                    transData("BK" & "Hello, you said " & commandPart(1) & "!")
                    Return True

                Case "reload"
                    If userDetails.Rank = 7 Then
                        Select Case commandPart(1)
                            Case "catalogue"
                                HoloCLIENT.catalogueManager.Init()
                                Return True

                            Case "somecommand"
                                Return True
                        End Select
                    End If
            End Select
        End If

        Return False
    End Function
    Private Sub modToolAccessError()
        transData("BK" & HoloSTRINGS.getString("modtool_accesserror"))
    End Sub
#End Region
End Class
