Imports System.Text
Imports System.Threading
Public Class clsHoloROOM
    Friend roomID As Integer
    Friend isPublicRoom As Boolean
    Friend Delegate Sub userClassWorker(ByVal User As clsHoloUSER)

    Private roomModel As Byte
    Private publicRoomItems As String
    Private publicRoomHeightmap As String
    Private furnitureItems As Hashtable

    Private ocState(,) As Byte '// Typestate on X,Y | 0 = blocked, 1 = open, 2 = seat, 3 = bed, 4 = rug
    Private ocItemRot(,) As Byte '// Rotation of the item on X,Y
    Private ocHeight(,) As Byte '// Walkeight on X,Y
    Private ocItemHeight(,) As Double '// Itemheight on X,Y
    Private ocUserHere(,) As Boolean '// User here true/false

    Friend doorX, doorY As Integer
    Private doorH As Double

    Private roomUsers As New Hashtable
    Private roomUsersByName As New Hashtable
    Private ownerID As Integer
    Private walkManager As New Thread(AddressOf manageWalks)
    Private Delegate Sub timedPacketHandler(ByVal strPacket As String, ByVal msInterval As Integer)
#Region "Generic room tasks"
    Friend Sub New(ByVal thisRoomID As Integer, ByVal isPublic As Boolean)
        roomID = thisRoomID
        isPublicRoom = isPublic
        Dim tempHeightMap() As String

        If isPublicRoom = True Then
            Dim pubDoor() As String = HoloDB.runRead("SELECT map_door FROM publicrooms WHERE id = '" & roomID & "'").Split(" ")
            doorX = pubDoor(0)
            doorY = pubDoor(1)
            doorH = Double.Parse(pubDoor(2))
            publicRoomHeightmap = HoloDB.runRead("SELECT map_height FROM publicrooms WHERE id = '" & roomID & "'")
            tempHeightMap = publicRoomHeightmap.Split(Convert.ToChar(13))
        Else
            roomModel = HoloDB.runRead("SELECT model FROM guestrooms WHERE id = '" & roomID & "'")
            doorX = HoloSTATICMODEL(roomModel).doorX
            doorY = HoloSTATICMODEL(roomModel).doorY
            doorH = HoloSTATICMODEL(roomModel).doorH
            tempHeightMap = HoloSTATICMODEL(roomModel).strMap.Split(Convert.ToChar(13))
            furnitureItems = New Hashtable()
        End If

        Dim maxX As Integer = tempHeightMap(0).Length - 1
        Dim maxY As Integer = tempHeightMap.Length - 1

        ocState = New Byte(maxX, maxY) {}
        ocItemRot = New Byte(maxX, maxY) {}
        ocHeight = New Byte(maxX, maxY) {}
        ocItemHeight = New Double(maxX, maxY) {}
        ocUserHere = New Boolean(maxX, maxY) {}

        For Y = 0 To maxY - 1
            For X = 0 To maxX
                Dim curSq As String = tempHeightMap(Y).Substring(X, 1).Trim.ToLower
                Select Case curSq
                    Case "x", vbNullString '// Blocked
                    Case "o" '// Unblockable [no afro blocking here and aids is done kthx]
                        ocState(X, Y) = 1
                        '// Special unblockable bool herreeeee
                    Case Else '// Walkable
                        ocState(X, Y) = 1 '// Set this X,Y to walkable/open
                        ocHeight(X, Y) = Byte.Parse(curSq) '// Parse the current square height of X,Y
                End Select
            Next
        Next

        If isPublicRoom = True Then '// Do the publicroom items
            Dim tempItemMap() As String = HoloDB.runRead("SELECT map_items FROM publicrooms WHERE id = '" & roomID & "'").Split(vbCrLf)
            Dim tempCompletedItemMap As New StringBuilder
            For curItem = 0 To tempItemMap.Count - 1
                Dim curItemStats() As String = tempItemMap(curItem).Split(" ")
                Dim itemX, itemY, itemT As Integer
                itemX = curItemStats(2)
                itemY = curItemStats(3)
                itemT = curItemStats(6)

                If itemT = 1 Then '// Solid! =D
                    ocHeight(itemX, itemY) = 0
                ElseIf itemT = 2 Then '// Seat! =D
                    ocState(itemX, itemY) = 2
                    ocItemRot(itemX, itemY) = curItemStats(5)
                    ocItemHeight(itemX, itemY) = 1.0
                End If

                tempCompletedItemMap.Append(curItemStats(0) & " " & curItemStats(1) & " " & curItemStats(2) & " " & curItemStats(3) & " " & curItemStats(4) & " " & curItemStats(5) & Convert.ToChar(13))
            Next
            publicRoomItems = tempCompletedItemMap.ToString
        Else '// Do the inside furnitures, read them from database, store them into classes etc
            Dim itemIDs() As Integer = HoloDB.runReadColumn("SELECT id FROM furniture WHERE roomid = '" & roomID & "' AND opt_wallpos IS NULL", 0, Nothing)
            Dim itemTemplateIDs() As Integer = HoloDB.runReadColumn("SELECT tid FROM furniture WHERE roomid = '" & roomID & "' AND opt_wallpos IS NULL", 0, Nothing)
            Dim itemXs() As Integer = HoloDB.runReadColumn("SELECT x FROM furniture WHERE roomid = '" & roomID & "' AND opt_wallpos IS NULL", 0, Nothing)
            Dim itemYs() As Integer = HoloDB.runReadColumn("SELECT y FROM furniture WHERE roomid = '" & roomID & "' AND opt_wallpos IS NULL", 0, Nothing)
            Dim itemZs() As Integer = HoloDB.runReadColumn("SELECT z FROM furniture WHERE roomid = '" & roomID & "' AND opt_wallpos IS NULL", 0, Nothing)
            Dim itemHs() As String = HoloDB.runReadColumn("SELECT h FROM furniture WHERE roomid = '" & roomID & "' AND opt_wallpos IS NULL", 0)
            Dim itemVars() As String = HoloDB.runReadColumn("SELECT opt_var FROM furniture WHERE roomid = '" & roomID & "' AND opt_wallpos IS NULL", 0)

            For i = 0 To itemIDs.Count - 1
                Dim newItem As New furnitureItem(itemIDs(i), itemTemplateIDs(i), itemXs(i), itemYs(i), itemZs(i), itemHs(i))
                If Not (itemVars(i) = vbNullString) Then newItem.Var = itemVars(i)

                If Not (HoloITEM(newItem.tID).typeID = 4) Then '// Items modifies the heightmap (so it's not a rug (4))
                    Dim iLength As Integer = getItemLength(newItem.tID, newItem.Z)
                    Dim iWidth As Integer = getItemWidth(newItem.tID, newItem.Z)

                    For tX = newItem.X To newItem.X + iWidth - 1
                        For tY = newItem.Y To newItem.Y + iLength - 1
                            If HoloITEM(newItem.tID).typeID = 2 Then
                                ocState(tX, tY) = 2
                                ocItemHeight(tX, tY) = HoloITEM(newItem.tID).topH
                                ocItemRot(tX, tY) = newItem.Z
                            Else
                                ocState(tX, tY) = 0
                                If IsNothing(newItem.Var) = False Then If newItem.Var.ToLower = "o" Then If HoloITEM(newItem.tID).isDoor = True Then ocState(tX, tY) = 1
                            End If
                        Next
                    Next
                End If

                furnitureItems.Add(newItem.ID, newItem) '// Add the furniture item to the 'inside furiniture items' hashtable
            Next
        End If

        ocState(doorX, doorY) = 0 '// Make the door always walkable
        Console.WriteLine("[ROOM] Room " & roomID & " [publicroom: " & isPublicRoom.ToString.ToLower & "] loaded.")
    End Sub
    Friend Sub Unload(ByVal sendKick As Boolean)
        On Error Resume Next
        If roomUsers.Count > 0 Then '// Still people inside, room is forcefully unloaded
            For Each roomUserDetails As clsHoloUSERDETAILS In roomUsers.Values
                roomUserDetails.userClass.resetRoomStatus() '// Reset room user values and null roomCommunicator
                If sendKick = True Then roomUserDetails.userClass.transData("@R") '// Send kick packet
            Next
        End If

        HoloMANAGERS.updateRoomInsideCount(roomID, isPublicRoom, 0) '// Update the inside count in the database to 0
        HoloMANAGERS.hookedRooms.Remove(roomID) '// Remove roomclass from hashtable
        Console.WriteLine("[ROOM] Room " & roomID & " [publicroom: " & isPublicRoom.ToString.ToLower & "] destroyed.")
        walkManager.Abort()
        Me.Finalize()
    End Sub
    Friend Sub sendAll(ByVal strData As String)
        For Each roomUser As clsHoloUSERDETAILS In roomUsers.Values
            roomUser.userClass.transData(strData)
        Next
    End Sub
    Friend Sub sendToRightHavingUsers(ByVal strData As String)
        For Each roomUserDetails As clsHoloUSERDETAILS In roomUsers.Values
            If roomUserDetails.hasRights = True Then roomUserDetails.userClass.transData(strData)
        Next
    End Sub
    Private Sub manageWalks()
        While True
            '
            Dim syncStack As New StringBuilder '// No start value because this thread keeps running during the whole lifetime of the room
            For Each ruD As clsHoloUSERDETAILS In DirectCast(roomUsers.Clone, Hashtable).Values
                If ruD.DestX = -1 Then Continue For

                Dim jieksMap(,) As Byte
                jieksMap = ocState.Clone

                Try
                    If ocState(ruD.DestX, ruD.DestY) = 2 Then jieksMap(ruD.DestX, ruD.DestY) = 1
                    If ocUserHere(ruD.DestX, ruD.DestY) = True Then jieksMap(ruD.DestX, ruD.DestY) = 0
                Catch
                End Try

                Dim Jieks As New clsHoloPATHFINDER(jieksMap, ocHeight, ocUserHere)
                Dim nextCoords() As Integer = Jieks.getNextStep(ruD.PosX, ruD.PosY, ruD.DestX, ruD.DestY)

                ruD.removeStatus("mv")

                If IsNothing(nextCoords) = True Then
                    If ocState(ruD.PosX, ruD.PosY) = 2 Then '// Seat reached
                        ruD.removeStatus("dance")
                        ruD.addStatus("sit", ocItemHeight(ruD.PosX, ruD.PosY))
                        ruD.rotHead = ocItemRot(ruD.PosX, ruD.PosY)
                        ruD.DestX = -1
                    End If
                Else
                    ruD.removeStatus("sit")
                    ruD.addStatus("mv", nextCoords(0) & "," & nextCoords(1) & "," & ocHeight(nextCoords(0), nextCoords(1)))
                    ruD.rotHead = nextCoords(2)
                End If
                ruD.rotBody = ruD.rotHead

                syncStack.Append(ruD.dynamicStatus & Convert.ToChar(13))

                If IsNothing(nextCoords) Then
                    ruD.DestX = -1
                    If ruD.walkLock = True Then removeUser(ruD, True) : Continue For '// User has clicked the door and no more coords where found, so who knows and he might be in the door now?!
                Else
                    ocUserHere(ruD.PosX, ruD.PosY) = False
                    ruD.PosX = nextCoords(0)
                    ruD.PosY = nextCoords(1)
                    ruD.PosH = ocHeight(nextCoords(0), nextCoords(1))
                    ocUserHere(ruD.PosX, ruD.PosY) = True
                End If
            Next
            If Not (syncStack.Length = 0) Then
                sendAll("@b" & syncStack.ToString)
            End If
            Thread.Sleep(455)
            '
        End While
    End Sub
