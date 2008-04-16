Imports System.Collections
Imports System.Threading
Public Class clsHoloUSERDETAILS
    Friend userClass As clsHoloUSER
    '// Personal
    Friend UserID As Integer
    Friend Name As String
    Friend Mission As String
    Friend Sex As Char
    Friend Figure As String
    Friend consoleMission As String
    Friend Rank As Byte
    '// Special personal additions
    Friend nowBadge As String
    Friend clubMember As Boolean
    '// Room
    Friend roomUID As Integer '// The users number inside the room
    Friend roomID As Integer '// The ID of the room the user is in
    Friend inPublicroom As Boolean '// The room the user is in is publicroom yes/no
    Friend isAllowedInRoom As Boolean '// The user is allowed as user in the room, if this status is still false when the user is about to appear in room, he's kicked because he skipped the password/doorbell check or w/e
    Friend isOwner, hasRights As Boolean
    '// Walking
    Friend PosX As Integer
    Friend PosY As Integer
    Friend PosH As Double
    Friend DestX As Integer
    Friend DestY As Integer
    Friend walkLock As Boolean
    '// Actions
    Friend rotBody As Integer
    Friend rotHead As Integer
    Private carrydItem As String
    '// Trading
    Friend tradePartnerUID As Integer
    Friend tradeAccept As Boolean
    Friend tradeItems(100) As Integer '// People who trade more have aids
    Friend tradeItemCount As Integer
    '// Games
    Friend inBBLobby As Boolean '// User is in BattleBall lobby yes/no
    Friend Game_owns As Boolean '// User owns a BattleBall game atm
    Friend Game_ID As Integer '// The game the user is involved with
    Friend Game_withState As Integer '// 0=> = team ID, -2 = viewing, -3 = spectator

    Private Statuses As Hashtable '// Contains statuses like /dance/, /sit/ etc
    Private Delegate Sub actionThread(ByVal actionKey, ByVal actionValue, ByVal actionLength)
    Private itemCarrier As Thread
#Region "Class controls"
    Sub New(ByVal userClass As clsHoloUSER)
        Me.userClass = userClass
        Statuses = New Hashtable
    End Sub
    Sub Reset()
        roomUID = 0
        roomID = 0
        inPublicroom = False
        isAllowedInRoom = False
        isOwner = False
        hasRights = False
        PosX = 0
        PosY = 0
        PosH = 0.0
        DestX = -1
        DestY = -1
        rotBody = 0
        rotHead = 0
        walkLock = False
        carrydItem = vbNullString

        tradePartnerUID = 0
        tradeItems = New Integer(100) {}
        tradeItemCount = 0
        tradeAccept = False

        inBBLobby = False
        Game_owns = False
        Game_ID = -1
        Game_withState = -1

        Statuses = New Hashtable
        dropItem()
    End Sub
#End Region
#Region "Status management"
    Friend Sub addStatus(ByVal actionKey As String, ByVal actionValue As String)
        If Statuses.ContainsKey(actionKey) Then Statuses.Remove(actionKey)
        Statuses.Add(actionKey, actionValue)
    End Sub
    Friend Sub removeStatus(ByVal actionKey As String)
        If Statuses.ContainsKey(actionKey) Then Statuses.Remove(actionKey)
    End Sub
    Friend ReadOnly Property containsStatus(ByVal actionKey As String) As Boolean
        Get
            Return Statuses.Contains(actionKey)
        End Get
    End Property
    Private Sub carryItemLoop()
        For i = 1 To 5 '// Edit this to ya wishes
            addStatus("carryd", carrydItem) '// Start carrying this item
            userClass.Refresh() '// Make the drink appear in room
            Thread.Sleep(9000) '// Keep carrying this item for 8 seconds

            Statuses.Remove("carryd") '// Stop carrying
            addStatus("drink", carrydItem) '// Add drinking animation
            userClass.Refresh() '// Make your drinking animation appear
            Thread.Sleep(1000) '// Keep drinking animation for 2 seconds

            Statuses.Remove("drink") '// Stop drinking animation
        Next
        carrydItem = vbNullString
        userClass.Refresh()
    End Sub
