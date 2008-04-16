Imports System.Text
Public Class clsHoloBBGAMELOBBY
    Public parentRoom As clsHoloROOM
    Private gameLength As Integer = 180
    Public Games As New Hashtable
    Public allowedPowerUps As String = "1,2,3,4,5,6,7,8"

    Private strHeightmap As String
    Sub New(ByVal parentRoom As clsHoloROOM)
        Me.parentRoom = parentRoom
    End Sub
    Public Sub createGame(ByVal userDetails As clsHoloUSERDETAILS, ByVal gameSettings As String)
        If Integer.Parse(HoloDB.runRead("SELECT tickets FROM users WHERE id = '" & userDetails.UserID & "' LIMIT 1")) < 2 Then '// User doesn't have enough tickets!
            userDetails.userClass.transData("ClJ") '// Send the message that the user hasn't got enough tickets, and should buy some
            Return '// Stop here
        End If

        Dim gameID As Integer
        Dim newGame As New clsHoloBBGAME
        gameSettings = gameSettings.Substring(HoloENCODING.decodeB64(gameSettings.Substring(4, 2)) + 7)

        newGame.mapID = HoloENCODING.decodeVL64(gameSettings)
        gameSettings = gameSettings.Substring(HoloENCODING.encodeVL64(newGame.mapID).Length + 66)
        newGame.teamCount = HoloENCODING.decodeVL64(gameSettings)
        gameSettings = gameSettings.Substring(HoloENCODING.encodeVL64(newGame.teamCount).Length + 74)
        newGame.powerUps = gameSettings.Substring(2, HoloENCODING.decodeB64(gameSettings.Substring(0, 2))).Split(",")
        gameSettings = gameSettings.Substring((newGame.powerUps.Count * 2) + 195)
        newGame.Name = gameSettings.Substring(2, HoloENCODING.decodeB64(gameSettings.Substring(0, 2)))

        For gameID = 0 To 100
            If Games.ContainsKey(gameID) = False Then Exit For
        Next

        With newGame
            .ID = gameID
            .leftTime = gameLength
            .Owner = userDetails.Name
            .OwnerRoomIdentifier = userDetails.roomUID
            .defineTeams()
        End With

        Games.Add(gameID, newGame)

        userDetails.Game_owns = True
        userDetails.Game_ID = gameID
        newGame.modTeam(userDetails, -1, 0)
    End Sub
    Public Sub destroyGame(ByVal gameID As Integer)
        DirectCast(Games(gameID), clsHoloBBGAME).Dump()
        Games.Remove(gameID)
        parentRoom.sendAll(getGameList)
    End Sub
    Public Function getGameList() As String
        Dim gameHeader As String = "Ch"
        Dim gamePack As New StringBuilder

        If Games.Count > 0 Then
            Dim Game As clsHoloBBGAME
            Dim gameCount_waiting, gameCount_started, gameCount_disposed As Integer

            For Each Game In Games.Values
                gamePack.Append(HoloENCODING.encodeVL64(Game.ID) & Game.Name & Convert.ToChar(2) & HoloENCODING.encodeVL64(Game.OwnerRoomIdentifier) & Game.Owner & Convert.ToChar(2))

                If Game.leftTime = gameLength Then
                    gameCount_waiting += 1
                ElseIf Game.leftTime < gameLength Then
                    gameCount_started += 1
                Else
                    gameCount_disposed += 1
                End If

                gamePack.Append(HoloENCODING.encodeVL64(Game.mapID))
            Next

            gameHeader += HoloENCODING.encodeVL64(gameCount_waiting)
            If gameCount_started > 0 Or gameCount_disposed > 0 Then gameHeader += HoloENCODING.encodeVL64(gameCount_started) & HoloENCODING.encodeVL64(gameCount_disposed)
        End If

        Return (gameHeader & gamePack.ToString)
    End Function
End Class
