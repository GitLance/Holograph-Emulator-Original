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

    Private ocState(,) As Byte '// Typestate on X,Y0 = blocked, 1 = open, 2 = seat, 3 = bed, 4 = rug
    Private ocItemRot(,) As Byte '// Rotation of the item on X,Y (only set with seat/bed)
    Private ocHeight(,) As Byte '// Walkeight on X,Y, 255 = blocked by user
    Private ocItemHeight(,) As Double '// Itemheight on X,Y
    Private ocUserHere(,) As Boolean '// User here true/false

    Friend doorX, doorY As Integer
    Private doorH As Double

    Private roomUsers As New Hashtable
    Private roomUsersByName As New Hashtable
    Private ownerID As Integer
    Private walkManager As New Thread(AddressOf manageWalks)
    Private Delegate Sub habbowheelSpinner(ByVal itemID As Integer, ByVal wallPosition As String)
#Region "Generic room tasks"
    Sub New(ByVal thisRoomID As Integer, ByVal isPublic As Boolean)
        roomID = thisRoomID
        isPublicRoom = isPublic
        Dim tempHeightMap() As String

        If isPublicRoom = True Then
            Dim pubDoor() As String = HoloDB.runRead("SELECT map_door FROM publicrooms WHERE id = '" & roomID & "' LIMIT 1").Split(" ")
            doorX = pubDoor(0)
            doorY = pubDoor(1)
            doorH = Double.Parse(pubDoor(2))
            publicRoomHeightmap = HoloDB.runRead("SELECT map_height FROM publicrooms WHERE id = '" & roomID & "' LIMIT 1")
            tempHeightMap = publicRoomHeightmap.Split(sysChar(13))
        Else
            roomModel = HoloDB.runRead("SELECT model FROM guestrooms WHERE id = '" & roomID & "' LIMIT 1")
            doorX = HoloSTATICMODEL(roomModel).doorX
            doorY = HoloSTATICMODEL(roomModel).doorY
            doorH = HoloSTATICMODEL(roomModel).doorH
            tempHeightMap = HoloSTATICMODEL(roomModel).strMap.Split(sysChar(13))
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
            Dim tempItemMap() As String = HoloDB.runRead("SELECT map_items FROM publicrooms WHERE id = '" & roomID & "' LIMIT 1").Split(vbCrLf)
            Dim tempCompletedItemMap As New StringBuilder
            For curItem = 0 To tempItemMap.Count - 1
                Dim curItemStats() As String = tempItemMap(curItem).Split(" ")
                Dim itemX, itemY, itemT As Integer
                itemX = curItemStats(2)
                itemY = curItemStats(3)
                itemT = curItemStats(6)

                If itemT = 1 Then '// Solid! =D
                    ocState(itemX, itemY) = 0
                ElseIf itemT = 2 Then '// Seat! =D
                    ocState(itemX, itemY) = 2
                    ocItemRot(itemX, itemY) = curItemStats(5)
                    ocItemHeight(itemX, itemY) = 1.0
                End If

                tempCompletedItemMap.Append(curItemStats(0) & " " & curItemStats(1) & " " & curItemStats(2) & " " & curItemStats(3) & " " & curItemStats(4) & " " & curItemStats(5) & sysChar(13))
            Next
            publicRoomItems = tempCompletedItemMap.ToString
        Else '// Do the inside furnitures, read them from database, store them into classes etc
            Dim furnitureItem() As String = HoloDB.runReadArray("SELECT id,tid,x,y,z,h,opt_var FROM furniture WHERE roomid = '" & roomID & "' AND opt_wallpos IS NULL", True)
            For i = 0 To furnitureItem.Count - 1
                Dim itemData() As String = furnitureItem(i).Split(sysChar(9))
                Dim newItem As New furnitureItem
                newItem.ID = Integer.Parse(itemData(0))
                newItem.tID = Integer.Parse(itemData(1))
                newItem.X = Integer.Parse(itemData(2))
                newItem.Y = Integer.Parse(itemData(3))
                newItem.Z = Integer.Parse(itemData(4))
                newItem.H = Integer.Parse(itemData(5))
                If Not (itemData(6)) = vbNullString Then newItem.Var = itemData(6)
                furnitureItems.Add(newItem.ID, newItem)

                If Not (HoloITEM(newItem.tID).typeID = 4) Then '// Items modifies the heightmap (so it's not a rug (4))
                    Dim iLength As Integer = getItemLength(newItem.tID, newItem.Z)
                    Dim iWidth As Integer = getItemWidth(newItem.tID, newItem.Z)

                    For tX = newItem.X To newItem.X + iWidth - 1
                        For tY = newItem.Y To newItem.Y + iLength - 1
                            If HoloITEM(newItem.tID).typeID = 2 Then
                                ocState(tX, tY) = 2
                                ocItemHeight(tX, tY) = ocHeight(tX, tY) + HoloITEM(newItem.tID).topH
                                ocItemRot(tX, tY) = newItem.Z
                            Else
                                ocState(tX, tY) = 0
                            End If
                        Next tY
                    Next tX
                End If
            Next
        End If

        ocState(doorX, doorY) = 1 '// Make the door always walkable

        Console.WriteLine("[ROOM] Room " & roomID & " [publicroom: " & isPublicRoom.ToString.ToLower & "] loaded.")
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

                syncStack.Append(ruD.dynamicStatus & sysChar(13))

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
                syncStack.Append(sysChar(1))
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
                refreshPack.Append(roomUser.dynamicStatus & sysChar(13))
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
                Dim furnitureItem As furnitureItem
                itemPack.Append(HoloENCODING.encodeVL64(furnitureItems.Count))

                For Each furnitureItem In furnitureItems.Values
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
                Dim wallItemPack As New StringBuilder
                Dim wallItem() As String = HoloDB.runReadArray("SELECT id,tid,opt_var,opt_wallpos FROM furniture WHERE roomid = '" & roomID & "' AND NOT(opt_wallpos IS NULL)", True)
                For i = 0 To wallItem.Count - 1
                    Dim itemData() As String = wallItem(i).Split(sysChar(9))
                    Dim templateID As Integer = itemData(1)
                    wallItemPack.Append(itemData(0) & sysChar(9) & HoloITEM(templateID).cctName & sysChar(9) & " " & sysChar(9) & itemData(3) & sysChar(9))
                    If itemData(2) = vbNullString Then wallItemPack.Append(HoloITEM(templateID).Colour) Else wallItemPack.Append(itemData(2)) '// If the var is blank, then add the wallitem's 'colour', if it isn't blank, add it's var [var = special variable for item, like light on/off]
                    wallItemPack.Append(sysChar(13))
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
            If userID = 0 OrElse HoloDB.checkExists("SELECT userid FROM guestroom_votes WHERE userid = '" & userID & "' AND roomid = '" & roomID & "' LIMIT 1") = True Then
                Dim voteSum As Integer = HoloDB.runRead("SELECT SUM(vote) FROM guestroom_votes WHERE roomid = '" & roomID & "'")
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
                listBuilder.Append(roomUserDetails.Name & sysChar(2))
            Next
            Return listBuilder.ToString
        End Get
    End Property
