Imports System.Text
Public Class clsHoloMANAGERS
    Friend hookedUsers As Hashtable
    Friend hookedRooms As Hashtable
    Friend itemCache As Hashtable
    Friend maxConnections As Integer
    Public Sub New()
        hookedUsers = New Hashtable()
        hookedRooms = New Hashtable()
        itemCache = New Hashtable()
    End Sub
    Public Function isOnline(ByRef userID As Integer) As Boolean
        Return hookedUsers.ContainsKey(userID)
    End Function
#Region "Userclass(details) retrieval"
    Public Function getUserClass(ByVal userID As Integer) As clsHoloUSER
        If hookedUsers.ContainsKey(userID) Then Return hookedUsers(userID)
        Return Nothing
    End Function
    Public Function getUserClass(ByVal userName As String) As clsHoloUSER
        Dim userID As Integer = HoloDB.runRead("SELECT id FROM users WHERE name = '" & HoloDB.safeString(userName) & "'")
        If userID > 0 Then If hookedUsers.ContainsKey(userID) Then Return hookedUsers(userID)
        Return Nothing
    End Function
    Public Function getUserDetails(ByRef userID As Integer) As clsHoloUSERDETAILS
        If hookedUsers.ContainsKey(userID) Then Return DirectCast(hookedUsers(userID), clsHoloUSER).userDetails
        Return Nothing
    End Function
    Public Function getUserDetails(ByVal userName As String) As clsHoloUSERDETAILS
        Dim userID As Integer = HoloDB.runRead("SELECT id FROM users WHERE name = '" & HoloDB.safeString(userName) & "'")
        If userID > 0 Then If hookedUsers.ContainsKey(userID) Then Return DirectCast(hookedUsers(userID), clsHoloUSER).userDetails
        Return Nothing
    End Function
#End Region
    Public Function getUserHotelPosition(ByVal userID As Integer) As String
        Try
            Dim userDetails As clsHoloUSERDETAILS = DirectCast(hookedUsers(userID), clsHoloUSER).userDetails
            If userDetails.roomID = 0 Then Return "I" & HoloSTRINGS.getString("console_onhotelview")
            If userDetails.inPublicroom = True Then Return "I" & "In a Public Room"
            Return "I" & "Floor1b"

        Catch
            Return "H"

        End Try
    End Function
    Public Function getUserHotelStatus(ByVal userID As Integer, ByVal lastVisitDate As String) As String
        Try
            Dim userDetails As clsHoloUSERDETAILS = DirectCast(hookedUsers(userID), clsHoloUSER).userDetails
            If userDetails.roomID = 0 Then Return "I" & HoloSTRINGS.getString("console_onhotelview")
            If userDetails.inPublicroom = True Then Return "I" & "In a Public Room"
            Return "I" & "Floor1b"

        Catch
            Return "H" & Convert.ToChar(2) & lastVisitDate

        End Try
    End Function
    Public Function getRecommendedRooms() As String
        Dim roomPack As String = vbNullString

        Try
            For r = 1 To 3
                Dim roomData() As String = HoloDB.runReadRow("SELECT id,name,owner,descr,state,incnt_now,incnt_max FROM guestrooms ORDER BY RAND()")
                'Dim roomData() As String = HoloDB.runReadRow("SELECT id,name,owner,descr,state,incnt_now,incnt_max FROM guestrooms WHERE incnt_now > 0 ORDER BY RAND()") // Use this instead of the statement above if you want to show only rooms with people inside
                If roomData.Count = 0 Then Return "H" '// Error for w/e reason, no room for this list request now
                roomPack += HoloENCODING.encodeVL64(roomData(0)) & roomData(1) & Convert.ToChar(2) & roomData(2) & Convert.ToChar(2) & HoloMISC.getRoomState(roomData(4)) & Convert.ToChar(2) & HoloENCODING.encodeVL64(roomData(5)) & HoloENCODING.encodeVL64(roomData(6)) & roomData(3) & Convert.ToChar(2)
            Next
            Return HoloENCODING.encodeVL64(3) & roomPack.ToString & Convert.ToChar(2)

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
    ''' <summary>
    ''' Leaves a note for the Housekeeping in the system_tasklog table.
    ''' </summary>
    ''' <param name="staffAction">The action performed</param>
    ''' <param name="userID">The userid of the user who performed the action</param>
    ''' <param name="targetID">The userid of the target user, or the id of the room</param>
    ''' <param name="strMessage">The message that went with the performed action</param>
    ''' <param name="strNote">The note to leave</param>
    Public Sub addStaffNote(ByVal staffAction As String, ByVal userID As Integer, ByVal targetID As Integer, ByVal strMessage As String, ByVal strNote As String)
        HoloDB.runQuery("INSERT INTO system_stafflog (action,message,note,userid,targetid,timestamp) VALUES ('" & staffAction & "','" & HoloDB.safeString(strMessage) & "','" & HoloDB.safeString(strNote) & "','" & userID & "','" & targetID & "','" & DateTime.Now.ToString & "')")
    End Sub
End Class