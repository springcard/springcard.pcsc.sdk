using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace SpringCard.LibCs.Windows
{
    public class Kernel32
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(String lpFileName, UInt32 dwDesiredAccess, Int32 dwShareMode, IntPtr lpSecurityAttributes, Int32 dwCreationDisposition, Int32 dwFlagsAndAttributes, Int32 hTemplateFile);

        public const Int32 FILE_SHARE_READ = 1;
        public const Int32 FILE_SHARE_WRITE = 2;
        public const uint GENERIC_READ = 0X80000000U;
        public const Int32 GENERIC_WRITE = 0X40000000;
        public const Int32 INVALID_HANDLE_VALUE = -1;
        public const Int32 OPEN_EXISTING = 3;


        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public string IniReadValue(string filePath, string section, string key, string def)
        {
            StringBuilder temp = new StringBuilder(255);

            int i = GetPrivateProfileString(section, key, def, temp, 255, filePath);

            return temp.ToString();
        }

        
    }
}