#End Region
#Region "User management"
    Friend Sub addUser(ByRef newUser As clsHoloUSERDETAILS)
        If newUser.isAllowedInRoom = False Then newUser.userClass.roomCommunicator = Nothing : newUser.Reset() : newUser.userClass.transData("@R" & sysChar(1)) : Return '// How in hells name did you got here? You haven't been autorised! Gtfo! Tubgirl!!11

        If roomUsers.Count = 0 Then walkManager.Start()
        If roomUsers.ContainsValue(newUser) = False Then
            Dim i As Integer
            While (True)
                If roomUsers.ContainsKey(i) = False Then '// This room identifier is not token yet
                    roomUsers.Add(i, newUser) '// Add the userclass together with the found free room identifier to the hashtable roomUsers
                    newUser.roomUID = i '// Set the users room identifier
                    Exit While
                End If
                i += 1
            End While
            '// Add to the list with names
            roomUsersByName.Add(newUser.Name, newUser)
        End If

        '// Set user's position matching the door of this room
        newUser.PosX = doorX
        newUser.PosY = doorY
        newUser.PosH = doorH

        sendAll("@\" & newUser.ToString & sysChar(1)) '// Make user appear in room


        '// Update room inside count
        HoloMANAGERS.updateRoomInsideCount(roomID, isPublicRoom, roomUsers.Count)
    End Sub
    Friend Sub removeUser(ByVal leavingUser As clsHoloUSERDETAILS, ByVal sendKick As Boolean, Optional ByVal kickMessage As String = vbNullString)
        If sendKick = True Then leavingUser.userClass.transData("@R" & sysChar(1))
        If Not (kickMessage = vbNullString) Then leavingUser.userClass.transData("@amod_warn/" & kickMessage & sysChar(1))

        If roomUsers.Count > 1 Then '// If there are more than just this user in the room
            roomUsers.Remove(leavingUser.roomUID)
            roomUsersByName.Remove(leavingUser.Name)
            ocUserHere(leavingUser.PosX, leavingUser.PosY) = False

            sendAll("@]" & leavingUser.roomUID & sysChar(1)) '// Send the 'make user disappear' packet to the room
            HoloMANAGERS.updateRoomInsideCount(roomID, isPublicRoom, roomUsers.Count) '// Update the inside count
        Else '// Last user leaves the room, start destroying this class
            HoloMANAGERS.updateRoomInsideCount(roomID, isPublicRoom, 0)

            HoloMANAGERS.hookedRooms.Remove(roomID) '// Remove roomclass from hashtable
            Console.WriteLine("[ROOM] Room " & roomID & " [publicroom: " & isPublicRoom.ToString.ToLower & "] was left by last user, resources destroyed.")
            walkManager.Abort()
        End If

        leavingUser.Reset()
        leavingUser.userClass.roomCommunicator = Nothing
    End Sub
    Private Sub refreshSpot(ByVal posX As Integer, ByVal posY As Integer)
        For Each roomUserDetails As clsHoloUSERDETAILS In roomUsers.Values
            If roomUserDetails.PosX = posX And roomUserDetails.PosY = posY Then
                roomUserDetails.removeStatus("sit")
                If ocState(posX, posY) = 2 Then '// Still a seaty, update sitheight + rotation
                    roomUserDetails.addStatus("sit", ocItemHeight(posX, posY))
                    roomUserDetails.rotHead = ocItemRot(posX, posY)
                    roomUserDetails.rotBody = ocItemRot(posX, posY)
                End If
                sendAll("@b" & roomUserDetails.dynamicStatus & sysChar(1))
                Return
            End If
        Next
    End Sub
    Friend Sub refreshUser(ByRef userDetails As clsHoloUSERDETAILS)
        sendAll("@b" & userDetails.dynamicStatus & sysChar(1))
    End Sub
    Friend Sub kickUser(ByVal kickTarget As String, ByVal myRank As Integer, Optional ByVal kickReason As String = vbNullString)
        If roomUsersByName.ContainsKey(kickTarget) = False Then Return
        Dim kickUser As clsHoloUSERDETAILS = roomUsersByName(kickTarget)

        If kickUser.isOwner = True Then If HoloRANK(myRank).containsRight("fuse_any_room_controller") = False Then Return '// If you are trying to kick the room owner (so also staff!) and you don't have the 'fuse_any_room_controller' fuse right, then you can't kick [so: room owner can't kick staff ;P]
        If kickReason = vbNullString Then '// Plain room kick
            kickUser.walkLock = True
            kickUser.DestX = doorX
            kickUser.DestY = doorY
        Else '// Kicked by staff
            removeUser(kickUser, True)
            kickUser.userClass.transData("@amod_warn/" & kickReason & sysChar(1)) '// Kicked by staffmember, send the reason
        End If
    End Sub
    Friend Sub modRights(ByVal withUserName As String, ByVal addInsteadOfRemove As Boolean)
        Try
            If roomUsersByName.ContainsKey(withUserName) = False Then Return
            Dim withUserDetails As clsHoloUSERDETAILS = roomUsersByName(withUserName)
            If addInsteadOfRemove = True Then
                If HoloDB.checkExists("SELECT userid FROM guestroom_rights WHERE userid = '" & withUserDetails.UserID & "' AND roomid = '" & roomID & "' LIMIT 1") = True Then Return '// User already has rights
                HoloDB.runQuery("INSERT INTO guestroom_rights (userid,roomid) VALUES ('" & withUserDetails.UserID & "','" & roomID & "')")

                withUserDetails.hasRights = True
                withUserDetails.userClass.transData("@j" & sysChar(1))
                withUserDetails.addStatus("flatctrl", "onlyfurniture")
            Else
                HoloDB.runQuery("DELETE FROM guestroom_rights WHERE userid = '" & withUserDetails.UserID & "' AND roomid = '" & roomID & "' LIMIT 1")
                withUserDetails.hasRights = False
                withUserDetails.userClass.transData("@k" & sysChar(1))
                withUserDetails.removeStatus("flatctrl")
            End If

            refreshUser(withUserDetails)
        Catch
        End Try
    End Sub
    Friend Sub doChat(ByRef userDetails As clsHoloUSERDETAILS, ByVal talkType As Char, ByVal talkMessage As String)
        sendAll("Ei" & HoloENCODING.encodeVL64(userDetails.roomUID) & "H" & sysChar(1) & "@" & talkType & HoloENCODING.encodeVL64(userDetails.roomUID) & talkMessage & sysChar(2) & sysChar(1))
        'If HoloRACK.Chat_doHeadTilt = True Then '// If the server user (so: you!) has chosen to use the head tilt animation at chatting
        'Dim toX As Integer = userClass.userDetails.PosX
        'Dim toY As Integer = userClass.userDetails.PosY
        '// Head tilt shiz
        'End If
    End Sub
    Friend Function roomUserDetails(ByVal roomUID As Integer) As clsHoloUSERDETAILS
        If roomUsers.ContainsKey(roomUID) = False Then Return Nothing
        Return roomUsers(roomUID)
    End Function
    Friend Function roomUserDetails(ByVal userName As String) As clsHoloUSERDETAILS
        If roomUsersByName.ContainsKey(userName) = False Then Return Nothing
        Return roomUsers(userName)
    End Function
