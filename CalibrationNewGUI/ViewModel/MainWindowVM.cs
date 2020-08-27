using CalibrationNewGUI.Equipment;
using CalibrationNewGUI.Message;
using CalibrationNewGUI.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using PropertyChanged;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CalibrationNewGUI.ViewModel
{
    [ImplementPropertyChanged]
    public class MainWindowVM : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Dmm));

        public bool CalRadio { get; set; }
        public bool ScanRadio { get; set; }
        public bool SettingRadio { get; set; }

        public SolidColorBrush McuConnColor { get; set; }
        public SolidColorBrush DmmConnColor { get; set; }
        public bool IsAllConnected { get; set; } = false;

        public string GuiVersion { get; set; }
        public string ProgramTitle { get; set; }

        public McuInfo McuInfos { get; set; }
        public DmmInfo DmmInfos { get; set; }

        public Mcu Mcu { get; set; }
        public Dmm Dmm { get; set; }

        public RelayCommand McuConnectClick { get; set; }
        public RelayCommand DmmConnectClick { get; set; }
        public RelayCommand<TextChangedEventArgs> SerialInputChanged { get; set; }
        public RelayCommand SerialEnterPush { get; set; }
        public RelayCommand LanguageClick { get; set; }

        public MainWindowVM()
        {
            GuiVersion = "1.0.0";

            McuConnColor = Application.Current.Resources["LedGreenOff"] as SolidColorBrush;
            DmmConnColor = Application.Current.Resources["LedGreenOff"] as SolidColorBrush;

            McuInfos = McuInfo.GetObj();
            DmmInfos = DmmInfo.GetObj();

            Mcu = Mcu.GetObj();
            Dmm = Dmm.GetObj();

            McuConnectClick = new RelayCommand(McuConnect);
            DmmConnectClick = new RelayCommand(DmmConnect);
            SerialInputChanged = new RelayCommand<TextChangedEventArgs>(SerialInput);
            SerialEnterPush = new RelayCommand(SerialEnter);
            LanguageClick = new RelayCommand(LanguageMove);

            Messenger.Default.Register<MainMoveMessage>(this, MianMode);


            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);
            ProgramTitle = $"KJ Calibration GUI       [ Last Build : {buildDate.ToString("yyyy-MM-dd")}]";
        }

        //통신 연결 버튼
        private void McuConnect()
        {
            string msg;

            // MCU 접속 처리
            if (!Mcu.IsConnected)
            {
                msg = Mcu.Connect(McuInfos.PortName, McuInfos.Borate);
                if (msg != "Connected!")
                {
                    MessageBox.Show(string.Format(App.GetString("McuConnErrMsg"), msg));
                    return;
                }
                McuConnColor = Application.Current.Resources["LedGreen"] as SolidColorBrush;
                Mcu.MonitorStart();
            }
            else
            {
                Mcu.Disconnect();
                if (Mcu.IsConnected == true)
                {
                    MessageBox.Show(App.GetString("McuDisconnErrMsg"));
                    return;
                }
                McuConnColor = Application.Current.Resources["LedGreenOff"] as SolidColorBrush;
                Mcu.MonitorStop();
            }
        }
        //통신 연결 버튼
        private void DmmConnect()
        {
            string msg;

            // DMM 접속 처리
            if (!Dmm.IsConnected)
            {
                msg = Dmm.Connect(DmmInfos.PortName, DmmInfos.Borate);
                if (msg != "Connected!")
                {
                    MessageBox.Show(string.Format(App.GetString("DmmConnErrMsg"), msg));
                    return;
                }

                DmmConnColor = Application.Current.Resources["LedGreen"] as SolidColorBrush;
                Dmm.Setting();
                Dmm.MonitorStart();
            }
            else
            {
                Dmm.Disconnect();
                if (Dmm.IsConnected == true)
                {
                    MessageBox.Show(App.GetString("DmmDisconnErrMsg"));
                    return;
                }
                DmmConnColor = Application.Current.Resources["LedGreenOff"] as SolidColorBrush;
                Dmm.MonitorStop();
            }
        }

        private void SerialInput(TextChangedEventArgs e)
        {
            McuInfos.McuSerialNum = ((TextBox)e.Source).Text;
        }

        private void SerialEnter()
        {
            if (string.IsNullOrEmpty(McuInfos.McuSerialNum))
                return;

            SerialMessage Message = new SerialMessage
            {
                SerialNum = McuInfos.McuSerialNum
            };
            Messenger.Default.Send(Message);
        }

        private void LanguageMove()
        {
            MainMoveMessage Message = new MainMoveMessage
            {
                LangMove = true
            };
            Messenger.Default.Send(Message);
        }

        private void MianMode(MainMoveMessage obj)
        {
            SettingRadio = true;
            SettingMoveMessage msg = new SettingMoveMessage();

            if (obj.LangMove)
                msg.LangMove = true;
            else if (obj.ShuntMode)
                msg.ShuntMode = true;

            Messenger.Default.Send(msg);
        }
    }
}
