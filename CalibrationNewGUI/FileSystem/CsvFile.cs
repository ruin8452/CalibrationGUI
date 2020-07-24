using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.FileSystem
{
    public class CsvFile
    {
        public static bool Save(string filePath, bool overWrite, object[] data)
        {
            using (StreamWriter csvStream = new StreamWriter(filePath, overWrite, Encoding.UTF8))
            {
                csvStream.WriteLine(string.Join(",", data));
            }

            return true;
        }
    }
}
