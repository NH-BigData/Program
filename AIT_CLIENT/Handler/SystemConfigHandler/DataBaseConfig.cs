using System.Diagnostics;
using System.Text;
using Handler.SecureHandler;

namespace Handler.SystemConfigHandler
{
    public class DBConfig
    {
        private readonly IniHandler ini = new IniHandler();

        public string this[string indexStr]
        {
            get
            {
                Trace.WriteLine(string.Format("DBConfig > get prop[{0}], INI: {1}", indexStr, ini.iniPath));
                return ini.ReadIni("DATABASE", indexStr);
            }
            set
            {
                Trace.WriteLine(string.Format("DBConfig > set prop[{0}]={1}", indexStr, value));
                ini.WriteIni("DATABASE", indexStr, value);
            }
        }

        public DBConfig(string sysConfigPath)
        {
            ini = new IniHandler(sysConfigPath);
        }

        public string getConnectionString(string poolName)
        {
            var so = new SecureObject();
            var vendor = this[poolName + "_VENDOR"];
            var dbName = this[poolName];
            var svrName = this[poolName + "_SVR"];
            var port = this[poolName + "_PORT"];
            var uid = this[poolName + "_UID"];
            var pwd = this[poolName + "_PWD"];
            try
            {
                pwd = so.GetDecryptString(this[poolName + "_PWD"]);
            }
            catch
            {
                this[poolName + "_PWD"] = so.GetEncryptString(pwd);
            }

            return GetConnectionString(vendor, dbName, svrName, port, uid, pwd);
        }

        public string GetConnectionString
            (string vendor, string dbName, string svrName, string port, string uid, string pwd)
        {
            Trace.WriteLine(string.Format("DBConfig > getConnectionString({0}, {1}, {2} ...)", vendor, dbName,
                svrName));

            var ret = new StringBuilder("Data Source=").Append(svrName).Append(",").Append(port)
                    .Append(";Initial Catalog=").Append(dbName)
                    .Append(";User Id=").Append(uid)
                    .Append(";Password=").Append(pwd).ToString();
            return ret;
        }
    }
}
