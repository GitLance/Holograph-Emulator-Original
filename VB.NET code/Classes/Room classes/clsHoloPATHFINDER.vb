Public Class clsHoloPATHFINDER
    Private roomMap(,) As Byte
    Private maxX, maxY As Integer

    Private openList, closedList As clsHoloILIST
    Private startNode, goalNode As mapNode
    Private solutionList, successorList As ArrayList
    Public Sub New(ByVal roomMap(,) As Byte)
        Me.roomMap = roomMap
        maxX = roomMap.GetLength(1) - 1
        maxY = roomMap.GetLength(0) - 1

        openList = New clsHoloILIST
        closedList = New clsHoloILIST
        solutionList = New ArrayList
        successorList = New ArrayList
    End Sub
    Public Function getNextStep(ByVal nowX As Integer, ByVal nowY As Integer, ByVal toX As Integer, ByVal toY As Integer) As Integer()
        If nowX = toX And nowY = toY Then Return Nothing

        Dim maxCycles As Integer = maxX * maxY
        Dim cntCycles As Integer = 0

        goalNode = New mapNode(toX, toY, 0.0, Nothing, Nothing, Me) '// Create a 'goalnode'
        startNode = New mapNode(nowX, nowY, 0.0, Nothing, goalNode, Me) '// Create a 'startnode'

        openList.Add(startNode) '// Add the startnode to the open list (IList)

        While openList.Count > 0
            If cntCycles = maxCycles Then Return Nothing '// Has searched whole map, no path found, gtfo
            cntCycles += 1 '// Increment cycle count + 1

            Dim curNode As mapNode = openList.Pop() '// Kick the topmost node in the list in 'curNode' [removes from list]
            If curNode.isSameCoordsAs(goalNode) = True Then '// If the kicked node is the goal...
                While IsNothing(curNode) = False '// Keep looping till there are no parents more to link to
                    solutionList.Insert(0, curNode) '// Add the curNode to the solutionlist
                    curNode = curNode.parentNode '// Set the curNode to it's own parent
                End While
                Exit While '// No more linking nodes, exit while and proceed to last part
            Else
                curNode.getSuccessors() '// Attach working successor nodes to curNode
                successorList = curNode.allSuccessors '// Get the attached nodes
                For Each sucNode As mapNode In successorList
                    Dim nodeOpen As mapNode = Nothing
                    If openList.Contains(sucNode) = True Then nodeOpen = openList(openList.IndexOf(sucNode))
                    If IsNothing(nodeOpen) = False Then If sucNode.totalCost > nodeOpen.totalCost Then Continue For

                    Dim nodeClosed As mapNode = Nothing
                    If openList.Contains(sucNode) = True Then nodeClosed = openList(openList.IndexOf(sucNode))
                    If IsNothing(nodeClosed) = False Then If sucNode.totalCost > nodeClosed.totalCost Then Continue For

                    openList.Remove(nodeOpen)
                    closedList.Remove(nodeClosed)

                    openList.Push(sucNode)
                Next
                closedList.Add(curNode)
            End If
        End While

        If solutionList.Count = 0 Then Return Nothing

        Dim nextStep(2) As Integer
        Dim nextStepNode As mapNode = solutionList(1)
        nextStep(0) = nextStepNode.X : nextStep(1) = nextStepNode.Y
        nextStep(2) = getRotation(nowX, nowY, nextStepNode.X, nextStepNode.Y)
        Return nextStep
    End Function
    Public Function getSqState(ByVal X As Integer, ByVal Y As Integer) As Byte
        Try
            If roomMap(X, Y) = 2 Then Return 0
            Return roomMap(X, Y)
        Catch
            Return 0
        End Try
    End Function
    Private Class mapNode
        Implements ICOMPARABLE
        Friend X, Y As Integer
        Private goalNode As mapNode
        Private parentPather As clsHoloPATHFINDER
        Friend parentNode As mapNode
        Private iCost, iGoalEstimate As Double
        Private Successors As ArrayList
        Friend Sub New(ByVal X As Integer, ByVal Y As Integer, ByVal Cost As Double, ByVal parentNode As mapNode, ByVal goalNode As mapNode, ByVal pathFinder As clsHoloPATHFINDER)
            Me.X = X
            Me.Y = Y
            Me.parentNode = parentNode
            Me.goalNode = goalNode
            Me.parentPather = pathFinder
            Me.Successors = New ArrayList
        End Sub
        Friend ReadOnly Property isSameCoordsAs(ByVal asNode As mapNode) As Boolean
            Get
                Return (X = asNode.X And Y = asNode.Y)
            End Get
        End Property
        Friend ReadOnly Property allSuccessors() As ArrayList
            Get
                Return Successors
            End Get
        End Property
        Friend Property Cost()
            Get
                Return iCost
            End Get
            Set(ByVal value)
                iCost = value
            End Set
        End Property
        Friend Property goalEstimate()
            Get
                reCalc()
                Return iGoalEstimate
            End Get
            Set(ByVal value)
                iGoalEstimate = value
            End Set
        End Property
        Friend ReadOnly Property totalCost() As Double
            Get
                Return Cost + goalEstimate
            End Get
        End Property
        Friend Sub getSuccessors()
            addSuccessor(X - 1, Y)
            addSuccessor(X, Y + 1)
            addSuccessor(X, Y - 1)
            addSuccessor(X + 1, Y)

            addSuccessor(X - 1, Y - 1)
            addSuccessor(X + 1, Y - 1)
            addSuccessor(X + 1, Y + 1)
            addSuccessor(X - 1, Y + 1)
            Return

            '// Cornering [less paths, need to fix!]
                If parentPather.getSqState(X, Y - 1) > 0 Then If parentPather.getSqState(X - 1, Y) > 0 Then addSuccessor(X - 1, Y - 1)
                If parentPather.getSqState(X, Y - 1) > 0 Then If parentPather.getSqState(X + 1, Y) > 0 Then addSuccessor(X + 1, Y - 1)
                If parentPather.getSqState(X + 1, Y) > 0 Then If parentPather.getSqState(X, Y + 1) > 0 Then addSuccessor(X + 1, Y + 1)
                If parentPather.getSqState(X - 1, Y - 0) > 0 Then If parentPather.getSqState(X - 0, Y + 0) > 0 Then addSuccessor(X - 1, Y + 1)
        End Sub
        Private Sub addSuccessor(ByVal X As Integer, ByVal Y As Integer)
            Dim currentCost As Integer = parentPather.getSqState(X, Y) - 1
            If currentCost = -1 Then Return
            Dim newNode As mapNode = New mapNode(X, Y, Me.Cost + currentCost, Me, goalNode, parentPather)
            Successors.Add(newNode)
        End Sub
        Private Sub reCalc()
            If IsNothing(goalNode) = True Then
                goalEstimate = 0
            Else
                Dim xD As Double = X - goalNode.X
                Dim yD As Double = Y - goalNode.Y
                goalEstimate = Math.Sqrt((xD ^ 2) + (yD ^ 2))
            End If
        End Sub
        Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.CompareTo
            Return -totalCost.CompareTo(DirectCast(obj, mapNode).totalCost)
        End Function
    End Class
    Private Function getRotation(ByVal X1 As Integer, ByVal Y1 As Integer, ByVal X2 As Integer, ByVal Y2 As Integer) As Integer
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
