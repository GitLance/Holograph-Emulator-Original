Public Class clsHoloDB
    Private dbConnection As ADODB.Connection
    Private dbRecordset As ADODB.Recordset
    Function openConnection(ByVal serverHost As String, ByVal serverPort As Short, ByVal serverDatabase As String, ByVal serverUsername As String, ByVal serverPassword As String) As Boolean
        Try '// Try opening connection
            dbConnection = New ADODB.Connection
            dbRecordset = New ADODB.Recordset
            dbConnection.ConnectionString = "Driver={MySQL ODBC 3.51 Driver};Server=" & serverHost & ";Port=" & serverPort & ";Database=" & serverDatabase & ";User=" & serverUsername & ";Password=" & serverPassword & ";Option=3;"
            dbConnection.Open() '// Try opening the connection
            dbRecordset.ActiveConnection = dbConnection
            Return True

        Catch ex As Exception '// Error occured!
            Console.WriteLine("[MYSQL] Epic fail at connecting, error thrown was: " & ex.Message)
            stopServer()

        End Try
    End Function
    Friend Sub closeConnection()
        On Error Resume Next '// Make it skip errors
        dbConnection.Close() '// Close the connection
    End Sub
    Sub runQuery(ByRef Query As String)
        dbConnection.Execute(Query) '// Run the query on the database
    End Sub
    Function checkExists(ByRef Query As String) As Boolean
        dbRecordset.Open(Query) '// Open the recordset for this query
        If dbRecordset.EOF = False Then checkExists = True '// If there were results found (so the row exists), then return 'true'
        dbRecordset.Close() '// Close this recordset
    End Function
    Function runRead(ByRef Query As String) As String
        Dim queryResult As String = vbNullString
        dbRecordset.Open(Query) ''// Open the recordset for this query

        If dbRecordset.EOF = False Then '// If there was a row found matching this critecithinges
            queryResult = dbRecordset.GetString '// Get the row
            If Not (queryResult = vbNullString) Then queryResult = queryResult.Substring(0, queryResult.Length - 1) '// If the row was found, then get it's strData and output it to the function
        End If

        dbRecordset.Close() '// Close the recordset
        Return queryResult '// Return the results
    End Function
    Function runReadArray(ByRef Query As String, Optional ByVal splitVertical As Boolean = False) As String()
        Dim finalArray() As String = {} '// Dimension an empty array

        dbRecordset.Open(Query) '// Open the recordset for this query
        If dbRecordset.EOF = False Then '// If there was a row found matching this critecithinges
            Dim chrSplitter As Char = sysChar(9) '// Set the default splitter
            If splitVertical = True Then chrSplitter = sysChar(13) '// It requested to split vertically, set splitter to char 13
            Dim queryResult As String = dbRecordset.GetString '// Get the row
            finalArray = (queryResult.Remove(queryResult.Length - 1)).Split(chrSplitter) '// Split the row with set splitter and output
        End If

        dbRecordset.Close() '// Close the recordset for this query

        '// Return the final array, we can't return runReadArray directly because it'll be seen as a nulled array, which is invalid.
        '// That's why we keep a real empty array called finalArray, and only store stuff in it when there's a row found =]
        Return finalArray
    End Function
    Function fixChars(ByVal strData As String) As String
        Return strData.Replace("'", "\'") '// Replace the ' character (MySQL sees it as begin and end of a input string) with another character, in the database MySQL sets it to a ' ;]
    End Function
End Class