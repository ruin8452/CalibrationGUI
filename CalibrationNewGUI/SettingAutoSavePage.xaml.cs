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
    /// SettingAutoSavePage.xaml에 대한 상호 작용 논리
    /// </summary>
    [ImplementPropertyChanged]
    public partial class SettingAutoSavePage : Page
    {
        public SettingData AllSetData { get; set; } //데이터 갱신을 위한 변수
        public SettingAutoSavePage()
        {
            InitializeComponent();
            AllSetData = SettingData.GetObj();
            DataContext = SettingData.GetObj();
        }
        private void AutoSavePageLoad(object sender, RoutedEventArgs e)
        {
            if (AllSetData.AutoSaveFlag == 1) AutoSaveSelect.IsChecked = true;
            else AutoSaveSelect.IsChecked = false;
        }
        private void AutoSaveSettingSaveBtn_Click(object sender, RoutedEventArgs e)
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
        //오토파일 세이브 설정
        private void AutoSaveSelect_Checked(object sender, RoutedEventArgs e)
        {
            AllSetData.AutoSaveFlag = 1;
        }
        private void AutoSaveSelect_Unchecked(object sender, RoutedEventArgs e)
        {
            AllSetData.AutoSaveFlag = 0;
        }


    }
}
