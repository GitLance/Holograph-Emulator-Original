Public Class clsHoloMAPNODEBASE
    Inherits clsHoloMAPNODE
    Sub New(ByVal parentNode As clsHoloMAPNODE, ByVal goalNode As clsHoloMAPNODE, ByVal Cost As Double, ByVal X As Integer, ByVal Y As Integer)
        MyBase.New(parentNode, goalNode, Cost, X, Y)
    End Sub
    Private Sub addSuccessor(ByVal X As Integer, ByVal Y As Integer)
        Dim currentCost As Integer = Pathfinder.getMap(X, Y)
        If currentCost = -1 Then Return

        Dim newNode As New clsHoloMAPNODE(Me, goalNode, Cost + currentCost, X, Y)
        If isSameState(parentNode) = True Then Return

        Successors.Add(newNode)
    End Sub
    Public Overloads Overrides Function isSameState(ByVal Node As clsHoloMAPNODE) As Boolean
        If IsNothing(Node) Then Return False
        Return ((DirectCast(Node, clsHoloMAPNODE).X = X) AndAlso (DirectCast(Node, clsHoloMAPNODE).Y = Y))
    End Function
    Public Sub Calculate()
        If IsNothing(goalNode) Then
            estimatedCost = 0
        Else
            estimatedCost = Math.Max(Math.Abs(X - DirectCast(goalNode, clsHoloMAPNODEBASE).X), Math.Abs(Y - DirectCast(goalNode, clsHoloMAPNODEBASE).Y))
        End If
    End Sub
    Public Overloads Overrides Sub getSuccessors(ByVal Successors As ArrayList)
        Successors.Clear()
        addSuccessor(X - 1, Y)
        addSuccessor(X - 1, Y - 1)
        addSuccessor(X, Y - 1)
        addSuccessor(X + 1, Y - 1)
        addSuccessor(X + 1, Y)
        addSuccessor(X + 1, Y + 1)
        addSuccessor(X, Y + 1)
        addSuccessor(X - 1, Y + 1)
    End Sub
    Public Overloads Overrides Function Equals(ByVal obj As Object) As Boolean
        Return isSameState(DirectCast(obj, clsHoloMAPNODE))
    End Function
    Public Overloads Overrides Function GetHashCode() As Integer
        Return MyBase.GetHashCode()
    End Function
End Class
Public Class clsHoloMAPNODE
    Implements IComparable
    Friend parentNode As clsHoloMAPNODE
    Friend goalNode As clsHoloMAPNODE
    Friend Cost, totalCost, estimatedCost As Double
    Friend X, Y As Integer
    Friend Successors As ArrayList
    Sub New(ByVal parentNode As clsHoloMAPNODE, ByVal goalNode As clsHoloMAPNODE, ByVal Cost As Double, ByVal X As Integer, ByVal Y As Integer)
        Me.parentNode = parentNode
        Me.goalNode = goalNode
        Successors = New ArrayList
        Me.X = X
        Me.Y = Y
        Me.Cost = Cost
    End Sub
    Public Overridable Function isSameState(ByVal Node As clsHoloMAPNODE) As Boolean
        Return False
    End Function
    Function isGoal() As Boolean
        Return isSameState(goalNode)
    End Function
    Public Overridable Sub getSuccessors(ByVal Successors As ArrayList)
    End Sub
    Public Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.compareto
        Return (-totalCost.CompareTo(DirectCast(obj, clsHoloMAPNODE).totalCost))
    End Function
End Class
