Imports System.Data.Odbc
Imports System.Collections
''' <summary>
''' Provides high speed data access to the MySQL database of Holograph Emulator. It owns eh?
''' </summary>
''' <remarks></remarks>
Public Class clsHoloDB
    Private dbConnection As OdbcConnection
#Region "Database connection management"
    ''' <summary>
    ''' Opens connection to the MySQL database with the supplied parameters, and returns a 'true' Boolean when the connection has succeeded. Requires MySQL ODBC 3.51 driver to be installed.
    ''' </summary>
    ''' <param name="dbHost">The hostname/IP address where the database server is located.</param>
    ''' <param name="dbPort">The port the database server is running on.</param>
    ''' <param name="dbName">The name of the database.</param>
    ''' <param name="dbUsername">The username for authentication with the database.</param>
    ''' <param name="dbPassword">The pasword for authentication with the database.</param>
    Public Function openConnection(ByVal dbHost As String, ByVal dbPort As Integer, ByVal dbName As String, ByVal dbUsername As String, ByVal dbPassword As String) As Boolean
        Try '// Try opening connection
            dbConnection = New OdbcConnection("Driver={MySQL ODBC 3.51 Driver};Server=" & dbHost & ";Port=" & dbPort & ";Database=" & dbName & ";User=" & dbUsername & ";Password=" & dbPassword & ";Option=3;")
            dbConnection.Open() '// Try opening the connection
            Return True

        Catch ex As Exception '// Error occured!
            Console.WriteLine("[MYSQL] Failed to connect! Error thrown was: " & ex.Message)
            Shutdown()
            Return False

        End Try
    End Function
    ''' <summary>
    ''' Closes connection with the MySQL database. Any errors are ignored.
    ''' </summary>
    Public Sub closeConnection()
        Try
            dbConnection.Close()
            Console.WriteLine("[MYSQL:CLOSE] MySQL connection closed.")

        Catch
            Console.WriteLine("MYSQL:NULL] No MySQL open, thus not closing one.")

        End Try
    End Sub
#End Region

#Region "Database data manipulation"
    ''' <summary>
    ''' Executes a SQL statement on the database.
    ''' </summary>
    ''' <param name="Query">The SQL statement to be executed. Default SQL syntax</param>
    Public Sub runQuery(ByRef Query As String)
        Try
            Dim dbCommand As New OdbcCommand(Query, dbConnection)
            dbCommand.ExecuteScalar()
            '/Console.WriteLine("[MYSQL:EXQUERY] " & Query)

        Catch ex As Exception
            Console.WriteLine("[MYSQL:EXQUERY] #QUERY: '" & Query & "' resulted in #ERROR: '" & ex.Message & "'")

        End Try
    End Sub
#End Region

#Region "Database data retrieval"
#Region "runRead"
    ''' <summary>
    ''' Performs a SQL query and returns the first selected field as string. Other fields are ignored.
    ''' </summary>
    ''' <param name="Query">The SQL query that selects a field</param>
    Public Function runRead(ByVal Query As String) As String
        Try
            Return New OdbcCommand(Query & " LIMIT 1", dbConnection).ExecuteScalar.ToString

        Catch ex As Exception
            Console.WriteLine("[MYSQL:RUNREAD] #QUERY: '" & Query & "' resulted in #ERROR: '" & ex.Message & "'")
            Return vbNullString
            

        End Try
    End Function
    ''' <summary>
    ''' Performs a SQL query and returns the first selected field as integer. Other fields are ignored.
    ''' </summary>
    ''' <param name="Query">The SQL query that selects a field</param>
    ''' <param name="Tick">Just to differ the runRead functions; supply a null (Nothing)</param>
    Public Function runRead(ByVal Query As String, ByVal Tick As Object) As Integer
        Try
            Return Integer.Parse(New OdbcCommand(Query & " LIMIT 1", dbConnection).ExecuteScalar)

        Catch ex As Exception
            Console.WriteLine("[MYSQL:RUNREAD] #QUERY: '" & Query & "' resulted in #ERROR: '" & ex.Message & "'")
            Return 0

        End Try
    End Function