#End Region
#Region "Room dynamic properties"
    Friend ReadOnly Property Heightmap() As String
        Get
            If isPublicRoom = True Then '// This roomclass is for a publicroom
                Return publicRoomHeightmap
            Else
                Return HoloSTATICMODEL(roomModel).strMap
            End If
        End Get
    End Property
    Friend ReadOnly Property insideUsers() As String
        Get
            Dim userPack As New StringBuilder
            For Each roomUserDetails As clsHoloUSERDETAILS In roomUsers.Values
                userPack.Append(roomUserDetails.ToString)
            Next

            Return userPack.ToString
        End Get
    End Property
    Friend ReadOnly Property insideUsersDynamics() As String
        Get
            Dim refreshPack As New StringBuilder()
            For Each roomUser As clsHoloUSERDETAILS In roomUsers.Values
                refreshPack.Append(roomUser.dynamicStatus & Convert.ToChar(13))
            Next
            Return refreshPack.ToString
        End Get
    End Property
    Friend ReadOnly Property Items() As String
        Get
            If isPublicRoom = True Then
                Return "H"
            Else
                Dim itemPack As New StringBuilder
                itemPack.Append(HoloENCODING.encodeVL64(furnitureItems.Count))

                For Each furnitureItem As furnitureItem In furnitureItems.Values
                    Dim templateID As Integer = furnitureItem.tID
                    itemPack.Append(furnitureItem.ToString)
                Next

                Return itemPack.ToString
            End If
        End Get
    End Property
    Friend ReadOnly Property wallItems() As String
        Get
            If isPublicRoom = True Then
                Return vbNullString
            Else
                Dim itemIDs() As Integer = HoloDB.runReadColumn("SELECT id FROM furniture WHERE roomid = '" & roomID & "' AND NOT(opt_wallpos IS NULL)", 0, Nothing)
                If itemIDs.Count = 0 Then Return vbNullString '// No wallitems, saves us some queries :)

                Dim itemTemplateIDs() As Integer = HoloDB.runReadColumn("SELECT tid FROM furniture WHERE roomid = '" & roomID & "' AND NOT(opt_wallpos IS NULL)", 0, Nothing)
                Dim itemWallpositions() As String = HoloDB.runReadColumn("SELECT opt_wallpos FROM furniture WHERE roomid = '" & roomID & "' AND NOT(opt_wallpos IS NULL)", 0)
                Dim itemVars() As String = HoloDB.runReadColumn("SELECT opt_var FROM furniture WHERE roomid = '" & roomID & "' AND NOT(opt_wallpos IS NULL)", 0)

                Dim wallItemPack As New StringBuilder
                For i = 0 To itemIDs.Count - 1
                    wallItemPack.Append(itemIDs(i) & Convert.ToChar(9) & HoloITEM(itemTemplateIDs(i)).cctName & Convert.ToChar(9) & " " & Convert.ToChar(9) & itemWallpositions(i) & Convert.ToChar(9))
                    If itemVars(i) = vbNullString Then wallItemPack.Append(HoloITEM(itemTemplateIDs(i)).Colour) Else wallItemPack.Append(itemVars(i)) '// If the var is blank, then add the wallitem's 'colour', if it isn't blank, add it's var [var = special variable for item, like light on/off]
                    wallItemPack.Append(Convert.ToChar(13))
                Next

                Return wallItemPack.ToString
            End If
        End Get
    End Property
    Friend ReadOnly Property otherItems() As String
        Get
            If isPublicRoom = True Then
                Return publicRoomItems
            Else
                Return vbNullString
            End If
        End Get
    End Property
    Friend ReadOnly Property Votes(ByVal userID As Integer) As String
        Get
            If userID = 0 OrElse HoloDB.checkExists("SELECT userid FROM guestroom_votes WHERE userid = '" & userID & "' AND roomid = '" & roomID & "'") = True Then
                Dim voteSum As Integer = HoloDB.runRead("SELECT SUM(vote) FROM guestroom_votes WHERE roomid = '" & roomID & "'", Nothing)
                If voteSum < 1 Then voteSum = 0
                Return HoloENCODING.encodeVL64(voteSum)
            Else
                Return "M" '// Return a -1 in VL64, so the Thumb up/down buttons appear
            End If
        End Get
    End Property
    Friend ReadOnly Property whosInHereList() As String
        Get
            Dim listBuilder As New StringBuilder(HoloENCODING.encodeVL64(roomID) & HoloENCODING.encodeVL64(roomUsers.Count))
            For Each roomUserDetails As clsHoloUSERDETAILS In roomUsers.Values
                listBuilder.Append(roomUserDetails.Name & Convert.ToChar(2))
            Next
            Return listBuilder.ToString
        End Get
    End Property
