using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CalibrationNewGUI.Globalization
{
    public class LocalizationLib
    {
        public static string GetLocalizaionString(string key)
        {
            string uiString;

            ResourceManager rm = new ResourceManager("CalibrationNewGUI.Globalization.Resources", Assembly.GetExecutingAssembly());

            uiString = rm.GetString(key);
            return uiString;
        }

    }
}
