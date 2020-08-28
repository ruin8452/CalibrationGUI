using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.FileSystem
{
    public class Log
    {
        public static bool LogSave(string text)
        {
            try
            {
                string path = string.Format($@"{Environment.CurrentDirectory}\Logs\{DateTime.Now:yyyy-MM-dd HH.mm.ss}_Log.log");
                using (StreamWriter csvStream = new StreamWriter(path, true, Encoding.UTF8))
                {
                    csvStream.WriteLine(text);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