#End Region
#Region "User & bot management"
#Region "User management"
    Friend Sub addUser(ByRef newUser As clsHoloUSERDETAILS)
        If newUser.isAllowedInRoom = False Then newUser.userClass.roomCommunicator = Nothing : newUser.Reset() : newUser.userClass.transData("@R") : Return '// How in hells name did you got here? You haven't been autorised! Gtfo! Tubgirl!!11

        If roomUsers.Count = 0 Then walkManager.Start()
        If roomUsers.ContainsValue(newUser) = False Then
            newUser.roomUID = freeRoomUID()
            roomUsers.Add(newUser.roomUID, newUser)
            roomUsersByName.Add(newUser.Name, newUser)
        End If

        '// Set user's position matching the door of this room
        newUser.PosX = doorX
        newUser.PosY = doorY
        newUser.PosH = doorH

        sendAll("@\" & newUser.ToString) '// Make user appear in room

        '// Update room inside count
        HoloMANAGERS.updateRoomInsideCount(roomID, isPublicRoom, roomUsers.Count)
    End Sub
    Friend Sub removeUser(ByVal leavingUser As clsHoloUSERDETAILS, ByVal sendKick As Boolean, Optional ByVal kickMessage As String = vbNullString)
        If sendKick = True Then leavingUser.userClass.transData("@R")
        If Not (kickMessage = vbNullString) Then leavingUser.userClass.transData("B!" & kickMessage & Convert.ToChar(2))

        If roomUsers.Count > 1 Then '// If there are more than just this user in the room
            roomUsers.Remove(leavingUser.roomUID)
            roomUsersByName.Remove(leavingUser.Name)
            ocUserHere(leavingUser.PosX, leavingUser.PosY) = False

            sendAll("@]" & leavingUser.roomUID) '// Send the 'make user disappear' packet to the room
            HoloMANAGERS.updateRoomInsideCount(roomID, isPublicRoom, roomUsers.Count) '// Update the inside count
        Else '// Last user leaves the room
            Me.Unload(False)
        End If

        leavingUser.Reset()
        leavingUser.userClass.roomCommunicator = Nothing
    End Sub
    Friend Sub refreshUser(ByRef userDetails As clsHoloUSERDETAILS)
        sendAll("@b" & userDetails.dynamicStatus)
    End Sub
    Friend Sub kickUser(ByVal kickTarget As String, ByVal myRank As Integer, Optional ByVal kickReason As String = vbNullString)
        If roomUsersByName.ContainsKey(kickTarget) = False Then Return
        Dim kickUser As clsHoloUSERDETAILS = roomUsersByName(kickTarget)

        If kickUser.isOwner = True Then If HoloRANK(myRank).containsRight("fuse_any_room_controller") = False Then Return '// If you are trying to kick the room owner (so also staff!) and you don't have the 'fuse_any_room_controller' fuse right, then you can't kick [so: room owner can't kick staff ;P]
        If kickReason = vbNullString Then '// Plain room kick
            If kickUser.PosX = doorX And kickUser.PosY = doorY Then removeUser(kickUser, True) : Return

            kickUser.walkLock = True
            kickUser.DestX = doorX
            kickUser.DestY = doorY
        Else '// Kicked by staff
            removeUser(kickUser, True)
            kickUser.userClass.transData("B!" & kickReason & Convert.ToChar(2)) '// Kicked by staffmember, send the reason
        End If
    End Sub
    Friend Sub modRights(ByVal withUserName As String, ByVal addInsteadOfRemove As Boolean)
        Try
            If roomUsersByName.ContainsKey(withUserName) = False Then Return
            Dim withUserDetails As clsHoloUSERDETAILS = roomUsersByName(withUserName)
            If addInsteadOfRemove = True Then
                If HoloDB.checkExists("SELECT userid FROM guestroom_rights WHERE userid = '" & withUserDetails.UserID & "' AND roomid = '" & roomID & "'") = True Then Return '// User already has rights
                HoloDB.runQuery("INSERT INTO guestroom_rights (userid,roomid) VALUES ('" & withUserDetails.UserID & "','" & roomID & "')")

                withUserDetails.hasRights = True
                withUserDetails.userClass.transData("@j")
                withUserDetails.addStatus("flatctrl", "onlyfurniture")
            Else
                HoloDB.runQuery("DELETE FROM guestroom_rights WHERE userid = '" & withUserDetails.UserID & "' AND roomid = '" & roomID & "'")
                withUserDetails.hasRights = False
                withUserDetails.userClass.transData("@k")
                withUserDetails.removeStatus("flatctrl")
            End If

            refreshUser(withUserDetails)
        Catch
        End Try
    End Sub