#End Region
#Region "Item management"
    Friend Sub placeItem(ByVal userID As Integer, ByVal placePacket As String)
        Dim itemID As Integer = placePacket.Split(" ")(0)
        Try
            Dim templateID As Integer = HoloDB.runRead("SELECT tid FROM furniture WHERE id = '" & itemID & "' AND inhand = '" & userID & "' LIMIT 1")
            If templateID = 0 Then Return '// Not found/not in users hand

            If HoloITEM(templateID).typeID = 0 Then '// Wallitem
                If HoloITEM(templateID).cctName = "roomdimmer" Then If HoloDB.checkExists("SELECT id FROM furniture WHERE roomid = '" & roomID & "' AND tid = '" & templateID & "' LIMIT 1") = True Then Return '// Already mood light in room, no more please!

                Dim wallPosition As String = placePacket.Substring(itemID.ToString.Length + 1)
                sendAll("Ac" & itemID & sysChar(1) & "AS" & itemID & sysChar(9) & HoloITEM(templateID).cctName & sysChar(9) & " " & sysChar(9) & wallPosition & sysChar(9) & HoloITEM(templateID).Colour & sysChar(1))
                HoloDB.runQuery("UPDATE furniture SET roomid = '" & roomID & "',inhand = '0',opt_wallpos = '" & wallPosition & "' WHERE id = '" & itemID & "' LIMIT 1")

                If HoloITEM(templateID).cctName = "roomdimmer" Then
                    HoloDB.runQuery("UPDATE furniture_moodlight SET roomid = '" & roomID & "' WHERE id = '" & itemID & "' LIMIT 1")
                    refreshWallitem(itemID, "roomdimmer", wallPosition, HoloDB.runRead("SELECT opt_var FROM furniture WHERE id = '" & itemID & "' LIMIT 1"))
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

                Dim newFurniture As New furnitureItem
                newFurniture.ID = itemID
                newFurniture.tID = templateID
                newFurniture.X = iX
                newFurniture.Y = iY
                newFurniture.Z = iZ
                newFurniture.H = iH
                newFurniture.Var = HoloDB.runRead("SELECT opt_var FROM furniture WHERE id = '" & itemID & "' LIMIT 1")

                sendAll("A]" & itemID & sysChar(2) & HoloITEM(templateID).cctName & sysChar(2) & HoloENCODING.encodeVL64(iX) & HoloENCODING.encodeVL64(iY) & HoloENCODING.encodeVL64(HoloITEM(templateID).Length) & HoloENCODING.encodeVL64(HoloITEM(templateID).Width) & HoloENCODING.encodeVL64(iZ) & iH.ToString & sysChar(2) & HoloITEM(templateID).Colour & sysChar(2) & sysChar(2) & newFurniture.Var.ToString & sysChar(1))
                HoloDB.runQuery("UPDATE furniture SET roomid = '" & roomID & "',inhand = '0',x = '" & iX & "',y = '" & iY & "',z = '" & iZ & "',h = '" & iH.ToString & "' WHERE id = '" & itemID & "' LIMIT 1")
                furnitureItems.Add(itemID, newFurniture)
            End If
        Catch

        End Try
    End Sub
    Friend Sub removeItem(ByVal userID As Integer, ByVal itemID As Integer)
        Dim templateID As Integer = HoloDB.runRead("SELECT tid FROM furniture WHERE id = '" & itemID & "' AND roomid = '" & roomID & "' LIMIT 1")
        If templateID = 0 Then Return '// Not found/not in this room

        If HoloITEM(templateID).typeID = 0 Then
            sendAll("AT" & itemID & sysChar(1))
            If HoloITEM(templateID).cctName = "roomdimmer" Then HoloDB.runQuery("UPDATE furniture_moodlight SET roomid = '0' WHERE id = '" & itemID & "' LIMIT 1") '// Set moodlight room ID to 0 [if it's a moodlight]
        Else
            sendAll("A^" & itemID & sysChar(1))
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
            HoloDB.runQuery("UPDATE furniture SET roomid = '0',x = '0',y = '0',z = '0',h = '0',opt_wallpos = NULL,inhand = '" & userID & "' WHERE id = '" & itemID & "' LIMIT 1")
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
                    If ocUserHere(X, Y) = True Then If Not (HoloITEM(targetItem.tID).typeID = 2) Then Return '// Self-explanatory
                    If Not (ocHeight(X, Y) = newH) Then Return '// Put on a stair or something
                    'If Not (ocState(X, Y) = 1) Then
                    '
                    'End If
                    'Return '// Square in use
                Next
            Next

            Dim oLength As Integer = getItemLength(targetItem.tID, targetItem.Z)
            Dim oWidth As Integer = getItemWidth(targetItem.tID, targetItem.Z)

            For X = targetItem.X To targetItem.X + oWidth - 1
                For Y = targetItem.Y To targetItem.Y + oLength - 1
                    ocState(X, Y) = 1
                    ocItemHeight(X, Y) = 0.0
                    ocItemRot(X, Y) = 0
                Next
            Next

            If Not (HoloITEM(targetItem.tID).typeID = 4) Then '// Not a rug
                For X = newX To newX + nWidth - 1
                    For Y = newY To newY + nLength - 1
                        ocState(X, Y) = HoloITEM(targetItem.tID).typeID
                        If ocState(X, Y) = 2 Then
                            ocItemHeight(X, Y) = HoloITEM(targetItem.tID).topH
                            ocItemRot(X, Y) = newZ
                            refreshSpot(X, Y)
                        End If
                    Next
                Next
            End If

            '// Update furnitureclass of this item
            targetItem.X = newX
            targetItem.Y = newY
            targetItem.Z = newZ
            targetItem.H = newH
            sendAll("A_" & targetItem.ToString & sysChar(1))
            'sendAll("A_" & itemID & sysChar(2) & HoloITEM(targetItem.tID).cctName & sysChar(2) & HoloENCODING.encodeVL64(newX) & HoloENCODING.encodeVL64(newY) & HoloENCODING.encodeVL64(HoloITEM(targetItem.tID).Length) & HoloENCODING.encodeVL64(HoloITEM(targetItem.tID).Width) & HoloENCODING.encodeVL64(newZ) & newH.ToString & sysChar(2) & HoloITEM(templateID).Colour & sysChar(2) & sysChar(2) & "H" & targetItem.Var & sysChar(2) & sysChar(1))

            HoloDB.runQuery("UPDATE furniture SET x = '" & newX & "',y = '" & newY & "',z = '" & newZ & "',h = '" & newH & "' WHERE id = '" & itemID & "' AND roomid = '" & roomID & "' LIMIT 1")
        Catch
        End Try
    End Sub
    Private Sub refreshWallitem(ByVal itemID As Integer, ByVal cctName As String, ByVal wallPosition As String, ByVal itemVariable As String)
        sendAll("AU" & itemID & sysChar(9) & cctName & sysChar(9) & " " & wallPosition & sysChar(9) & itemVariable & sysChar(1))
    End Sub
    Friend Sub signWallitem(ByVal itemID As Integer, ByVal statusID As Integer)
        Dim itemData() As String = HoloDB.runReadArray("SELECT tid,opt_wallpos FROM furniture WHERE id = '" & itemID & "' AND roomid = '" & roomID & "' LIMIT 1")
        If itemData.Count = 0 Then Return '// Item not found/not in this room

        refreshWallitem(itemID, HoloITEM(Integer.Parse(itemData(0))).cctName, itemData(1), statusID)
        HoloDB.runQuery("UPDATE furniture SET opt_var = '" & statusID & "' WHERE id = '" & itemID & "' LIMIT 1")
    End Sub
