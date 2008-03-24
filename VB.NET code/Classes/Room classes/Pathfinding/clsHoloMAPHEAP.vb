'// IList/ICloneable class for IList/ICloneable purposes
Public Class clsHoloMAPHEAP
    Implements IList
    Implements ICloneable
    Private FList As ArrayList
    Private FComparer As IComparer = Nothing
    Private FUseObjectsComparison As Boolean
    Private FAddDuplicates As Boolean
    Public Delegate Function Equality(ByVal Object1 As Object, ByVal Object2 As Object) As Boolean
    Public Sub New()
        InitProperties(Nothing, 0)
    End Sub
    Public Sub New(ByVal Comparer As IComparer)
        InitProperties(Comparer, 0)
    End Sub
    Public Property AddDuplicates() As Boolean
        Get
            Return FAddDuplicates
        End Get
        Set(ByVal value As Boolean)
            FAddDuplicates = value
        End Set
    End Property
    Public Property Capacity() As Integer
        Get
            Return FList.Capacity
        End Get
        Set(ByVal value As Integer)
            FList.Capacity = value
        End Set
    End Property
    Default Public Property Item(ByVal Index As Integer) As Object Implements IList.Item
        Get
            If Index >= FList.Count OrElse Index < 0 Then
                Throw New ArgumentOutOfRangeException("Index is less than zero or Index is greater than Count.")
            End If
            Return FList(Index)
        End Get
        Set(ByVal value As Object)
            Throw New InvalidOperationException("[] operator cannot be used to set a value in a Heap.")
        End Set
    End Property
    Public Function Add(ByVal O As Object) As Integer Implements IList.Add
        Dim [Return] As Integer = -1
        If ObjectIsCompliant(O) Then
            Dim Index As Integer = IndexOf(O)
            Dim NewIndex As Integer = IIf(Index >= 0, Index, -Index - 1)
            If NewIndex >= Count Then
                FList.Add(O)
            Else
                FList.Insert(NewIndex, O)
            End If
            [Return] = NewIndex
        End If
        Return [Return]
    End Function
    Public Function Contains(ByVal O As Object) As Boolean Implements IList.Contains
        Return FList.BinarySearch(O, FComparer) >= 0
    End Function
    Public Function IndexOf(ByVal value As Object) As Integer Implements IList.IndexOf
        Dim Result As Integer = -1
        Result = FList.BinarySearch(value, FComparer)
        While Result > 0 AndAlso FList(Result - 1).Equals(value)
            Result -= 1
        End While
        Return Result
    End Function
    Public ReadOnly Property IsFixedSize() As Boolean Implements IList.IsFixedSize
        Get
            Return FList.IsFixedSize
        End Get
    End Property
    Public ReadOnly Property IsReadOnly() As Boolean Implements IList.IsReadOnly
        Get
            Return FList.IsReadOnly
        End Get
    End Property
    Public Sub Clear() Implements IList.Clear
        FList.Clear()
    End Sub
    Public Sub Insert(ByVal Index As Integer, ByVal O As Object) Implements IList.Insert
        Throw New InvalidOperationException("Insert method cannot be called on a Heap.")
    End Sub
    Public Sub Remove(ByVal Value As Object) Implements IList.Remove
        FList.Remove(Value)
    End Sub
    Public Sub RemoveAt(ByVal Index As Integer) Implements IList.RemoveAt
        FList.RemoveAt(Index)
    End Sub
    Public Sub CopyTo(ByVal array As Array, ByVal arrayIndex As Integer) Implements ICollection.CopyTo
        FList.CopyTo(array, arrayIndex)
    End Sub
    Public ReadOnly Property Count() As Integer Implements IList.Count
        Get
            Return FList.Count
        End Get
    End Property
    Public ReadOnly Property IsSynchronized() As Boolean Implements ICollection.IsSynchronized
        Get
            Return FList.IsSynchronized
        End Get
    End Property
    Public ReadOnly Property SyncRoot() As Object Implements ICollection.syncRoot
        Get
            Return FList.SyncRoot
        End Get
    End Property
    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return FList.GetEnumerator()
    End Function
    Public Function Clone() As Object Implements ICloneable.Clone
        Dim cClone As New clsHoloMAPHEAP(FComparer)
        cClone.FList = DirectCast(FList.Clone(), ArrayList)
        cClone.FAddDuplicates = FAddDuplicates
        Return cClone
    End Function
    Public Overrides Function ToString() As String
        Dim OutString As String = "{"
        For i As Integer = 0 To FList.Count - 1
            OutString += FList(i).ToString() + (IIf(i <> FList.Count - 1, "; ", "}"))
        Next
        Return OutString
    End Function
    Public Overrides Function Equals(ByVal [Object] As Object) As Boolean
        Dim SL As clsHoloMAPHEAP = DirectCast([Object], clsHoloMAPHEAP)
        If SL.Count <> Count Then
            Return False
        End If
        For i As Integer = 0 To Count - 1
            If Not SL(i).Equals(Me(i)) Then
                Return False
            End If
        Next
        Return True
    End Function
    Public Overrides Function GetHashCode() As Integer
        Return FList.GetHashCode()
    End Function
    Public Function IndexOf(ByVal [Object] As Object, ByVal Start As Integer) As Integer
        Dim Result As Integer = -1
        Result = FList.BinarySearch(Start, FList.Count - Start, [Object], FComparer)
        While Result > Start AndAlso FList(Result - 1).Equals([Object])
            Result -= 1
        End While
        Return Result
    End Function
    Public Function IndexOf(ByVal [Object] As Object, ByVal AreEqual As Equality) As Integer
        For i As Integer = 0 To FList.Count - 1
            If AreEqual(FList(i), [Object]) Then
                Return i
            End If
        Next
        Return -1
    End Function
    Public Function IndexOf(ByVal [Object] As Object, ByVal Start As Integer, ByVal AreEqual As Equality) As Integer
        If Start < 0 OrElse Start >= FList.Count Then
            Throw New ArgumentException("Start index must belong to [0; Count-1].")
        End If
        For i As Integer = Start To FList.Count - 1
            If AreEqual(FList(i), [Object]) Then
                Return i
            End If
        Next
        Return -1
    End Function
    Public Sub AddRange(ByVal C As ICollection)
        For Each [Object] As Object In C
            Add([Object])
        Next
    End Sub
    Public Sub InsertRange(ByVal Index As Integer, ByVal C As ICollection)
        Throw New InvalidOperationException("Insert cannot be called on a Heap.")
    End Sub
    Public Sub LimitOccurrences(ByVal Value As Object, ByVal NumberToKeep As Integer)
        If Value Is Nothing Then
            Throw New ArgumentNullException("Value")
        End If
        Dim Pos As Integer = 0
        While (Pos = IndexOf(Value, Pos)) >= 0
            If NumberToKeep <= 0 Then
                FList.RemoveAt(Pos)
            Else
                Pos += 1
                NumberToKeep -= 1
            End If
            If FComparer.Compare(FList(Pos), Value) > 0 Then
                Exit While
            End If
        End While
    End Sub
    Public Sub RemoveDuplicates()
        Dim PosIt As Integer
        PosIt = 0
        While PosIt < Count - 1
            If FComparer.Compare(Me(PosIt), Me(PosIt + 1)) = 0 Then
                RemoveAt(PosIt)
            Else
                PosIt += 1
            End If
        End While
    End Sub
    Public Function IndexOfMin() As Integer
        Dim RetInt As Integer = -1
        If FList.Count > 0 Then
            RetInt = 0
            Dim RetObj As Object = FList(0)
        End If
        Return RetInt
    End Function
    Public Function IndexOfMax() As Integer
        Dim RetInt As Integer = -1
        If FList.Count > 0 Then
            RetInt = FList.Count - 1
            Dim RetObj As Object = FList(FList.Count - 1)
        End If
        Return RetInt
    End Function
    Public Function Pop() As Object
        If FList.Count = 0 Then
            Throw New InvalidOperationException("The heap is empty.")
        End If
        Dim [Object] As Object = FList(Count - 1)
        FList.RemoveAt(Count - 1)
        Return ([Object])
    End Function
    Public Function Push(ByVal [Object] As Object) As Integer
        Return (Add([Object]))
    End Function
    Private Function ObjectIsCompliant(ByVal [Object] As Object) As Boolean
        If FUseObjectsComparison AndAlso Not (TypeOf [Object] Is IComparable) Then
            Throw New ArgumentException("The Heap is set to use the IComparable interface of objects, and the object to add does not implement the IComparable interface.")
        End If
        If Not FAddDuplicates AndAlso Contains([Object]) Then
            Return False
        End If
        Return True
    End Function
    Private Class Comparison
        Implements IComparer
        Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements IComparer.Compare
            Dim C As IComparable = TryCast(x, IComparable)
            Return C.CompareTo(y)
        End Function
    End Class
    Private Sub InitProperties(ByVal Comparer As IComparer, ByVal Capacity As Integer)
        If Comparer IsNot Nothing Then
            FComparer = Comparer
            FUseObjectsComparison = False
        Else
            FComparer = New Comparison()
            FUseObjectsComparison = True
        End If
        FList = IIf(Capacity > 0, New ArrayList(Capacity), New ArrayList())
        FAddDuplicates = True
    End Sub
End Class