#End Region
#Region "Bot management"
    Private Sub reloadBots()
        Dim botIDs() As String = HoloDB.runReadColumn("SELECT id FROM roombots WHERE roomid = '" & roomID & "'", 0)

        For i = 0 To botIDs.Count - 1
            Dim botData() As String = HoloDB.runReadRow("SELECT name,figure,mission,location,chat,movements FROM roombots WHERE id = '" & botIDs(i) & "'")
            If botData.Count = 0 Then Continue For


        Next
    End Sub
#End Region
    Friend Sub doChat(ByRef roomUID As Integer, ByVal talkType As Char, ByVal talkMessage As String)
        sendAll("Ei" & HoloENCODING.encodeVL64(roomUID) & "H" & Convert.ToChar(1) & "@" & talkType & HoloENCODING.encodeVL64(roomUID) & talkMessage & Convert.ToChar(2))
        '// head bobbing shiz here ;D
    End Sub
    Friend Function roomUserDetails(ByVal roomUID As Integer) As clsHoloUSERDETAILS
        If roomUsers.ContainsKey(roomUID) = False Then Return Nothing
        Return roomUsers(roomUID)
    End Function
    Friend Function roomUserDetails(ByVal userName As String) As clsHoloUSERDETAILS
        If roomUsersByName.ContainsKey(userName) = False Then Return Nothing
        Return roomUsers(userName)
    End Function
    Private Function freeRoomUID() As Integer
        Dim i As Integer
        While (True)
            If roomUsers.ContainsKey(i) = False Then Return i
            i += 1
        End While
    End Function
    Private Sub refreshSpot(ByVal posX As Integer, ByVal posY As Integer)
        For Each roomUserDetails As clsHoloUSERDETAILS In roomUsers.Values
            If roomUserDetails.PosX = posX And roomUserDetails.PosY = posY Then
                roomUserDetails.removeStatus("sit")
                If ocState(posX, posY) = 2 Then '// Still a seaty, update sitheight + rotation
                    roomUserDetails.addStatus("sit", ocItemHeight(posX, posY))
                    roomUserDetails.rotHead = ocItemRot(posX, posY)
                    roomUserDetails.rotBody = ocItemRot(posX, posY)
                End If
                sendAll("@b" & roomUserDetails.dynamicStatus)
                Return
            End If
        Next
    End Sub
