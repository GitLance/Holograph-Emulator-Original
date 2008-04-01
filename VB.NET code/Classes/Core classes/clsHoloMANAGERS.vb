Imports System.Threading
Public Class clsHoloMANAGERS
    Friend hookedUsers As New Hashtable
    Friend hookedRooms As New Hashtable
    Friend cfhCache As New Hashtable
    Friend maxConnections As Integer
    Function isOnline(ByRef userID As Integer) As Boolean
        Return hookedUsers.ContainsKey(userID)
    End Function
    Function getUserClass(ByRef userID As Integer) As clsHoloUSER
        If hookedUsers.ContainsKey(userID) Then Return hookedUsers(userID)
        Return Nothing
    End Function
    Function getUserDetails(ByRef userID As Integer) As clsHoloUSERDETAILS
        If hookedUsers.ContainsKey(userID) Then Return DirectCast(hookedUsers(userID), clsHoloUSER).userDetails
        Return Nothing
    End Function
    Function getUserHotelPosition(ByRef userID As Integer) As String
        Try
            Dim userDetails As clsHoloUSERDETAILS = DirectCast(hookedUsers(userID), clsHoloUSER).userDetails '// Get the users class
            '// Determine the users location
            If userDetails.roomID > 0 Then '// User is in room
                If userDetails.inPublicroom = True Then getUserHotelPosition = "in Public Room NAME kthx" Else getUserHotelPosition = "Floor1a"
            Else
                getUserHotelPosition = HoloRACK.Console_OnHotelView '// On Hotel View entry matching external_texts
            End If

            Return "I" & getUserHotelPosition
        Catch
            Return "H"
        End Try
    End Function
    Public Sub sendToRank(ByVal rankID As Integer, ByVal includeHigher As Boolean, ByVal strData As String)
        For Each User As clsHoloUSER In hookedUsers.Values
            If User.userDetails.Rank < rankID Then Continue For
            If User.userDetails.Rank > rankID Then If includeHigher = False Then Continue For
            User.transData(strData)
        Next
    End Sub
    Public Sub updateRoomInsideCount(ByVal roomID As Integer, ByVal isPublicroom As Boolean, ByVal insideCount As Integer)
        Dim roomType As String = vbNullString
        If isPublicroom = True Then roomType = "publicrooms" Else roomType = "guestrooms"
        HoloDB.runQuery("UPDATE " & roomType & " SET incnt_now = '" & insideCount & "' WHERE id = '" & roomID & "' LIMIT 1")
    End Sub
End Class