using System;
using System.Data.Odbc;
using System.Collections;

namespace Holo
{
    /// <summary> 
    /// Provides high speed data access to the MySQL database of Holograph Emulator. It owns eh? 
    /// </summary> 
    /// <remarks></remarks> 
    public static class DB
    {
        private static OdbcConnection dbConnection;
        #region Database connection management
        /// <summary> 
        /// Opens connection to the MySQL database with the supplied parameters, and returns a 'true' boolean when the connection has succeeded. Requires MySQL ODBC 5.1 driver to be installed. 
        /// </summary> 
        /// <param name="dbHost">The hostname/IP address where the database server is located.</param> 
        /// <param name="dbPort">The port the database server is running on.</param> 
        /// <param name="dbName">The name of the database.</param> 
        /// <param name="dbUsername">The username for authentication with the database.</param> 
        /// <param name="dbPassword">The pasword for authentication with the database.</param> 
        public static bool openConnection(string dbHost, int dbPort, string dbName, string dbUsername, string dbPassword)
        {
            try
            {
                Out.WriteLine("Connecting to " + dbName + " at " + dbHost + ":" + dbPort + " for user '" + dbUsername + "'");
                dbConnection = new OdbcConnection("Driver={MySQL ODBC 5.1 Driver};Server=" + dbHost + ";Port=" + dbPort + ";Database=" + dbName + ";User=" + dbUsername + ";Password=" + dbPassword + ";Option=3;");
                dbConnection.Open();
                Out.WriteLine("Connection to database successfull.");
                return true;
            }

            catch (Exception ex)
            {
                Out.WriteError("Failed to connect! Error thrown was: " + ex.Message);
                return false;
            }
        }
        /// <summary> 
        /// Closes connection with the MySQL database. Any errors are ignored. 
        /// </summary> 
        public static void closeConnection()
        {
            Out.WriteLine("Closing database connection...");
            try
            {
                dbConnection.Close();
                Out.WriteLine("Database connection closed.");
            }
            catch { Out.WriteLine("No database connection."); }
        }
        #endregion

        #region Database data manipulation
        /// <summary> 
        /// Executes a SQL statement on the database. 
        /// </summary> 
        /// <param name="Query">The SQL statement to be executed. Default SQL syntax.</param> 
        public static void runQuery(string Query)
        {
            try { new OdbcCommand(Query, dbConnection).ExecuteScalar(); }
            catch (Exception ex) { Out.WriteError("Error '" + ex.Message + "' at '" + Query + "'"); }
        }
        #endregion

