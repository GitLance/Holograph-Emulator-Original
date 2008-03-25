Public Class clsHoloRANK
    Friend strFuse As String
    Friend fuseRights() As String
    Friend Function containsRight(ByVal fuseRight As String) As Boolean
        For c = 0 To UBound(fuseRights)
            If fuseRights(c) = fuseRight Then Return True '// This rank contains the right that was searched, return a true and exit here
        Next
        Return False '// Nuthing found, this rank doesn't contains the searched fuseright, return a false and return
    End Function
End Class
