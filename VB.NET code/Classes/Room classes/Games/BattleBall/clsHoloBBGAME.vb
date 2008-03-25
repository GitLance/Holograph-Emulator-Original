Imports System.Text
Imports System.Threading
Public Class clsHoloBBGAME
    Friend ID As Integer
    Friend Name As String
    Friend Owner As String
    Friend OwnerRoomIdentifier As Integer
    Friend teamCount As Integer
    Friend mapID As Integer
    Friend totalTime As Integer
    Friend leftTime As Integer
    Friend powerUps() As String

    Private teamMembers(3) As Hashtable
    Private subViewers As New Hashtable
    Private Spectators As New Hashtable
    Private arenaWorker As New Thread(AddressOf arenaActions)
    Private gTile(,) As Byte
    Public Sub defineTeams()
        For TT = 0 To teamCount - 1
            teamMembers(TT) = New Hashtable
        Next
    End Sub
    Public Sub Dump()
        Dim subViewer As clsHoloUSERDETAILS
        For TT = 0 To teamCount - 1
            For Each subViewer In teamMembers(TT).Values
                subViewer.Game_owns = False
                subViewer.Game_withState = -1
                subViewer.Game_ID = -1
                subViewer.userClass.transData("CmH" & sysChar(1))
            Next
        Next

        For Each subViewer In Spectators.Values
            subViewer.Game_ID = -1
            subViewer.userClass.transData("CmH" & sysChar(1))
        Next

        For Each subViewer In subViewers.Values
            subViewer.Game_ID = -1
            subViewer.userClass.transData("CmH" & sysChar(1))
        Next
    End Sub
    Public Sub sendToAllViewers(ByVal strData As String)
        Dim subViewer As clsHoloUSERDETAILS

        For TT = 0 To teamCount - 1
            For Each subViewer In teamMembers(TT).Values
                subViewer.userClass.transData(strData)
            Next
        Next

        For Each subViewer In Spectators.Values
            subViewer.userClass.transData(strData)
        Next

        For Each subViewer In subViewers.Values
            subViewer.userClass.transData(strData)
        Next
    End Sub
    Public Sub modTeam(ByVal userDetails As clsHoloUSERDETAILS, ByVal oldTeamID As Integer, ByVal newTeamID As Integer)
        On Error Resume Next
        If newTeamID = -1 Then
            teamMembers(oldTeamID).Remove(userDetails.UserID)
        Else
            If oldTeamID >= 0 Then teamMembers(oldTeamID).Remove(userDetails.UserID)
            teamMembers(newTeamID).Add(userDetails.UserID, userDetails.userClass)
            userDetails.Game_withState = newTeamID
        End If
        sendToAllViewers(getGameSub)
    End Sub
    Public Sub modViewers(ByVal userDetails As clsHoloUSERDETAILS, ByVal isSpectator As Boolean, Optional ByVal removeMe As Boolean = False)
        If removeMe = False Then
            If isSpectator = True Then
                Spectators.Add(userDetails.UserID, userDetails)
                sendToAllViewers(getGameSub)
            Else
                subViewers.Add(userDetails.UserID, userDetails)
            End If
            userDetails.Game_ID = Me.ID
            userDetails.Game_owns = False
            userDetails.Game_withState = -1
            userDetails.userClass.transData(getGameSub)
        Else
            If isSpectator = True Then
                Spectators.Remove(userDetails.UserID)
                sendToAllViewers(getGameSub)
            Else
                subViewers.Remove(userDetails.UserID)
            End If
            userDetails.Game_ID = -1
            userDetails.Game_owns = False
            userDetails.Game_withState = -1
        End If
    End Sub
    Public Sub startGame()
        Dim gameMap As String = "@S" & sysChar(1) & "Bf" & "http://www.holographemulator.com/" & sysChar(1) & "AE" & HoloDB.runRead("SELECT name_ae FROM publicrooms WHERE id = '9" & mapID & "' LIMIT 1") & sysChar(1)
        gameMap += "CP0" & sysChar(1) & "@_" & HoloDB.runRead("SELECT map_height FROM publicrooms WHERE id = '9" & mapID & "' LIMIT 1") & sysChar(1)

        For TT = 0 To teamCount - 1
            For Each arenaAspirant As clsHoloUSERDETAILS In teamMembers(TT).Values
                HoloBBGAMELOBBY.parentRoom.leaveUser(arenaAspirant)
                arenaAspirant.roomID = Integer.Parse(9 & mapID)
                arenaAspirant.Game_withState = -5
                arenaAspirant.userClass.transData(gameMap)
                ' arenaAspirant.transData(gameDyn)
            Next
        Next

        For Each arenaInsider As clsHoloUSERDETAILS In Spectators.Values
            arenaInsider.userClass.transData(gameMap)
            'arenaAspirant.transData(gameDyn)
        Next
    End Sub
    Public Function getGameSub()
        Dim teamMemberCount, startStatus, canStart As Integer
        Dim navPack As New System.Text.StringBuilder("Ci")

        For TT = 0 To teamCount - 1
            teamMemberCount += teamMembers(TT).Count
        Next

        If teamMemberCount > 1 Then canStart = 1
        If leftTime < totalTime Then startStatus = 1
        If leftTime = 0 Then startStatus = 2

        navPack.Append(HoloENCODING.encodeVL64(startStatus) & HoloENCODING.encodeVL64(canStart) & Name & sysChar(2) & HoloENCODING.encodeVL64(OwnerRoomIdentifier) & Owner & sysChar(2) & HoloENCODING.encodeVL64(mapID) & HoloENCODING.encodeVL64(Spectators.Count) & HoloENCODING.encodeVL64(teamCount))

        For T = 0 To teamCount - 1
            navPack.Append(HoloENCODING.encodeVL64(teamMembers(T).Count))
            For Each teamMember As clsHoloUSERDETAILS In teamMembers(T).Values
                navPack.Append(HoloENCODING.encodeVL64(teamMember.roomUID) & teamMember.Name & sysChar(2))
            Next
        Next

        If powerUps.Count > 0 Then
            For P = 0 To powerUps.Count - 1
                navPack.Append(powerUps(P) & ",")
            Next
        End If

        navPack.Append("9" & sysChar(2) & sysChar(1))

        Return navPack.ToString
    End Function
    Private Sub sendToArena(ByVal strData As String)
        Dim arenaInsider As clsHoloUSERDETAILS

        For TT = 0 To teamCount - 1
            For Each arenaInsider In teamMembers(TT).Values
                arenaInsider.userClass.transData(strData)
            Next
        Next

        For Each arenaInsider In Spectators.Values
            arenaInsider.userClass.transData(strData)
        Next
    End Sub
    Private Sub arenaActions()
        Dim actionPack As New StringBuilder("Ct")

        actionPack.Append(sysChar(1))
        sendToArena(actionPack.ToString)

        Thread.Sleep(500)
    End Sub
End Class