        #region Database data retrieval"
        #region runRead
        /// <summary> 
        /// Performs a SQL query and returns the first selected field as string. Other fields are ignored. 
        /// </summary> 
        /// <param name="Query">The SQL query that selects a field.</param> 
        public static string runRead(string Query)
        {
            try { return new OdbcCommand(Query + " LIMIT 1", dbConnection).ExecuteScalar().ToString(); }
            catch (Exception ex)
            {
                Out.WriteError("Error '" + ex.Message + "' at '" + Query + "'");
                return "";
            }
        }
        /// <summary> 
        /// Performs a SQL query and returns the first selected field as integer. Other fields are ignored. 
        /// </summary> 
        /// <param name="Query">The SQL query that selects a field.</param> 
        /// <param name="Tick">Just to differ the runRead functions; supply a null if you want to use this overload.</param> 
        public static int runRead(string Query, object Tick)
        {
            try { return Convert.ToInt32(new OdbcCommand(Query + " LIMIT 1", dbConnection).ExecuteScalar()); }
            catch (Exception ex)
            {
                Out.WriteError("Error '" + ex.Message + "' at '" + Query + "'");
                return 0;
            }
        }
        #endregion
        #region runReadColumn
        /// <summary> 
        /// Performs a SQL query and returns all vertical matching fields as a String array. Only the first supplied columname is looked for. 
        /// </summary> 
        /// <param name="Query">The SQL query that selects a column.</param> 
        /// <param name="maxResults">Adds as LIMIT to the query. Using this, the array will never return more than xx fields in of the column. When maxResults is supplied as 0, then there is no max limit.</param> 
        public static string[] runReadColumn(string Query, int maxResults)
        {
            if (maxResults > 0)
                Query += " LIMIT " + maxResults;
            
            try
            {
                ArrayList columnBuilder = new ArrayList();
                OdbcDataReader columnReader = new OdbcCommand(Query, dbConnection).ExecuteReader();

                while (columnReader.Read())
                {
                    try { columnBuilder.Add(columnReader[0].ToString()); }
                    catch { columnBuilder.Add(""); }
                }
                columnReader.Close();

                return (string[])columnBuilder.ToArray(typeof(string));
            }

            catch (Exception ex)
            {
                Out.WriteError("Error '" + ex.Message + "' at '" + Query + "'");
                return new string[0];
            }
        }
        /// <summary> 
        /// Performs a SQL query and returns all vertical matching fields as an Integer array. Only the first supplied columname is looked for. 
        /// </summary> 
        /// <param name="Query">The SQL query that selects a column.</param> 
        /// <param name="maxResults">Adds as LIMIT to the query. Using this, the array will never return more than xx fields in of the column. When maxResults is supplied as 0, then there is no max limit.</param> 
        /// <param name="Tick">Just to differ the runReadColumn functions; supply a null if you want to use this overload.</param> 
        public static int[] runReadColumn(string Query, int maxResults, object Tick)
        {
            if (maxResults > 0)
                Query += " LIMIT " + maxResults;
            try
            {
                ArrayList columnBuilder = new ArrayList();
                OdbcDataReader columnReader = new OdbcCommand(Query, dbConnection).ExecuteReader();

                while (columnReader.Read())
                {
                    try { columnBuilder.Add(columnReader.GetInt32(0)); }
                    catch { columnBuilder.Add(0); }
                }
                columnReader.Close();
                return (int[])columnBuilder.ToArray(typeof(int));
            }

            catch (Exception ex)
            {
                Out.WriteError("Error '" + ex.Message + "' at '" + Query + "'");
                return new int[0];

            }
        }
        #endregion
        #region runReadRow
        /// <summary> 
        /// Performs a SQL query and returns the selected in the first found row as a String array. Useable for only one row. 
        /// </summary> 
        /// <param name="Query">The SQL query that selects a row and the fields to get. LIMIT 1 is added.</param> 
        public static string[] runReadRow(string Query)
        {
            try
            {
                ArrayList rowBuilder = new ArrayList();
                OdbcDataReader rowReader = new OdbcCommand(Query + " LIMIT 1", dbConnection).ExecuteReader();

                while (rowReader.Read())
                {
                    for (int i = 0; i < rowReader.FieldCount; i++)
                    {
                        try { rowBuilder.Add(rowReader[i].ToString()); }
                        catch { rowBuilder.Add(""); }
                    }
                }
                rowReader.Close();
                return (string[])rowBuilder.ToArray(typeof(string));
            }

            catch (Exception ex)
            {
                Out.WriteError("Error '" + ex.Message + "' at '" + Query + "'");
                return new string[0];
            }
        }
        /// <summary> 
        /// Performs a SQL query and returns the selected in the first found row as an Integer array. Useable for only one row. 
        /// </summary> 
        /// <param name="Query">The SQL query that selects a row and the fields to get. LIMIT 1 is added.</param> 
        /// <param name="Tick">Just to differ the runReadRow functions; supply a null if you want to use this overload.</param> 
        public static int[] runReadRow(string Query, object Tick)
        {
            try
            {
                ArrayList rowBuilder = new ArrayList();
                OdbcDataReader rowReader = new OdbcCommand(Query + " LIMIT 1", dbConnection).ExecuteReader();

                while (rowReader.Read())
                {
                    for (int i = 0; i < rowReader.FieldCount; i++)
                    {
                        try { rowBuilder.Add(rowReader.GetInt32(i)); }
                        catch { rowBuilder.Add(0); }
                    }
                }
                rowReader.Close();
                return (int[])rowBuilder.ToArray(typeof(int));
            }

            catch (Exception ex)
            {
                Out.WriteError("Error '" + ex.Message + "' at '" + Query + "'");
                return new int[0];
            }
        }
        /// <summary>
        /// Performs a SQL query and returns the result as a string. On error, no error is reported and "" is returned.
        /// </summary>
        /// <param name="Query">The SQL query to run. LIMIT 1 is added.</param>
        public static string runReadUnsafe(string Query)
        {
            try { return new OdbcCommand(Query + " LIMIT 1", dbConnection).ExecuteScalar().ToString(); }
            catch { return ""; }
        }
        /// <summary>
        /// Performs a SQL query and returns the result as an integer. On error, no error is reported and 0 is returned.
        /// </summary>
        /// <param name="Query">The SQL query to run. LIMIT 1 is added.</param>
        /// <param name="Tick">Just to differ the runReadUnsafe functions; supply a null if you want to use this overload.</param> 
        public static int runReadUnsafe(string Query, object Tick)
        {
            try { return Convert.ToInt32(new OdbcCommand(Query + " LIMIT 1", dbConnection).ExecuteScalar()); }
            catch { return 0; }
        }
        #endregion
        #endregion

        #region Data availability checks
        /// <summary> 
        /// Tries to find fields matching the query. When there is at least one result, it returns True and stops. 
        /// </summary> 
        /// <param name="Query">The SQL query that contains the seeked fields and conditions. LIMIT 1 is added.</param> 
        public static bool checkExists(string Query)
        {
            try { return new OdbcCommand(Query + " LIMIT 1", dbConnection).ExecuteReader().HasRows; }
            catch (Exception ex)
            {
                Out.WriteError("Error '" + ex.Message + "' at '" + Query + "'");
                return false;
            }
        }
        #endregion

        #region Misc
        /// <summary> 
        /// Returns a stripslashed copy of the input string.
        /// </summary> 
        /// <param name="Query">The string to add stripslashes to.</param>
        public static string Stripslash(string Query)
        {
            try { return Query.Replace(@"\", "\\").Replace("'", @"\'"); }
            catch { return ""; }
        }
        #endregion
    }
}