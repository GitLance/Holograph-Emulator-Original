Public Class clsHoloMISC
    Public Function filterWord(ByVal strText As String) As String
        If HoloRACK.wordFilter_Enabled = True Then
            For i = 0 To HoloRACK.wordFilter_Words.Count - 1 '// Send the input text through the whole word filter
                strText = Replace(strText, HoloRACK.wordFilter_Words(i), HoloRACK.wordFilter_Replacement, 1, -1, vbTextCompare)
            Next
        End If
        Return strText '// Output the filtered text
    End Function
    Public Function consoleMissionFix(ByVal strConsoleMission As String) As String
        If strConsoleMission = vbNullString Then Return "H" Else Return "I" & strConsoleMission
    End Function
    Public Function getRoomState(ByVal dbState As String, Optional ByVal useReverse As Boolean = False) As String
        If useReverse = False Then
            If dbState = 1 Then
                Return "closed"
            ElseIf dbState = 2 Then
                Return "password"
            Else
                Return "open"
            End If
        Else
            If dbState = "closed" Then
                Return 1
            ElseIf dbState = "password" Then
                Return 2
            Else
                Return 0
            End If
        End If
    End Function
    Public Function getRoomModelChar(ByVal modelID As Byte) As String
        Return HoloRACK.roomModels(Integer.Parse(modelID))
    End Function
    Public Function getRoomModelID(ByVal modelChar As String) As Byte
        For m = 1 To 18
            If HoloRACK.roomModels(m) = modelChar Then Return m
        Next
    End Function
    Public Function getUserRotation_Head(ByVal nowX As Integer, ByVal nowY As Integer, ByVal toX As Integer, ByVal toY As Integer) As Byte
        If nowX = toX Then
            If nowY < toY Then Return 4 Else Return 0
        ElseIf nowX < toX Then
            If nowX = toY Then : Return 2 : ElseIf nowY < toY Then : Return 3 : Else : Return 1 : End If
        ElseIf nowX > toX Then
            If nowY = toY Then : Return 6 : ElseIf nowY < toY Then : Return 5 : Else : Return 7 : End If
        Else
            Return 0
        End If
    End Function
    Public Function getFriendIDs(ByVal userID As Integer) As Integer()
        Try
            Dim idBuilder As New ArrayList
            Dim friendIDs() As Integer = HoloDB.runReadColumn("SELECT friendid FROM messenger_friendships WHERE userid = '" & userID & "'", 0, Nothing)
            For i = 0 To friendIDs.Count - 1 : idBuilder.Add(friendIDs(i)) : Next
            friendIDs = HoloDB.runReadColumn("SELECT userid FROM messenger_friendships WHERE friendid = '" & userID & "'", 0, Nothing)
            For i = 0 To friendIDs.Count - 1 : idBuilder.Add(friendIDs(i)) : Next
            Return idBuilder.ToArray(GetType(Integer))

        Catch
            Return New Integer(-1) {}

        End Try
    End Function
End Class
