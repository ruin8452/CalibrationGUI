using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
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
    /// SettingCommPage.xaml에 대한 상호 작용 논리
    /// </summary>
    [ImplementPropertyChanged]
    public partial class SettingCommPage : Page
    {
        public SettingData AllSetData { get; set; } //데이터 갱신을 위한 변수
        public SettingCommPage()
        {
            InitializeComponent();
            DataContext = SettingData.GetObj();
            AllSetData = SettingData.GetObj();
        }
        //페이지 초기화
        private void CommPageLoad(object sender, EventArgs e)
        {
            //포트 초기화할것
            AllSetData.MCUPortNameList = new ObservableCollection<string>(SerialPort.GetPortNames());
            //if(AllSetData.MCUPortNameList>1)
            AllSetData.DMMPortNameList = new ObservableCollection<string>(SerialPort.GetPortNames());
            if (AllSetData.DMMOffsetUseFlag == 1) DMMOffsetSelect.IsChecked = true;
            else DMMOffsetSelect.IsChecked = false;
        }
        //설정 저장하기
        private void CommSettingSaveBtn_Click(object sender, RoutedEventArgs e)
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
        //DMM Offset사용할때 체크함수
        private void DMMOffsetSelectCheck(object sender, RoutedEventArgs e)
        {
            AllSetData.DMMOffsetUseFlag = 1;
        }
        private void DMMOffsetSelectUnCheck(object sender, RoutedEventArgs e)
        {
            AllSetData.DMMOffsetUseFlag = 0;
        }
    }
}
