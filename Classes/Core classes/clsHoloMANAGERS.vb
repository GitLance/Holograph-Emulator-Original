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
        If hookedUsers.ContainsKey(userID) = True Then '// The user is online!
            Dim userDetails As clsHoloUSERDETAILS = DirectCast(hookedUsers(userID), clsHoloUSER).userDetails '// Get the users class
            '// Determine the users location
            If userDetails.roomID > 0 Then '// User is in room
                If userDetails.inPublicroom = True Then getUserHotelPosition = "in Public Room NAME kthx" Else getUserHotelPosition = "Floor1a"
            Else
                getUserHotelPosition = HoloRACK.Console_OnHotelView '// On Hotel View entry matching external_texts
            End If
            Return "I" & getUserHotelPosition
        Else
            Return "H"
        End If
    End Function
    Public Sub sendToRank(ByVal rankID As Integer, ByVal includeHigher As Boolean, ByVal strData As String)
        For Each User As clsHoloUSER In hookedUsers.Values
            If User.userDetails.Rank < rankID Then Continue For
            If User.userDetails.Rank > rankID Then If includeHigher = False Then Continue For
            User.transData(strData)
        Next
    End Sub
End Class