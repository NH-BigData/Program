using Handler.SystemConfigHandler;
using System;
using System.Collections;
using System.Data;
using System.Data.Common;
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
                    CreateDBConnection(poolName);
                }
                else if (DBNull.Value.Equals(pool[poolName]))
                {
                    CreateDBConnection(poolName);
                }

                DbConnection ret = null;
                var i = 0;
                do
                {
                    ret = SearchPool(poolName);
                } while (ret == null && i++ < 300);

                return ret;
            }
        }

        public static void RemovePool(string poolName)
        {
            pool.Remove(poolName);
        }

        public static void Reload()
        {
            pool = new Hashtable();
        }

        internal DbConnection GetConnectionObject(string poolName)
        {
            DBConfig conf = null;
            if (confTmp == null)
                conf = new DBConfig();
            else
                conf = confTmp;

            return new SqlConnection(conf.GetConnectionString(poolName));
        }

        private DbConnection SearchPool(string poolName)
        {
            var temp = (DbConnection[])pool[poolName];

            for (var i = 0; i < temp.Length; i++)
                if (temp[0] != null)
                {
                    if (ConnectionState.Closed.Equals(temp[i].State))
                    {
                        temp[i].Dispose();
                        temp[i] = GetConnectionObject(poolName);
                        return temp[i];
                    }

                    if (ConnectionState.Open.Equals(temp[i].State)) return temp[i];
                }

            var conf = new DBConfig();
            if ("NO".Equals(conf[poolName + "_ALLOW_WAIT"].ToUpper())) return GetConnectionObject(poolName);

            return null;
        }

        private void CreateDBConnection(string poolName)
        {
            DbConnection[] conn = null;
            if (DBNull.Value.Equals(pool[poolName]))
            {
                conn = new DbConnection[1];
                for (var i = 0; i < conn.Length; i++) conn[i] = GetConnectionObject(poolName);
                pool[poolName] = conn;
            }
            else
            {
                var temp = (DbConnection[])pool[poolName];
                conn = new DbConnection[1];
                for (var i = 0; i < conn.Length; i++)
                    if (i >= temp.Length)
                        conn[i] = GetConnectionObject(poolName);
                    else
                        conn[i] = temp[i];
                pool[poolName] = conn;
            }
        }
    }
}
