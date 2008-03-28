Imports System.Diagnostics
Namespace Holo.logSystem
#Region "Enumerators"
    Public Enum logToFlags
        None = 0
        consoleOnly = 1
        logFile = 2
        Both = 3
    End Enum
    Public Enum logEventFlags
        Standard = 0
        errorsOnly = 1
        Both = 2
    End Enum
    Public Enum logImportanceFlags
        nonImportant = 0
        belowStandard = 1
        Standard = 2
        Important = 3
    End Enum
#End Region
    Public NotInheritable Class consoleHelper
        Private Shared logToFlag As logToFlags
        Private Shared logEventFlag As logEventFlags
        Private Shared logMinImportanceFlag As logImportanceFlags
        Private Shared logInUse As Boolean
#Region "Flag toggling"
        Friend Sub setLogToFlag(ByVal toFlag As logToFlags)
            logToFlag = toFlag
        End Sub
        Friend Sub setEventLogFlag(ByVal toFlag As logEventFlags)
            logEventFlag = toFlag
        End Sub
        Friend Sub setImportanceFlag(ByVal toFlag As logImportanceFlags)
            logMinImportanceFlag = toFlag
        End Sub
#End Region
#Region "Logging"
        Friend Shared Sub writeLine(ByVal strLine As String, ByVal colColor As ConsoleColor, ByVal Importance As logImportanceFlags)
            If Importance < logMinImportanceFlag Then Return
            Do Until logInUse = False : Loop '// Wait till done with writing of other action, so it doesn't writes over other logs
            logInUse = True '// Now it's our turn
            Console.Write(" [" + DateTime.Now + "] : ")
            Console.ForegroundColor = colColor

            Dim logStackTrace As New StackTrace
            Dim logStackFrame As StackFrame = logStackTrace.GetFrame(1)

            Console.Write(logStackFrame.GetMethod().ReflectedType.FullName + "." + logStackFrame.GetMethod.Name)
            Console.ForegroundColor = ConsoleColor.Gray
            Console.Write(vbNewLine)

            logInUse = False
        End Sub
#End Region
    End Class
End Namespace