#End Region
#Region "Item management"
    Friend Sub placeItem(ByVal userID As Integer, ByVal placePacket As String)
        Dim itemID As Integer = placePacket.Split(" ")(0)
        Try
            Dim templateID As Integer = HoloDB.runRead("SELECT tid FROM furniture WHERE id = '" & itemID & "' AND ownerid = '" & userID & "' AND roomid = '0'", Nothing)
            If templateID = 0 Then Return '// Not found/not in users hand

            If HoloITEM(templateID).typeID = 0 Then '// Wallitem
                If HoloITEM(templateID).cctName = "roomdimmer" Then If HoloDB.checkExists("SELECT id FROM furniture WHERE roomid = '" & roomID & "' AND tid = '" & templateID & "'") = True Then Return '// Already mood light in room, no more please!

                Dim wallPosition As String = placePacket.Substring(itemID.ToString.Length + 1)
                sendAll("AS" & itemID & Convert.ToChar(9) & HoloITEM(templateID).cctName & Convert.ToChar(9) & " " & Convert.ToChar(9) & wallPosition & Convert.ToChar(9) & HoloITEM(templateID).Colour)
                HoloDB.runQuery("UPDATE furniture SET roomid = '" & roomID & "',opt_wallpos = '" & wallPosition & "' WHERE id = '" & itemID & "' LIMIT 1")

                If HoloITEM(templateID).cctName = "roomdimmer" Then
                    HoloDB.runQuery("UPDATE furniture_moodlight SET roomid = '" & roomID & "' WHERE id = '" & itemID & "' LIMIT 1")
                    refreshWallitem(itemID, "roomdimmer", wallPosition, HoloDB.runRead("SELECT opt_var FROM furniture WHERE id = '" & itemID & "'"))
                End If
            Else '// Floor item
                Dim packetContent() As String = placePacket.Split(" ")
                Dim iX As Integer = packetContent(1)
                Dim iY As Integer = packetContent(2)
                Dim iZ As Integer = packetContent(5)
                Dim iH As Double = ocHeight(iX, iY)

                If Not (HoloITEM(templateID).typeID = 4) Then '// Not a rug
                    Dim iLength, iWidth As Integer

                    iLength = getItemLength(templateID, iZ)
                    iWidth = getItemWidth(templateID, iZ)

                    For tX = iX To iX + iWidth - 1
                        For tY = iY To iY + iLength - 1
                            If Not (ocState(tX, tY) = 1) Then Return
                            If Not (ocHeight(tX, tY) = iH) Then Return
                            If ocUserHere(tX, tY) = True Then Return
                        Next tY
                    Next tX

                    For tX = iX To iX + iWidth - 1
                        For tY = iY To iY + iLength - 1
                            If HoloITEM(templateID).typeID = 2 Then
                                ocState(tX, tY) = 2
                                ocItemHeight(tX, tY) = ocItemHeight(tX, tY) + HoloITEM(templateID).topH
                                ocItemRot(tX, tY) = iZ
                            Else
                                ocState(tX, tY) = 0
                            End If
                        Next tY
                    Next tX
                End If

                Dim newFurniture As New furnitureItem(itemID, templateID, iX, iY, iZ, iH)
                newFurniture.Var = HoloDB.runRead("SELECT opt_var FROM furniture WHERE id = '" & itemID & "'")

                sendAll("A]" & newFurniture.ToString())
                HoloDB.runQuery("UPDATE furniture SET roomid = '" & roomID & "',x = '" & iX & "',y = '" & iY & "',z = '" & iZ & "',h = '" & iH.ToString & "' WHERE id = '" & itemID & "' LIMIT 1")
                furnitureItems.Add(itemID, newFurniture)
            End If
        Catch

        End Try
    End Sub
    Friend Sub removeItem(ByVal userID As Integer, ByVal itemID As Integer)
        Dim templateID As Integer = HoloDB.runRead("SELECT tid FROM furniture WHERE id = '" & itemID & "' AND roomid = '" & roomID & "'", Nothing)
        If templateID = 0 Then Return '// Not found/not in this room

        If HoloITEM(templateID).typeID = 0 Then
            sendAll("AT" & itemID)
            If HoloITEM(templateID).cctName = "roomdimmer" Then HoloDB.runQuery("UPDATE furniture_moodlight SET roomid = '0' WHERE id = '" & itemID & "' LIMIT 1") '// Set moodlight room ID to 0 [if it's a moodlight]
        Else
            sendAll("A^" & itemID)
            Try
                If Not (HoloITEM(templateID).typeID = 4) Then '// Not a rug
                    Dim removingItem As furnitureItem = furnitureItems(itemID)
                    Dim iLength As Integer = getItemLength(templateID, removingItem.Z)
                    Dim iWidth As Integer = getItemWidth(templateID, removingItem.Z)

                    For tX = removingItem.X To removingItem.X + iWidth - 1
                        For tY = removingItem.Y To removingItem.Y + iLength - 1
                            ocState(tX, tY) = 1
                            If HoloITEM(removingItem.tID).typeID = 2 Then refreshSpot(tX, tY) '// Drop seated user
                            ocItemRot(tX, tY) = 0
                            ocItemHeight(tX, tY) = 0.0
                        Next tY
                    Next tX
                    furnitureItems.Remove(itemID)
                End If
            Catch
            End Try
        End If

        If userID = 0 Then
            HoloDB.runQuery("DELETE FROM furniture WHERE id = '" & itemID & "' LIMIT 1")
        Else
            HoloDB.runQuery("UPDATE furniture SET roomid = '0',x = '0',y = '0',z = '0',h = '0',opt_wallpos = NULL WHERE id = '" & itemID & "' LIMIT 1")
        End If
    End Sub
    Friend Sub relocateItem(ByVal itemID As Integer, ByVal newX As Integer, ByVal newY As Integer, ByVal newZ As Integer)
        Try
            Dim targetItem As furnitureItem = furnitureItems(itemID)
            Dim newH As Integer = ocHeight(newX, newY)
            Dim nLength As Integer = getItemLength(targetItem.tID, newZ)
            Dim nWidth As Integer = getItemWidth(targetItem.tID, newZ)

            For X = newX To newX + nWidth - 1
                For Y = newY To newY + nLength - 1
                    If ocUserHere(X, Y) = True Then Return '// Self-explanatory
                    If Not (ocHeight(X, Y) = newH) Then Return '// Put on a stair or something, heightmap is different here, shit
                    'If ocState(X, Y) = 2 Then Return '// Seat here, can't stack/put here
                Next
            Next

            Dim oLength As Integer = getItemLength(targetItem.tID, targetItem.Z)
            Dim oWidth As Integer = getItemWidth(targetItem.tID, targetItem.Z)

            For X = targetItem.X To targetItem.X + oWidth - 1
                For Y = targetItem.Y To targetItem.Y + oLength - 1
                    ocState(X, Y) = 1
                    ocItemHeight(X, Y) = 0.0
                    ocItemRot(X, Y) = 0
                    refreshSpot(X, Y)
                Next
            Next

            '// Update furnitureclass of this item
            targetItem.X = newX
            targetItem.Y = newY
            targetItem.Z = newZ
            targetItem.H = newH
            sendAll("A_" & targetItem.ToString)

            If Not (HoloITEM(targetItem.tID).typeID = 4) Then '// Not a rug
                For X = newX To newX + nWidth - 1
                    For Y = newY To newY + nLength - 1
                        ocState(X, Y) = HoloITEM(targetItem.tID).typeID
                        If ocState(X, Y) = 2 Then
                            ocItemHeight(X, Y) = HoloITEM(targetItem.tID).topH
                            ocItemRot(X, Y) = newZ
                        End If
                    Next
                Next
            End If

            HoloDB.runQuery("UPDATE furniture SET x = '" & newX & "',y = '" & newY & "',z = '" & newZ & "',h = '" & newH & "' WHERE id = '" & itemID & "' AND roomid = '" & roomID & "' LIMIT 1")
        Catch
        End Try
    End Sub
    Friend Function itemInside(ByVal itemID) As Boolean
        Return furnitureItems.ContainsKey(itemID)
    End Function
    Friend Function getItem(ByVal itemID As Integer) As furnitureItem
        If furnitureItems.ContainsKey(itemID) = True Then Return furnitureItems(itemID)
        Return Nothing
    End Function
    Friend Function getItem(ByVal X As Integer, ByVal Y As Integer, ByVal ignoreRugs As Boolean, ByVal ignoreSeats As Boolean, ByVal grabTopItem As Boolean) As furnitureItem
        Dim TOP_ITEM As furnitureItem = Nothing

        For Each furnitureItem As furnitureItem In furnitureItems.Values
            If furnitureItem.X = X And furnitureItem.Y = Y Then
                If ignoreRugs = True Then If HoloITEM(furnitureItem.tID).typeID = 4 Then Continue For
                If ignoreSeats = True Then If HoloITEM(furnitureItem.tID).typeID = 2 Then Continue For
                If grabTopItem = False Then Return furnitureItem

                If IsNothing(TOP_ITEM) = True Then
                    TOP_ITEM = furnitureItem
                Else
                    If furnitureItem.H > TOP_ITEM.H Then TOP_ITEM = furnitureItem
                End If
            End If
        Next

        If grabTopItem = True Then If IsNothing(TOP_ITEM) = False Then Return TOP_ITEM
        Return Nothing
    End Function
    Private Sub refreshWallitem(ByVal itemID As Integer, ByVal cctName As String, ByVal wallPosition As String, ByVal itemVariable As String)
        sendAll("AU" & itemID & Convert.ToChar(9) & cctName & Convert.ToChar(9) & " " & wallPosition & Convert.ToChar(9) & itemVariable)
    End Sub
    Friend Sub signWallitem(ByVal itemID As Integer, ByVal statusID As Integer)
        Dim itemData() As String = HoloDB.runReadRow("SELECT tid,opt_wallpos FROM furniture WHERE id = '" & itemID & "' AND roomid = '" & roomID & "'")
        If itemData.Count = 0 Then Return '// Item not found/not in this room

        refreshWallitem(itemID, HoloITEM(Integer.Parse(itemData(0))).cctName, itemData(1), statusID)
        HoloDB.runQuery("UPDATE furniture SET opt_var = '" & statusID & "' WHERE id = '" & itemID & "' LIMIT 1")
    End Sub
    Friend Sub signItem(ByVal itemID As Integer, ByVal newStatus As String, ByVal hasRights As Boolean)
        If furnitureItems.ContainsKey(itemID) = False Then Return '// This item aint in this room biatch =D
        Dim targetItem As furnitureItem = furnitureItems(itemID)

        If hasRights = True Then '// Quick check if user has rights, no need for checking the furniture class when the user has no rights =D
            If HoloITEM(targetItem.tID).isDoor = True Then  '// Item is a door
                Dim itemLength As Integer = getItemLength(targetItem.tID, targetItem.Z)
                Dim itemWidth As Integer = getItemWidth(targetItem.tID, targetItem.Z)
                If newStatus.ToLower = "c" Then '// Close the door
                    If sqBlocked(targetItem.X, targetItem.Y, itemLength, itemWidth) = True Then Return '// Spot[s] are blocked by something, not closing door...
                    setSqState(targetItem.X, targetItem.Y, itemLength, itemWidth, 0) '// Make the spot[s] non-walkable
                Else '// Open the door
                    setSqState(targetItem.X, targetItem.Y, itemLength, itemWidth, 1) '// Make the spot[s] walkable
                End If
            End If
        End If

        targetItem.Var = newStatus
        sendAll("AX" & itemID & Convert.ToChar(2) & newStatus & Convert.ToChar(2))
        HoloDB.runQuery("UPDATE furniture SET opt_var = '" & newStatus & "' WHERE id = '" & itemID & "' LIMIT 1")
    End Sub
