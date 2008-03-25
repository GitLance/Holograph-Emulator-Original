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
    '// Actions
    Friend rotBody As Integer
    Friend rotHead As Integer
    Friend isWalking As Boolean
    Friend isSitting As Boolean
    Private carrydItem As String
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
        On Error Resume Next '// The itemCarrier could be null, we don't want an error message then
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
        isWalking = False
        isSitting = False
        carrydItem = vbNullString
        inBBLobby = False
        Game_owns = False
        Game_ID = -1
        Game_withState = -1

        Statuses = New Hashtable
        If itemCarrier.IsAlive = True Then itemCarrier.Abort()
    End Sub
#End Region
#Region "Status management"
    Friend Function containsStatus(ByVal actionKey As String) As Boolean
        Return Statuses.Contains(actionKey)
    End Function
    Friend Function getStatuses() As String
        Dim actionKey, myStatuses As String
        For Each actionKey In Statuses.Keys
            myStatuses += actionKey
            If Not (Statuses(actionKey) = vbNullString) Then myStatuses += " " & Statuses(actionKey)
            myStatuses += "/"
        Next
        Return myStatuses
    End Function
    Friend Sub addStatus(ByVal actionKey As String, ByVal actionValue As String)
        If Statuses.ContainsKey(actionKey) Then Statuses.Remove(actionKey)
        Statuses.Add(actionKey, actionValue)
    End Sub
    Friend Sub removeStatus(ByVal actionKey As String)
        If Statuses.ContainsKey(actionKey) Then Statuses.Remove(actionKey)
    End Sub
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
        If isSitting = True Then Return
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
        userClass.Refresh()
    End Sub
    Friend Sub CarryItem(ByVal itemToCarry As String)
        If IsNumeric(itemToCarry) = True Then
            If itemToCarry <= 0 Or itemToCarry > 25 Then Return '// 'Look mom I got an invisible item!', get a life son, really
        Else
            If Not (itemToCarry = "Water" Or itemToCarry = "Milk" Or itemToCarry = "Juice") Then Return '// If it's not a numeric item, but also not a drink you can get in the Infobus, then it's a kid without a life. Stop here kthx
        End If
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
    Private Sub handleStatus(ByVal actionKey As String, ByVal actionValue As String, ByVal actionLength As Integer)
        If Statuses.ContainsKey(actionKey) Then Return
        Statuses.Add(actionKey, actionValue)
        userClass.Refresh() '// Send updated @b packet that makes user perform ACTIONKEY in room
        Thread.Sleep(actionLength) '// Wait...
        Statuses.Remove(actionKey)
        userClass.Refresh() '// Send updated @b packet again, but now the /ACTIONKEY/ is removed
    End Sub
#End Region
End Class
