using PropertyChanged;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CalibrationNewGUI
{
    /// <summary>
    /// SettingCalPage.xaml에 대한 상호 작용 논리
    /// </summary>
    [ImplementPropertyChanged]
    public partial class SettingCalPage : Page
    {
        public SettingData AllSetData { get; set; } //데이터 갱신을 위한 변수
        public SettingCalPage()
        {
            InitializeComponent();
            AllSetData = SettingData.GetObj();
            DataContext = SettingData.GetObj();
        }

        private void CalSettingSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            int saveOK = 0;
            saveOK = AllSetData.SaveFile();
            if (saveOK == 1)
            {
                string errormsg = "저장 성공";
                MessageBox.Show(errormsg);
            }
            else
            {
                string errormsg = "저장 실패";
                MessageBox.Show(errormsg);
            }
        }
    }
}