#Region "Habbowheel"
    Friend Sub spinHabbowheel(ByVal itemID As Integer)
        Dim wallPosition As String = HoloDB.runRead("SELECT opt_wallpos FROM furniture WHERE roomid = '" & roomID & "' AND id = '" & itemID & "' LIMIT 1")
        If wallPosition = vbNullString Then Return '// Item not found/not in this room

        refreshWallitem(itemID, "habbowheel", wallPosition, "-1")
        Dim spinAction As New habbowheelSpinner(AddressOf spinHabbowheel_Finish)
        spinAction.BeginInvoke(itemID, wallPosition, Nothing, Nothing)
    End Sub
    Private Sub spinHabbowheel_Finish(ByVal itemID As Integer, ByVal wallPosition As String)
        Thread.Sleep(4250)

        Dim stopAt As New Integer
        Dim v As New Random
        stopAt = v.Next(0, 10)
        refreshWallitem(itemID, "habbowheel", wallPosition, stopAt)
    End Sub
#End Region
#Region "Moodlight"
    Friend ReadOnly Property moodLight_GetSettings() As String
        Get
            Try
                Dim itemSettings() As String = HoloDB.runReadArray("SELECT preset_cur,preset_1,preset_2,preset_3 FROM furniture_moodlight WHERE roomid = '" & roomID & "' LIMIT 1")
                Dim settingPack As String = HoloENCODING.encodeVL64(3) & HoloENCODING.encodeVL64(itemSettings(0))

                For i = 1 To 3
                    Dim curPresetData() As String = itemSettings(i).Split(",")
                    settingPack += HoloENCODING.encodeVL64(i) & HoloENCODING.encodeVL64(curPresetData(0)) & curPresetData(1) & sysChar(2) & HoloENCODING.encodeVL64(curPresetData(2))
                Next

                Return settingPack

            Catch
                Return vbNullString

            End Try
        End Get
    End Property
    Friend Sub moodLight_SetSettings(ByVal isEnabled As Boolean, ByVal presetID As Integer, ByVal bgState As Integer, ByVal presetColour As String, ByVal alphaDarkF As Integer)
        Dim itemID As Integer = HoloDB.runRead("SELECT id FROM furniture_moodlight WHERE roomid = '" & roomID & "' LIMIT 1")
        Dim newPresetValue As String
        If isEnabled = False Then
            Dim curPresetValue As String = HoloDB.runRead("SELECT opt_var FROM furniture WHERE id = '" & itemID & "' LIMIT 1")
            If curPresetValue.Substring(0, 1) = "2" Then newPresetValue = "1" & curPresetValue.Substring(1) Else newPresetValue = "2" & curPresetValue.Substring(1)
            HoloDB.runQuery("UPDATE furniture SET opt_var = '" & newPresetValue & "' WHERE id = '" & itemID & "' LIMIT 1")
        Else
            newPresetValue = "2" & "," & presetID & "," & bgState & "," & presetColour & "," & alphaDarkF
            HoloDB.runQuery("UPDATE furniture SET opt_var = '" & newPresetValue & "' WHERE id = '" & itemID & "' LIMIT 1")
            HoloDB.runQuery("UPDATE furniture_moodlight SET preset_cur = '" & presetID & "',preset_" & presetID & " = '" & bgState & "," & presetColour & "," & alphaDarkF & "' WHERE id = '" & itemID & "' LIMIT 1")
        End If
        Dim wallPosition As String = HoloDB.runRead("SELECT opt_wallpos FROM furniture WHERE id = '" & itemID & "' LIMIT 1")
        refreshWallitem(itemID, "roomdimmer", wallPosition, newPresetValue)
    End Sub
