Imports System.Collections
Public Class clsHoloSTRINGS
    Private langStrings As Hashtable
    Public langExt As String
    Public Sub New(ByVal langExt As String)
        Me.langExt = langExt
        langStrings = New Hashtable

        Dim langKeys() As String = HoloDB.runReadColumn("SELECT stringid FROM system_strings", 0)
        Dim langVars() As String = HoloDB.runReadColumn("SELECT var_" & langExt & " FROM system_strings", 0)

        For i = 0 To langKeys.Count - 1
            If langKeys(i) = vbNullString Then Continue For
            If langVars(i) = vbNullString Then langVars(i) = langKeys(i)
            langStrings.Add(langKeys(i), langVars(i))
        Next

        Console.WriteLine("[SERVER] Loaded " & langKeys.Count & " strings from system_strings table.")
    End Sub
    Public Function getString(ByVal stringID As String) As String
        Try
            Return langStrings(stringID)

        Catch
            Return stringID

        End Try
    End Function
End Class
