﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CalibrationNewGUI.UI
{
    /// <summary>
    /// MonitorPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MonitorPage : Page
    {
        public MonitorPage()
        {
            InitializeComponent();
            //DataContext = new MonitorPageVM();
        }

        private void Border_LayoutUpdated(object sender, EventArgs e)
        {
            double height = LogBorder.ActualHeight - 31;

            if (height > 0)
                LogScroll.Height = height;
            else
                LogScroll.Height = 0;
        }

    }
}