#Region "Habbowheel"
    Friend Sub spinHabbowheel(ByVal itemID As Integer)
        Dim wallPosition As String = HoloDB.runRead("SELECT opt_wallpos FROM furniture WHERE roomid = '" & roomID & "' AND id = '" & itemID & "'")
        If wallPosition = vbNullString Then Return '// Item not found/not in this room

        refreshWallitem(itemID, "habbowheel", wallPosition, "-1")

        Dim v As New Random
        Dim stopAt As Integer = v.Next(0, 10)
        timedPacket("AU" & itemID & Convert.ToChar(9) & "habbowheel" & Convert.ToChar(9) & " " & wallPosition & Convert.ToChar(9) & stopAt, 4250)
    End Sub
#End Region
#Region "Moodlight"
    Friend ReadOnly Property moodLight_GetSettings() As String
        Get
            Try
                Dim itemSettings() As String = HoloDB.runReadRow("SELECT preset_cur,preset_1,preset_2,preset_3 FROM furniture_moodlight WHERE roomid = '" & roomID & "'")
                Dim settingPack As String = HoloENCODING.encodeVL64(3) & HoloENCODING.encodeVL64(itemSettings(0))

                For i = 1 To 3
                    Dim curPresetData() As String = itemSettings(i).Split(",")
                    settingPack += HoloENCODING.encodeVL64(i) & HoloENCODING.encodeVL64(curPresetData(0)) & curPresetData(1) & Convert.ToChar(2) & HoloENCODING.encodeVL64(curPresetData(2))
                Next

                Return settingPack

            Catch
                Return vbNullString

            End Try
        End Get
    End Property
    Friend Sub moodLight_SetSettings(ByVal isEnabled As Boolean, ByVal presetID As Integer, ByVal bgState As Integer, ByVal presetColour As String, ByVal alphaDarkF As Integer)
        Dim itemID As Integer = HoloDB.runRead("SELECT id FROM furniture_moodlight WHERE roomid = '" & roomID & "'", Nothing)
        Dim newPresetValue As String
        If isEnabled = False Then
            Dim curPresetValue As String = HoloDB.runRead("SELECT opt_var FROM furniture WHERE id = '" & itemID & "'")
            If curPresetValue.Substring(0, 1) = "2" Then newPresetValue = "1" & curPresetValue.Substring(1) Else newPresetValue = "2" & curPresetValue.Substring(1)
            HoloDB.runQuery("UPDATE furniture SET opt_var = '" & newPresetValue & "' WHERE id = '" & itemID & "' LIMIT 1")
        Else
            newPresetValue = "2" & "," & presetID & "," & bgState & "," & presetColour & "," & alphaDarkF
            HoloDB.runQuery("UPDATE furniture SET opt_var = '" & newPresetValue & "' WHERE id = '" & itemID & "' LIMIT 1")
            HoloDB.runQuery("UPDATE furniture_moodlight SET preset_cur = '" & presetID & "',preset_" & presetID & " = '" & bgState & "," & presetColour & "," & alphaDarkF & "' WHERE id = '" & itemID & "' LIMIT 1")
        End If
        Dim wallPosition As String = HoloDB.runRead("SELECT opt_wallpos FROM furniture WHERE id = '" & itemID & "'")
        refreshWallitem(itemID, "roomdimmer", wallPosition, newPresetValue)
    End Sub
