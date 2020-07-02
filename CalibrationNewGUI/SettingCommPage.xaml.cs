using PropertyChanged;
using System;
using System.Collections.Generic;
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
        }
        //페이지 초기화
        private void CommPageInit(object sender, EventArgs e)
        {
            AllSetData = SettingData.GetObj();
            //포트 초기화할것
            MCUPortNameCombo.ItemsSource = SerialPort.GetPortNames();
            DMMPortNameCombo.ItemsSource = SerialPort.GetPortNames();
        }
        //설정 저장하기
        private void CommSettingSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MCUPortNameCombo.SelectedIndex != -1) AllSetData.MCUPortName = MCUPortNameCombo.SelectedItem.ToString();
                else AllSetData.MCUPortName = "0";
                if (DMMPortNameCombo.SelectedIndex != -1) AllSetData.DMMPortName = DMMPortNameCombo.SelectedItem.ToString();
                else AllSetData.DMMPortName = "0";
                //AllSetData.MCUPortName = MCUPortNameCombo.SelectedItem.ToString();
                //AllSetData.DMMPortName = DMMPortNameCombo.SelectedItem.ToString();

                AllSetData.MCUBorate   = Convert.ToInt32(MCUBorateCombo.SelectedItem.ToString(), 10);
                AllSetData.MCUDataBit  = Convert.ToInt32(MCUDataBitCombo.SelectedItem.ToString(), 10);
                AllSetData.MCUParity   = MCUParityCombo.SelectedItem.ToString();
                AllSetData.MCUStopBit  = Convert.ToInt32(MCUStopBitCombo.SelectedItem.ToString(), 10);
                
                AllSetData.DMMModel    = DMMModelCombo.SelectedItem.ToString();
                AllSetData.DMMBorate   = Convert.ToInt32(DMMBorateCombo.SelectedItem.ToString(), 10);
                AllSetData.DMMDataBit  = Convert.ToInt32(DMMDataBitCombo.SelectedItem.ToString(), 10);
                AllSetData.DMMParity   = DMMParityCombo.SelectedItem.ToString();
                AllSetData.DMMStopBit  = Convert.ToInt32(DMMStopBitCombo.SelectedItem.ToString(), 10);
                //AllSetData.MCUFlowCtrl = MCUParityCombo.SelectedItem.ToString();
                //AllSetData.DMMFlowCtrl
                

                if (DMMOffsetSelect.IsChecked == true) AllSetData.DMMOffsetUseFlag = 1;
                else                                  AllSetData.DMMOffsetUseFlag = 0;
            }
            catch (NullReferenceException ex)
            {
                string errormsg = "설정을 확인하세요.";
                MessageBox.Show(errormsg);
            }
        }
    }
}
