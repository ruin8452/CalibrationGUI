using CalibrationNewGUI.Equipment;
using CalibrationNewGUI.Equipment.DigitalMeter;
using CalibrationNewGUI.Model;
using J_Project.Communication.CommFlags;
using J_Project.Communication.CommModule;
using J_Project.ViewModel.CommandClass;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CalibrationNewGUI.VeiwModel
{
    [ImplementPropertyChanged]
    class MainWindowVM
    {
        public SolidColorBrush McuConnColor { get; set; }
        public SolidColorBrush DmmConnColor { get; set; }
        public bool IsAllConnected { get; set; } = false;

        public string GuiVersion { get; set; }

        public McuInfo McuInfos { get; set; }
        public DmmInfo DmmInfos { get; set; }

        public Mcu Mcu { get; set; }
        public Dmm Dmm { get; set; }

        public ICommand CommConnect { get; set; }

        public MainWindowVM()
        {
            GuiVersion = "1.0.0";

            McuConnColor = Application.Current.Resources["LedGreenOff"] as SolidColorBrush;
            DmmConnColor = Application.Current.Resources["LedGreenOff"] as SolidColorBrush;

            McuInfos = McuInfo.GetObj();
            DmmInfos = DmmInfo.GetObj();

            Mcu = Mcu.GetObj();
            Dmm = Dmm.GetObj();

            CommConnect = new BaseCommand(ConnectBtn);
        }

        //통신 연결 버튼
        private void ConnectBtn()
        {
            string msg;

            // 전체 접속
            if (!IsAllConnected)
            {
                // MCU 접속 처리
                if(!Mcu.IsConnected)
                {
                    msg = Mcu.Connect(McuInfos.PortName, McuInfos.Borate);
                    if (msg != "Connected!")
                    {
                        MessageBox.Show($"MCU 접속 오류 : {msg}");
                        return;
                    }
                    McuConnColor = Application.Current.Resources["LedGreen"] as SolidColorBrush;
                    Mcu.MonitorStart();
                }

                // DMM 접속 처리
                if (!Dmm.IsConnected)
                {
                    msg = Dmm.Connect(DmmInfos.PortName, DmmInfos.Borate);
                    if (msg != "Connected!")
                    {
                        MessageBox.Show($"DMM 접속 오류 : {msg}");
                        return;
                    }
                    DmmConnColor = Application.Current.Resources["LedGreen"] as SolidColorBrush;
                    Dmm.MonitorStart();
                }

                IsAllConnected = true;
            }
            // 전체 해제
            else if (IsAllConnected)
            {
                if(Mcu.IsConnected)
                {
                    if(Mcu.Disconnect() == false)
                    {
                        MessageBox.Show($"MCU 접속 해제 오류");
                        return;
                    }
                    McuConnColor = Application.Current.Resources["LedGreenOff"] as SolidColorBrush;
                    Mcu.MonitorStop();
                }
                if (Dmm.IsConnected)
                {
                    Dmm.Disconnect();
                    if (Dmm.IsConnected == true)
                    {
                        MessageBox.Show($"DMM 접속 해제 오류");
                        return;
                    }
                    DmmConnColor = Application.Current.Resources["LedGreenOff"] as SolidColorBrush;
                    Dmm.MonitorStop();
                }

                IsAllConnected = false;
            }
        }
    }
}
