using CalibrationNewGUI.Globalization;
using CalibrationNewGUI.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CalibrationNewGUI
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        static CultureInfo language;

        public App()
        {
            Directory.CreateDirectory(Environment.CurrentDirectory + @"\Config\CalPoint");
            Directory.CreateDirectory(Environment.CurrentDirectory + @"\Config\MeasurePoint");

            McuInfo.GetObj().Load();
            DmmInfo.GetObj().Load();
            AutoSaveInfo.GetObj().Load();
            CalMeasureInfo.GetObj().Load();
            ShuntInfo.GetObj().Load();
            OthersInfo.GetObj().Load();

            if(OthersInfo.GetObj().Language == "English")
                Thread.CurrentThread.CurrentUICulture = language = new CultureInfo("en-US");
            else
                Thread.CurrentThread.CurrentUICulture = language = new CultureInfo("ko-KR");
        }

        public static string GetString(string key)
        {
            return Resource.ResourceManager.GetString(key, language);
        }
    }
}