#End Region
#Region "Statuses"
    Friend Sub Wave()
        If Statuses.ContainsKey("wave") Then Return '// If user waves already, why waving again?
        If Statuses.ContainsKey("dance") Then Statuses.Remove("dance") '// Remove all types of dances
        Dim waveAction As New actionThread(AddressOf handleStatus)
        waveAction.BeginInvoke("wave", vbNullString, 1500, Nothing, Nothing)
    End Sub
    Friend Sub Dance(ByVal danceID As Integer)
        If Statuses.ContainsKey("sit") = True Then Return
        If Statuses.ContainsKey("dance") = True Then Statuses.Remove("dance")
        If danceID = 0 Then
            Statuses.Add("dance", vbNullString)
        Else
            If danceID > 4 Then Return
            If HoloRANK(Rank).containsRight("fuse_use_club_dance") = True Then
                Statuses.Add("dance", danceID)
            Else
                Statuses.Add("dance", vbNullString)
            End If
        End If
        dropItem()
        userClass.Refresh()
    End Sub
    Friend Sub CarryItem(ByVal itemToCarry As String)
        If IsNumeric(itemToCarry) = True Then
            If itemToCarry <= 0 Or itemToCarry > 25 Then Return '// 'Look mom I got an invisible item!', get a life son, really
        Else
            If Not (itemToCarry = "Water" Or itemToCarry = "Milk" Or itemToCarry = "Juice") Then Return '// If it's not a numeric item, but also not a drink you can get in the Infobus, then it's a kid without a life. Stop here kthx
        End If
        dropItem()
        removeStatus("dance")
        carrydItem = itemToCarry
        itemCarrier = New Thread(AddressOf carryItemLoop)
        itemCarrier.Priority = ThreadPriority.Lowest
        itemCarrier.Start()
    End Sub
    Friend Sub showTalkAnimation(ByVal talkTime As Integer, ByVal talkGesture As String)
        Dim talkAction As New actionThread(AddressOf handleStatus)
        talkAction.BeginInvoke("talk", vbNullString, talkTime, Nothing, Nothing)
        If Not (talkGesture = vbNullString) Then showGesture(talkGesture, talkTime + 3000)
    End Sub
    Friend Sub showGesture(ByVal theGesture As String, ByVal timeToShow As Integer)
        Dim gestAction As New actionThread(AddressOf handleStatus)
        gestAction.BeginInvoke("gest", theGesture, timeToShow, Nothing, Nothing)
    End Sub
    Friend Sub showLidoVote(ByVal voteID As Integer)
        If Statuses.ContainsKey("sign") Then Return
        If Statuses.ContainsKey("wave") Then Statuses.Remove("wave")
        If Statuses.ContainsKey("dance") Then Statuses.Remove("dance")

        Dim voteAction As New actionThread(AddressOf handleStatus)
        voteAction.BeginInvoke("sign", voteID, 1500, Nothing, Nothing)
    End Sub
