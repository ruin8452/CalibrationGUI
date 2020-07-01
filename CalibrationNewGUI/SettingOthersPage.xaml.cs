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
    /// SettingOthersPage.xaml에 대한 상호 작용 논리
    /// </summary>
    [ImplementPropertyChanged]
    public partial class SettingOthersPage : Page
    {
        public SettingData AllSetData { get; set; } //데이터 갱신을 위한 변수
        public SettingOthersPage()
        {
            InitializeComponent();
            AllSetData = SettingData.GetObj();
            DataContext = SettingData.GetObj();
        }

        private void OtherSettingSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //ini파일 저장
            }
            catch (NullReferenceException ex)
            {
                string errormsg = "설정을 확인하세요.";
                MessageBox.Show(errormsg);
            }
        }
    }
}
