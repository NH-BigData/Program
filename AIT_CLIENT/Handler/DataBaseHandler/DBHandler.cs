using Handler.SystemConfigHandler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace Handler.DataBaseHandler
{
    public class DBHandler
    {
        public const int PAGING_SIZE = 100;
        protected const string argSymbol = "@";

        public const string EXIT_CODE = "_ExitCode_";
        public const string PROC_MESSAGE = "_ProcessMessage_";
        private static readonly Hashtable self = new Hashtable();
        public static string defaultConnName = "TargetDB";
        internal DbTransaction activeTransaction;
        public string conectionString = string.Empty;
        public bool initialzieFlag = true;
        public string vendor = string.Empty;

        public string poolName { get; set; }
        public DbConnection activeConnection { get; set; }

        private DBHandler()
        {
            var stackTrace = new StackTrace();
            string methodName = (stackTrace.GetFrames().Length > 5)
                ? stackTrace.GetFrame(5).GetMethod().ToString()
                : stackTrace.GetFrame(1).GetMethod().ToString();

            Trace.WriteLine(string.Format("{0:HH:mm:ss.fff} DBHandler() Constructed from [{1}]",
                DateTime.Now, methodName));
            
            var conf = new DBConfig();
            poolName = defaultConnName;
            vendor = conf[poolName + "_VENDOR"];
            conectionString = conf.GetConnectionString(poolName);
            InitializeConnection();
            activeConnection.Open();
        }

        private DBHandler(string poolName, string iniPath)
        {
            var stackTrace = new StackTrace();
            string methodName = (stackTrace.GetFrames().Length > 5)
                ? stackTrace.GetFrame(5).GetMethod().ToString()
                : stackTrace.GetFrame(1).GetMethod().ToString(); 

            Trace.WriteLine(string.Format(
                "{0:HH:mm:ss.fff} DBHandler(poolName:{1}, iniPath:{2}) Constructed from [{3}]",
                DateTime.Now, poolName, iniPath, methodName));

            var conf = new DBConfig();
            this.poolName = poolName;
            vendor = conf[this.poolName + "_VENDOR"];
            conectionString = conf.GetConnectionString(poolName);

            Trace.WriteLine(string.Format("{0:HH:mm:ss.fff} DBHandler > 접속문자열:{1}", DateTime.Now, conectionString));           
           
            InitializeConnection();
            try
            {
                if (activeConnection != null && ConnectionState.Closed.Equals(activeConnection.State))
                    activeConnection.Open();
            }
            catch (Exception ex)
            {
                initialzieFlag = false;
                MessageBox.Show(ex.Message);
            }            
        }

        ~DBHandler()
        {
            DataBaseClose();
        }

        public static bool TestDB(string vendor, string dbsvr, string dbport, string dbname, string id, string pwd)
        {
            var conf = new DBConfig();
            var connStr = conf.GetConnectionString(vendor, dbname, dbsvr, dbport, id, pwd);
            DbConnection conn = new SqlConnection();

            try
            {
                conn.ConnectionString = connStr;
                conn.Open();

                if (ConnectionState.Open.Equals(conn.State)) return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(new StringBuilder(ex.Message).AppendLine(ex.StackTrace).ToString());
                Debug.Flush();
                return false;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
            return false;
        }

        public static DBHandler GetInstanse(string poolName)
        {
            var stackTrace = new StackTrace();
            string methodName = (stackTrace.GetFrames().Length > 5)
                ? stackTrace.GetFrame(5).GetMethod().ToString()
                : stackTrace.GetFrame(1).GetMethod().ToString();

            Trace.WriteLine(string.Format("{0:HH:mm:ss.fff} DBHandler > getInstanse(poolName:{1}) from [{2}]",
                DateTime.Now, poolName, methodName));

            lock (self)
            {
                var ret = (DBHandler)self[poolName];
                if (ret == null)
                {
                    ret = new DBHandler(poolName, null);

                    if ("".Equals(ret.conectionString))
                        return null;
                    self.Add(poolName, ret);
                }
                return ret;
            }
        }

        public static DBHandler GetInstanse()
        {
            return GetInstanse(defaultConnName);
        }

        public static DBHandler GetInstanse(string iniPath, int type)
        {
            return GetInstanse(defaultConnName);
        }

        public void DataBaseClose()
        {
            self[poolName] = null;
            self.Remove(poolName);

            if (activeTransaction != null)
            {
                activeTransaction.Dispose();
                activeTransaction = null;
            }

            if (activeConnection != null)
            {
                if (ConnectionState.Open.Equals(activeConnection.State))
                    try
                    {
                        activeConnection.Close();
                        activeConnection.Dispose();
                    }
                    catch
                    {
                        activeConnection = null;
                    }

                activeConnection = null;
            }

            RemovePoolName(poolName);
        }

        private void InitializeConnection()
        {
            activeConnection = new ConnectionPool()[poolName];
            if (activeConnection == null) throw new Exception("Database Pool not found");
        }

        public void OpenTransaction()
        {
            activeTransaction = activeConnection.BeginTransaction();
        }

        public bool IsOpen()
        {
            if (activeTransaction != null && activeTransaction.Connection != null &&
                activeTransaction.Connection.State == ConnectionState.Open)
                return true;
            return false;
        }

        public void CommitTransaction()
        {
            try
            {
                if (activeTransaction != null)
                    activeTransaction.Commit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }

        public void RollBackTransaction()
        {
            try
            {
                activeTransaction.Rollback();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }

        public void RemovePoolName(string name)
        {
            self.Remove(name);
            ConnectionPool.RemovePool(name);
        }

        public DbCommand CreateCommand(string sqlText, object[] args)
        {
            return CreateCommand(vendor.ToUpper(), activeConnection, sqlText, args, activeTransaction);
        }

        internal DbCommand CreateCommand(string ven, DbConnection conn, string sqlText, object[] args)
        {
            return CreateCommand(ven, conn, sqlText, args, null);
        }

        internal DbCommand CreateCommand(string ven, DbConnection conn, string sqlText, object[] args,
            DbTransaction tran)
        {
            var command = conn.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sqlText;
            if (tran != null) command.Transaction = tran;

            for (var i = 0; i < args.Length; i++)
            {
                var idxParam = command.CommandText.IndexOf('?');
                if (idxParam >= 0)
                {
                    command.CommandText = command.CommandText.Substring(0, idxParam)
                                            + argSymbol + (i + 1) + command.CommandText.Substring(idxParam + 1);
                    ((SqlCommand)command).Parameters.Add(
                        new SqlParameter(argSymbol + (i + 1), null == args[i] ? DBNull.Value : args[i]));
                }
            }
            TraceQueryText(command);

            return command;
        }

        public string[][] ReaderToStringArray(DbDataReader reader)
        {
            var ret = new List<string[]>();

            if (reader != null && reader.HasRows)
                while (reader.Read())
                {
                    var tempArr = new object[reader.FieldCount];
                    var temp = new string[reader.FieldCount];
                    reader.GetValues(tempArr);
                    for (var i = 0; i < tempArr.Length; i++) temp[i] = tempArr[i].ToString();
                    ret.Add(temp);
                }

            return ret.ToArray();
        }

        public DataTable ReaderToDataTable(DbDataReader reader)
        {
            var ret = new DataTable();
            if (reader != null)
            {
                var columns = reader.GetSchemaTable();
                for (var i = 0; i < columns.Rows.Count; i++)
                {
                    var col = ret.Columns.Add(columns.Rows[i][0].ToString());
                    col.DataType = typeof(object);
                }

                if (reader.HasRows)
                    while (reader.Read())
                    {
                        object[] tempArr = new object[reader.FieldCount];
                        reader.GetValues(tempArr);
                        DataRow tempDR = ret.NewRow();
                        for (var i = 0; i < tempArr.Length; i++)
                            if (DBNull.Value.Equals(tempArr[i]))
                            {
                                if (tempDR.Table.Columns[i].DataType.FullName.Equals(typeof(string).FullName))
                                    tempDR[i] = "";
                                else
                                    tempDR[i] = tempArr[i];
                            }
                            else
                            {
                                tempDR[i] = tempArr[i];
                            }

                        ret.Rows.Add(tempDR);
                    }
            }
            return ret;
        }

        public DataTable Paging(DbDataReader reader)
        {
            var ret = new DataTable();
            if (reader != null)
            {
                var columns = reader.GetSchemaTable();
                for (var i = 0; i < columns.Rows.Count; i++) ret.Columns.Add(columns.Rows[i][0].ToString());

                if (reader.HasRows)
                {
                    var pagingSize = 0;
                    while (pagingSize < PAGING_SIZE && reader.Read())
                    {
                        var tempArr = new object[reader.FieldCount];
                        reader.GetValues(tempArr);
                        var tempDR = ret.NewRow();
                        for (var i = 0; i < tempArr.Length; i++) tempDR[i] = tempArr[i];
                        ret.Rows.Add(tempDR);
                        pagingSize++;
                    }
                }
            }
            return ret;
        }

        public Process GetProcess()
        {
            var procInfo = new ProcessStartInfo();
            procInfo.UseShellExecute = false;
            procInfo.RedirectStandardError = true;
            procInfo.RedirectStandardOutput = true;
            procInfo.RedirectStandardInput = true;
            procInfo.CreateNoWindow = true;
            procInfo.WindowStyle = ProcessWindowStyle.Hidden;
            procInfo.ErrorDialog = true;
            procInfo.StandardOutputEncoding = Encoding.UTF8;
            procInfo.StandardErrorEncoding = Encoding.UTF8;

            var ret = new Process();
            ret.StartInfo = procInfo;

            return ret;
        }

        internal DbConnection ConnectionForAdater()
        {
            var connection = new ConnectionPool().GetConnectionObject(poolName);
            return connection;
        }

        private void TraceQueryText(DbCommand command)
        {
            var nCount = command.Parameters.Count;
            var S = command.CommandText;

            for (var K = command.Parameters.Count - 1; K >= 0; K--)
            {
                var param = command.Parameters[K];
                var val = string.Empty;
                if (param.Value.Equals(DBNull.Value))
                    val = "NULL";
                else if (param.DbType.Equals(DbType.String) || param.DbType.Equals(DbType.DateTime))
                    val = "'" + Convert.ToString(param.Value).Replace(@"\", @"\\").Replace("'", @"\'") + "'";
                else
                    val = param.Value.ToString();

                S = S.Replace(param.ParameterName, val);
            }

            var stackTrace = new StackTrace();
            string methodName = (stackTrace.GetFrames().Length > 5)
                ? stackTrace.GetFrame(5).GetMethod().ToString()
                : stackTrace.GetFrame(1).GetMethod().ToString();

            Trace.WriteLine(string.Format("-- {0:HH:mm:ss.fff} 실행쿼리: [{1}]\n{2}",
                DateTime.Now, methodName, S));
            Trace.WriteLine(string.Format("{0:yyyy-MM-dd HH:mm:ss.fff} | 실행 쿼리:\n{1}", DateTime.Now, S));
        }
    }
}