#End Region
#Region "runReadColumn"
    ''' <summary>
    ''' Performs a SQL query and returns all vertical matching fields as a String array. Only the first supplied columname is looked for.
    ''' </summary>
    ''' <param name="Query">The SQL query that selects a column</param>
    ''' <param name="maxResults">Adds as LIMIT to the query. Using this, the array will never return more than xx fields in of the column. When maxResults is supplied as 0, then there is no max limit</param>
    Public Function runReadColumn(ByVal Query As String, ByVal maxResults As Integer) As String()
        If maxResults > 0 Then Query += " LIMIT " & maxResults
        Try
            Dim columnBuilder As New ArrayList
            Dim columnReader As OdbcDataReader = New OdbcCommand(Query, dbConnection).ExecuteReader()

            While columnReader.Read = True
                Try : columnBuilder.Add(columnReader(0).ToString)
                Catch : columnBuilder.Add(vbNullString) : End Try
            End While
            columnReader.Close()

            Return columnBuilder.ToArray(GetType(String))

        Catch ex As Exception
            Console.WriteLine("[MYSQL:RUNREADCOLUMN] #QUERY: '" & Query & "' resulted in #ERROR: '" & ex.Message & "'")
            Return New String(-1) {}

        End Try
    End Function
    ''' <summary>
    ''' Performs a SQL query and returns all vertical matching fields as an Integer array. Only the first supplied columname is looked for.
    ''' </summary>
    ''' <param name="Query">The SQL query that selects a column</param>
    ''' <param name="maxResults">Adds as LIMIT to the query. Using this, the array will never return more than xx fields in of the column. When maxResults is supplied as 0, then there is no max limit</param>
    ''' <param name="Tick">Just to differ the runReadColumn functions; supply a null (Nothing)</param>
    Public Function runReadColumn(ByVal Query As String, ByVal maxResults As Integer, ByVal Tick As Object) As Integer()
        If maxResults > 0 Then Query += " LIMIT " & maxResults
        Try
            Dim columnBuilder As New ArrayList
            Dim columnReader As OdbcDataReader = New OdbcCommand(Query, dbConnection).ExecuteReader()

            While columnReader.Read = True
                Try : columnBuilder.Add(columnReader.GetInt32(0))
                Catch : columnBuilder.Add(0) : End Try
            End While
            columnReader.Close()

            Return columnBuilder.ToArray(GetType(Integer))

        Catch ex As Exception
            Console.WriteLine("[MYSQL:RUNREADCOLUMN] #QUERY: '" & Query & "' resulted in #ERROR: '" & ex.Message & "'")
            Return New Integer(-1) {}

        End Try
    End Function
#End Region
#Region "runReadRow"
    ''' <summary>
    ''' Performs a SQL query and returns the selected in the first found row as a String array. Useable for only one row.
    ''' </summary>
    ''' <param name="Query">The SQL query that selects a row and the fields to get. LIMIT 1 is added</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function runReadRow(ByVal Query As String) As String()
        Try
            Dim rowBuilder As New ArrayList
            Dim rowReader As OdbcDataReader = New OdbcCommand(Query & " LIMIT 1", dbConnection).ExecuteReader()

            While rowReader.Read()
                For i = 0 To rowReader.FieldCount - 1
                    Try : rowBuilder.Add(rowReader(i).ToString)
                    Catch : rowBuilder.Add(vbNullString) : End Try
                Next
            End While
            rowReader.Close()

            'Console.WriteLine("[MYSQL:READROW] " & Query)
            Return rowBuilder.ToArray(GetType(String))

        Catch ex As Exception
            Console.WriteLine("[MYSQL:RUNREADROW] #QUERY: '" & Query & "' resulted in #ERROR: '" & ex.Message & "'")
            Return New String(-1) {}

        End Try
    End Function
    ''' <summary>
    ''' Performs a SQL query and returns the selected in the first found row as an Integer array. Useable for only one row.
    ''' </summary>
    ''' <param name="Query">The SQL query that selects a row and the fields to get. LIMIT 1 is added</param>
    ''' <param name="Tick">Just to differ the runReadRow functions; supply a null (Nothing)</param>
    Public Function runReadRow(ByVal Query As String, ByVal Tick As Object) As Integer()
        Try
            Dim rowBuilder As New ArrayList
            Dim rowReader As OdbcDataReader = New OdbcCommand(Query & " LIMIT 1", dbConnection).ExecuteReader()

            While rowReader.Read()
                For i = 0 To rowReader.FieldCount - 1
                    Try : rowBuilder.Add(rowReader.GetInt32(i))
                    Catch : rowBuilder.Add(0) : End Try
                Next
            End While
            rowReader.Close()

            'Console.WriteLine("[MYSQL:READROW] " & Query)
            Return rowBuilder.ToArray(GetType(Integer))

        Catch ex As Exception
            Console.WriteLine("[MYSQL:RUNREADROW] #QUERY: '" & Query & "' resulted in #ERROR: '" & ex.Message & "'")
            Return New Integer(-1) {}

        End Try
    End Function
#End Region
#End Region

#Region "Data availability checks"
    ''' <summary>
    ''' Tries to find fields matching the query. When there is at least one result, it returns True and stops.
    ''' </summary>
    ''' <param name="Query">The SQL query that contains the seeked fields and conditions. LIMIT 1 is added</param>
    Public Function checkExists(ByVal Query As String) As Boolean
        Try
            Return New OdbcCommand(Query & " LIMIT 1", dbConnection).ExecuteReader.HasRows

        Catch ex As Exception
            Console.WriteLine(Query)
            Console.WriteLine(ex.Message)
            Return False

        End Try
    End Function
#End Region

#Region "Input data manipulation"
    ''' <summary>
    ''' Returns a copy of thet input string that is safe for input in a SQL query, e.g. ' replaced by \'
    ''' </summary>
    ''' <param name="strInput">The input string to be fixed</param>
    Public Function safeString(ByVal strInput As String) As String
        Try
            strInput = strInput.Replace("\", "\\")
            strInput = strInput.Replace("'", "\'")
            Return strInput

        Catch
            Return vbNullString

        End Try
    End Function
#End Region
End Class