#End Region
#End Region
#Region "Misc room tasks"
    Friend Sub timedPacket(ByVal strPacket As String, ByVal msInterval As Integer)
        Dim timedAction As New timedPacketHandler(AddressOf delTimedPacket)
        timedAction.BeginInvoke(strPacket, msInterval, Nothing, Nothing)
    End Sub
    Private Sub delTimedPacket(ByVal strPacket As String, ByVal msInterval As Integer)
        Thread.Sleep(msInterval)
        sendAll(strPacket)
    End Sub
    Friend Sub castVote(ByVal userID As Integer, ByVal castedVote As Integer)
        If HoloDB.checkExists("SELECT userid FROM guestroom_votes WHERE userid = '" & userID & "' AND roomid = '" & roomID & "'") = True Then Return
        HoloDB.runQuery("INSERT INTO guestroom_votes (userid,roomid,vote) VALUES ('" & userID & "','" & roomID & "','" & castedVote & "')")
        sendAll("EY" & Votes(0))
    End Sub
    Friend Sub castRoomKick(ByVal myRank As Integer, ByVal strMessage As String)
        For Each roomUserDetails As clsHoloUSERDETAILS In roomUsers.Values
            If roomUserDetails.Rank < myRank Then removeUser(roomUserDetails, True, strMessage)
        Next
    End Sub