#End Region
    Private ReadOnly Property getItemLength(ByVal templateID As Integer, ByVal itemRotation As Integer)
        Get
            If itemRotation = 2 Or itemRotation = 6 Then
                Return HoloITEM(templateID).Length
            Else
                Return HoloITEM(templateID).Width
            End If
        End Get
    End Property
    Private ReadOnly Property getItemWidth(ByVal templateID As Integer, ByVal itemRotation As Integer)
        Get
            If itemRotation = 2 Or itemRotation = 6 Then
                Return HoloITEM(templateID).Width
            Else
                Return HoloITEM(templateID).Length
            End If
        End Get
    End Property
#End Region
#Region "Misc room tasks"
    Friend Sub castVote(ByVal userID As Integer, ByVal castedVote As Integer)
        If HoloDB.checkExists("SELECT userid FROM guestroom_votes WHERE userid = '" & userID & "' AND roomid = '" & roomID & "' LIMIT 1") = True Then Return
        HoloDB.runQuery("INSERT INTO guestroom_votes (userid,roomid,vote) VALUES ('" & userID & "','" & roomID & "','" & castedVote & "')")
        sendAll("EY" & Votes(0) & sysChar(1))
    End Sub
#End Region
#Region "Furniture item tasks"
    Private Class furnitureItem
        Friend ID As Integer
        Friend tID As Integer
        Friend X, Y, Z As Integer
        Friend H As Double
        Friend Var As String
        Friend Shadows ReadOnly Property ToString()
            Get
                Return ID & sysChar(2) & HoloITEM(tID).cctName & sysChar(2) & HoloENCODING.encodeVL64(X) & HoloENCODING.encodeVL64(Y) & HoloENCODING.encodeVL64(HoloITEM(tID).Length) & HoloENCODING.encodeVL64(HoloITEM(tID).Width) & HoloENCODING.encodeVL64(Z) & H.ToString & sysChar(2) & HoloITEM(tID).Colour & sysChar(2) & sysChar(2) & "H" & Var & sysChar(2)
            End Get
        End Property
    End Class
#End Region
End Class
