Public Class clsHoloROOMBOT
    Friend roomUID As Integer
    Friend Name As String
    Friend Mission As String
    Friend Figure As String

    Friend PosX, PosY, PosH As Integer
    Friend DestX, DestY As Integer
    Friend rotHead, rotBody As Byte

    Private roomCommunicator As clsHoloROOM
    Private Statuses As Hashtable '// Contains statuses like /dance/ etc
    Private Delegate Sub actionThread(ByVal actionKey, ByVal actionValue, ByVal actionLength)
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
#End Region
#Region "Statuses"
    Friend Sub Wave()
        If Statuses.ContainsKey("wave") Then Return
        If Statuses.ContainsKey("dance") Then Statuses.Remove("dance")
        Dim waveAction As New actionThread(AddressOf handleStatus)
        waveAction.BeginInvoke("wave", vbNullString, 1500, Nothing, Nothing)
    End Sub
    Friend Sub Dance(ByVal danceID As Integer)
        Statuses.Add("dance", danceID)
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
            "a:-1" & Convert.ToChar(13) & _
            "n:" & Name & Convert.ToChar(13) & _
            "f:" & Figure & Convert.ToChar(13) & _
            "l:" & PosX & " " & PosY & " " & PosH & Convert.ToChar(13)
            If Not (Mission = vbNullString) Then detailsStack += "c:" & Mission & Convert.ToChar(13)

            Return detailsStack & "[bot]"
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
        ' roomCommunicator.sendAll() '// Send updated @b packet that makes user perform ACTIONKEY in room
        'Thread.Sleep(actionLength) '// Wait...
        Statuses.Remove(actionKey)
        'Refresh() '// Send updated @b packet again, but now the /ACTIONKEY/ is removed
    End Sub
#End Region
End Class
