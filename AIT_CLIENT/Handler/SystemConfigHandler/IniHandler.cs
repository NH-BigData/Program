using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Handler.SystemConfigHandler
{
    public class IniHandler
    {
        public string iniPath = string.Empty;

        public IniHandler() 
            : this(new StringBuilder(AppDomain.CurrentDomain.BaseDirectory).Append("SystemConfig.ini").ToString())
        { 
        
        }

        public IniHandler(string path)
        {
            iniPath = path;
        }

        [DllImport("Kernel32")]
        private static extern long WritePrivateProfileString(string section,
            string key,
            string val,
            string filePath);

        [DllImport("Kernel32")]
        private static extern int GetPrivateProfileString(string section,
            string key,
            string def,
            StringBuilder retVal,
            int size,
            string filePath);

        public void WriteIni(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, iniPath);
        }

        public string ReadIni(string section, string key)
        {
            var temp = new StringBuilder(512);
            var i = GetPrivateProfileString(section, key, "", temp,
                512, iniPath);
            return temp.ToString();
        }
    }
}
