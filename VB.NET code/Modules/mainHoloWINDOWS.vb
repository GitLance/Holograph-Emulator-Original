Module mainHoloWINDOWS
    Private Declare Unicode Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringW" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpDefault As String, ByVal lpReturnedString As String, ByVal nSize As Int32, ByVal lpFileName As String) As Int32
    Private Declare Unicode Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringW" (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpString As String, ByVal lpFileName As String) As Int32
    Public Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Integer)
    Function readINI(ByVal iniSection As String, ByVal iniKey As String, ByVal iniLocation As String) As String
        Dim retLen As Integer
        Dim retStr As String
        retStr = Space$(1024)
        retLen = GetPrivateProfileString(iniSection, iniKey, vbNullString, retStr, retStr.Length, iniLocation)
        If retLen > 0 Then Return retStr.Substring(0, retLen)
        Return vbNullString
    End Function
    Sub modINI(ByVal iniSection As String, ByVal iniKey As String, ByVal strNewContent As String, ByVal iniLocation As String)
        WritePrivateProfileString(iniSection, iniKey, strNewContent, iniLocation)
    End Sub
    Function rndVal(ByVal minVal As Long, ByVal maxVal As Long) As Int32
        Dim v As New Random
        Return v.Next(minVal, maxVal + 1)
    End Function
End Module