#End Region
#Region "Misc status handlers"
    Friend Shadows ReadOnly Property ToString()
        Get
            Dim detailsStack As String = "i:" & roomUID & Convert.ToChar(13) & _
            "n:" & Name & Convert.ToChar(13) & _
            "f:" & Figure & Convert.ToChar(13) & _
            "s:" & Sex & Convert.ToChar(13) & _
            "l:" & PosX & " " & PosY & " " & PosH & Convert.ToChar(13)
            If Not (Mission = vbNullString) Then detailsStack += "c:" & Mission & Convert.ToChar(13)
            If Not (nowBadge = vbNullString) Then detailsStack += "b:" & nowBadge & Convert.ToChar(13)

            Return detailsStack
        End Get
    End Property
    Friend ReadOnly Property dynamicStatus() As String
        Get
            Dim myStatuses As String = vbNullString
            For Each actionKey As String In Statuses.Keys
                myStatuses += actionKey
                If Not (Statuses(actionKey) = vbNullString) Then myStatuses += " " & Statuses(actionKey)
                myStatuses += "/"
            Next
            Return roomUID & " " & PosX & "," & PosY & "," & PosH & "," & rotHead & "," & rotBody & "/" & myStatuses
        End Get
    End Property
    Private Sub handleStatus(ByVal actionKey As String, ByVal actionValue As String, ByVal actionLength As Integer)
        If Statuses.ContainsKey(actionKey) Then Return
        Statuses.Add(actionKey, actionValue)
        userClass.Refresh() '// Send updated @b packet that makes user perform ACTIONKEY in room
        Thread.Sleep(actionLength) '// Wait...
        Statuses.Remove(actionKey)
        userClass.Refresh() '// Send updated @b packet again, but now the /ACTIONKEY/ is removed
    End Sub
    Friend Sub dropItem()
        On Error Resume Next
        If itemCarrier.IsAlive = True Then itemCarrier.Abort()
        Statuses.Remove("carryd")
        Statuses.Remove("drink")
        carrydItem = vbNullString
    End Sub
    Friend Sub refreshTradeBoxes()
        Dim partnerClass As clsHoloUSERDETAILS = userClass.roomCommunicator.roomUserDetails(tradePartnerUID)
        If IsNothing(partnerClass) Then Return

        Dim refreshPack As New System.Text.StringBuilder("Al" & Name & Convert.ToChar(9) & tradeAccept.ToString.ToLower & Convert.ToChar(9))
        If tradeItemCount > 0 Then refreshPack.Append(createItemList(tradeItems))
        refreshPack.Append(Convert.ToChar(13) & partnerClass.Name & Convert.ToChar(9) & partnerClass.tradeAccept.ToString.ToLower & Convert.ToChar(9))
        If partnerClass.tradeItemCount > 0 Then refreshPack.Append(createItemList(partnerClass.tradeItems))
        userClass.transData(refreshPack.ToString)
    End Sub
    Friend Sub abortTrade()
        Dim partnerClass As clsHoloUSERDETAILS = userClass.roomCommunicator.roomUserDetails(tradePartnerUID)
        If IsNothing(partnerClass) Then Return

        '// Reset this user [the one who cancels the trade or finishes it]
        tradePartnerUID = -1
        tradeAccept = False
        tradeItems = New Integer(100) {}
        tradeItemCount = 0
        removeStatus("trd")
        Me.userClass.transData("An")

        '// Reset the other trade partner
        partnerClass.tradePartnerUID = -1
        partnerClass.tradeAccept = False
        partnerClass.tradeItems = New Integer(100) {}
        partnerClass.tradeItemCount = 0
        partnerClass.removeStatus("trd")
        partnerClass.userClass.transData("An")

        '// Refresh the room status so the buttons appear again
        Me.userClass.Refresh()
        partnerClass.userClass.Refresh()
    End Sub
    Private Function createItemList(ByVal itemIDs() As Integer) As String
        Dim itemList As New System.Text.StringBuilder
        For i = 0 To itemIDs.Count - 1
            If itemIDs(i) = 0 Then Continue For '// Empty slot
            Dim templateID As Integer = HoloDB.runRead("SELECT tid FROM furniture WHERE id = '" & itemIDs(i) & "' LIMIT 1")

            itemList.Append("SI" & Convert.ToChar(30) & itemIDs(i) & Convert.ToChar(30) & i & Convert.ToChar(30))
            If HoloITEM(templateID).typeID = 0 Then itemList.Append("I") Else itemList.Append("S")
            itemList.Append(Convert.ToChar(30) & itemIDs(i) & Convert.ToChar(30) & HoloITEM(templateID).cctName & Convert.ToChar(30))
            If HoloITEM(templateID).typeID > 0 Then itemList.Append(HoloITEM(templateID).Length & Convert.ToChar(30) & HoloITEM(templateID).Width & Convert.ToChar(30) & HoloDB.runRead("SELECT opt_var FROM furniture WHERE id = '" & itemIDs(i) & "' LIMIT 1") & Convert.ToChar(30))
            itemList.Append(HoloITEM(templateID).Colour & Convert.ToChar(30) & i & Convert.ToChar(30) & "/")
        Next
        Return itemList.ToString
    End Function
#End Region
End Class
