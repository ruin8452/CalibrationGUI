using System;
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
using System.Windows.Shapes;
using System.ComponentModel;//윈도우 cancel이벤트

namespace CalibrationNewGUI
{
    /// <summary>
    /// TestSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TestSetting : Window
    {
        public string portNameMCU;
        public string portNameDMM;

        public TestSetting()
        {
            InitializeComponent();
            this.Closing += new CancelEventHandler(CloseWindow);
        }
        private void CloseWindow(object sender, CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
        private void SettingWindow(object sender, RoutedEventArgs e)
        {

        }
        private void SettingSend(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PortNameComboMCU.SelectedIndex != -1) portNameMCU = PortNameComboMCU.SelectedItem.ToString();
                else portNameMCU = "0";
                if (PortNameComboDMM.SelectedIndex != -1) portNameDMM = PortNameComboDMM.SelectedItem.ToString();
                else portNameDMM = "0";
                TestSetting.GetWindow(this).Hide();
            }
            catch (NullReferenceException ex)
            {
                
            }
        }
    }
}
