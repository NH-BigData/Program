using Handler.SystemConfigHandler;
using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;

namespace Handler.DataBaseHandler
{
    internal class ConnectionPool
    {
        private static Hashtable pool = new Hashtable();

        private readonly DBConfig confTmp;

        public ConnectionPool()
        {

        }

        public ConnectionPool(DBConfig _DBConfig)
        {
            confTmp = _DBConfig;
        }

        public DbConnection this[string poolName]
        {
            get
            {
                if (pool[poolName] == null)
                {
                    pool.Add(poolName, DBNull.Value);
                    createDBConnection(poolName);
                }
                else if (DBNull.Value.Equals(pool[poolName]))
                {
                    createDBConnection(poolName);
                }

                DbConnection ret = null;
                var i = 0;
                do
                {
                    ret = searchIdlePool(poolName);
                } while (ret == null && i++ < 300);

                return ret;
            }
        }

        public static void removePool(string poolName)
        {
            pool.Remove(poolName);
        }

        public static void reload()
        {
            pool = new Hashtable();
        }

        internal DbConnection getConnectionObject(string poolName)
        {
            DBConfig conf = null;
            if (confTmp == null)
                conf = new DBConfig();
            else
                conf = confTmp;


            return new SqlConnection(conf.GetConnectionString(poolName));
        }

        private DbConnection searchIdlePool(string poolName)
        {
            var temp = (DbConnection[])pool[poolName];

            for (var i = 0; i < temp.Length; i++)
                if (temp[0] != null)
                {
                    if (ConnectionState.Closed.Equals(temp[i].State))
                    {
                        temp[i].Dispose();
                        temp[i] = getConnectionObject(poolName);
                        return temp[i];
                    }

                    if (ConnectionState.Open.Equals(temp[i].State)) return temp[i];
                }

            var conf = new DBConfig();
            if ("NO".Equals(conf[poolName + "_ALLOW_WAIT"].ToUpper())) return getConnectionObject(poolName);

            return null;
        }

        private void createDBConnection(string poolName)
        {
            DbConnection[] conn = null;
            if (DBNull.Value.Equals(pool[poolName]))
            {
                conn = new DbConnection[1];
                for (var i = 0; i < conn.Length; i++) conn[i] = getConnectionObject(poolName);
                pool[poolName] = conn;
            }
            else
            {
                var temp = (DbConnection[])pool[poolName];
                conn = new DbConnection[1];
                for (var i = 0; i < conn.Length; i++)
                    if (i >= temp.Length)
                        conn[i] = getConnectionObject(poolName);
                    else
                        conn[i] = temp[i];
                pool[poolName] = conn;
            }
        }
    }
}