#End Region
#Region "Furniture item tasks"
    Public Class furnitureItem
        Friend ID As Integer
        Friend tID As Integer
        Friend X, Y, Z As Integer
        Friend H As Double
        Friend Var As String
        Friend Sub New(ByVal ID As Integer, ByVal tID As Integer, ByVal X As Integer, ByVal Y As Integer, ByVal Z As Integer, ByVal H As Double)
            Me.ID = ID
            Me.tID = tID
            Me.X = X
            Me.Y = Y
            Me.Z = Z
            Me.H = Double.Parse(H)
        End Sub
        Friend Shadows ReadOnly Property ToString()
            Get
                Return ID & Convert.ToChar(2) & HoloITEM(tID).cctName & Convert.ToChar(2) & HoloENCODING.encodeVL64(X) & HoloENCODING.encodeVL64(Y) & HoloENCODING.encodeVL64(HoloITEM(tID).Length) & HoloENCODING.encodeVL64(HoloITEM(tID).Width) & HoloENCODING.encodeVL64(Z) & H.ToString & Convert.ToChar(2) & HoloITEM(tID).Colour & Convert.ToChar(2) & Convert.ToChar(2) & "H" & Var & Convert.ToChar(2)
            End Get
        End Property
    End Class
    Private Function getItemLength(ByVal templateID As Integer, ByVal itemRotation As Integer) As Integer
        If itemRotation = 2 Or itemRotation = 6 Then
            Return HoloITEM(templateID).Length
        Else
            Return HoloITEM(templateID).Width
        End If
    End Function
    Private Function getItemWidth(ByVal templateID As Integer, ByVal itemRotation As Integer) As Integer
        If itemRotation = 2 Or itemRotation = 6 Then
            Return HoloITEM(templateID).Width
        Else
            Return HoloITEM(templateID).Length
        End If
    End Function
    Private Function sqBlocked(ByVal X As Integer, ByVal Y As Integer, ByVal itemLength As Integer, ByVal itemWidth As Integer) As Boolean
        For ocX = X To X + itemWidth - 1
            For ocY = Y To Y + itemLength - 1
                If ocUserHere(ocX, ocY) = True Then Return True
                If Not (ocState(ocX, ocY) = 1) Then Return True
            Next
        Next
        Return False
    End Function
    Private Sub setSqState(ByVal X As Integer, ByVal Y As Integer, ByVal itemLength As Integer, ByVal itemWidth As Byte, ByVal toState As Byte)
        For ocX = X To X + itemWidth - 1
            For ocY = Y To Y + itemLength - 1
                ocState(ocX, ocY) = toState
            Next
        Next
    End Sub
#End Region
End Class
