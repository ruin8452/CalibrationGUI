using CalibrationNewGUI.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CalibrationNewGUI
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
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
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ko-KR");
        }
    }
}
