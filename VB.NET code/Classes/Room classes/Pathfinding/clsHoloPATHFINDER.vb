Public Class clsHoloPATHFINDER
    Public startNode, goalNode As clsHoloMAPNODE
    Public openList, closedList As New clsHoloMAPHEAP
    Public Successors As New ArrayList
    Public Solution As New ArrayList
    Public roomMap(,) As Integer
    Public maxX, maxY As Integer
    Public Function getMap(ByVal X As Integer, ByVal Y As Integer)
        'Console.WriteLine("...")
        If X > maxX OrElse X < 0 Then Return -1
        If Y > maxY OrElse X < 0 Then Return -1
        If roomMap(X, Y) = -1 Then Return -1
        Return 0
    End Function
    Public Function findPath(ByVal roomMap(,) As Integer, ByVal nowX As Integer, ByVal nowY As Integer, ByVal toX As Integer, ByVal toY As Integer) As Integer()
        Me.roomMap = roomMap
        maxX = roomMap.GetLength(1) - 1
        maxY = roomMap.GetLength(0) - 1

        goalNode = New clsHoloMAPNODEBASE(Nothing, Nothing, 0, nowX, nowY)
        startNode = New clsHoloMAPNODEBASE(Nothing, goalNode, 0, toX, toY)

        openList.Add(startNode)
        While openList.Count > 0
            ' Get the node with the lowest TotalSum 
            Dim NodeCurrent As clsHoloMAPNODE = DirectCast(openList.Pop(), clsHoloMAPNODE)

            ' If the node is the goal copy the path to the solution array 
            If NodeCurrent.isGoal() Then
                While NodeCurrent IsNot Nothing
                    Solution.Insert(0, NodeCurrent)
                    NodeCurrent = NodeCurrent.parentNode
                End While
                Exit While
            End If

            ' Get successors to the current node 
            NodeCurrent.getSuccessors(Successors)
            For Each NodeSuccessor As clsHoloMAPNODE In NodeCurrent.Successors
                ' Test if the currect successor node is on the open list, if it is and 
                ' the TotalSum is higher, we will Throw away the current successor. 
                Dim NodeOpen As clsHoloMAPNODE = Nothing
                If openList.Contains(NodeSuccessor) Then
                    NodeOpen = DirectCast(openList(openList.IndexOf(NodeSuccessor)), clsHoloMAPNODE)
                    If (NodeOpen IsNot Nothing) AndAlso (NodeSuccessor.totalCost > NodeOpen.totalCost) Then
                        Continue For
                    End If
                End If

                ' Test if the currect successor node is on the closed list, if it is and 
                ' the TotalSum is higher, we will Throw away the current successor. 
                Dim NodeClosed As clsHoloMAPNODE = Nothing
                If closedList.Contains(NodeSuccessor) Then
                    NodeClosed = DirectCast(closedList(closedList.IndexOf(NodeSuccessor)), clsHoloMAPNODE)
                    If (NodeClosed IsNot Nothing) AndAlso (NodeSuccessor.totalCost > NodeClosed.totalCost) Then
                        Continue For
                    End If
                End If

                ' Remove the old successor from the open list 
                openList.Remove(NodeSuccessor)

                ' Remove the old successor from the closed list 
                closedList.Remove(NodeSuccessor)

                ' Add the current successor to the open list 
                openList.Push(NodeSuccessor)
            Next
            ' Add the current node to the closed list 
            openList.Add(NodeCurrent)
        End While

        If Solution.Count = 0 Then
            Return Nothing
        Else
            Dim solutionResult() As Integer
            Dim solutionNode As clsHoloMAPNODEBASE = DirectCast(Solution(1), clsHoloMAPNODEBASE)
            solutionResult(0) = solutionNode.X
            solutionResult(1) = solutionNode.Y
            solutionResult(2) = getRotation(nowX, nowY, solutionResult(0), solutionResult(1))
        End If
    End Function
    Private Shared Function getRotation(ByVal X1 As Integer, ByVal Y1 As Integer, ByVal X2 As Integer, ByVal Y2 As Integer) As Integer
        Dim vRtx As Integer
        If X1 > X2 And Y1 > Y2 Then
            vRtx = 7
        ElseIf X1 < X2 And Y1 < Y2 Then
            vRtx = 3
        ElseIf X1 > X2 And Y1 < Y2 Then
            vRtx = 5
        ElseIf X1 < X2 And Y1 > Y2 Then
            vRtx = 1
        ElseIf X1 > X2 Then
            vRtx = 6
        ElseIf X1 < X2 Then
            vRtx = 2
        ElseIf Y1 < Y2 Then
            vRtx = 4
        ElseIf Y1 > Y2 Then
            vRtx = 0
        End If

        Return vRtx
    End Function
End Class
