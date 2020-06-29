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
using J_Project.Communication.CommModule;
using J_Project.Communication.CommFlags;
using J_Project.Manager;
using System.IO.Ports;
using System.Timers;
using System.Windows.Threading;

namespace CalibrationNewGUI
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    [ImplementPropertyChanged]
    public partial class MainWindow : Window
    {
        public SettingData AllSetData { get; set; } //데이터 갱신을 위한 변수

        //통신 개체 작성
        public SerialComm commRS232DMM = new SerialComm("DMM", 0);//DMM 용
        public SerialComm commRS232MCU = new SerialComm("MCU", 0);//MCU 용
        TestSetting setWindow = new TestSetting(); //통신세팅 창 띄우기 - 강제 세팅 테스트용

        Timer MCUMonitoringTimer = new Timer(); //MCU 모니터링 타이머용
        Timer DMMMonitoringTimer = new Timer(); //DMM 모니터링 타이머용
        //Timer GUIOutPutCheckTimer = new Timer(); //cal, mea 버튼 입력들 감지 타이머용
        DispatcherTimer FlagMonitor = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = SettingData.GetObj();
        }
        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            AllSetData = SettingData.GetObj();
            DMMMonitoringTimer.Interval = 500;    // ms
            MCUMonitoringTimer.Interval = 200;    // ms
            MCUMonitoringTimer.Elapsed += MCUMonitorEvent;
            DMMMonitoringTimer.Elapsed += DMMMonitorEvent;
            MCUMonitoringTimer.Start();
            DMMMonitoringTimer.Start();
            FlagMonitor.Interval = TimeSpan.FromMilliseconds(50);
            FlagMonitor.Tick += OutputEvent;
            FlagMonitor.Start();
            //GUIOutPutCheckTimer.Interval = 50;    // ms
            //GUIOutPutCheckTimer.Elapsed += OutputEvent;
            //GUIOutPutCheckTimer.Start();
        }
        private void MainWindowClosing(object sender, RoutedEventArgs e)
        {
            setWindow.Close();
            Environment.Exit(0);
        }
        private void MonitorBtn_Click(object sender, RoutedEventArgs e)
        {
            PageFrame.Source = new Uri("MonitoringPage.xaml", UriKind.Relative);
        }

        //통신 연결 버튼
        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            string msg;
            int comportSearch;
            byte[] bytestream = new byte[3];

            int DMMdisconnect = 0;
            int MCUdisconnect = 0;

            if (AllSetData.AllConnectFlag == 0)
            {
                comportSearch = settingComport(setWindow);
                if (comportSearch == 1)
                {
                    setWindow.ShowDialog();
                    //통신 연결
                    if (setWindow.portNameMCU != "0")
                    {
                        msg = commRS232MCU.Connect(setWindow.portNameMCU, AllSetData.MCUBorate);
                        //MessageBox.Show("MCU" + msg);
                        if (msg == "Connected!")
                        {
                            AllSetData.MCUConnectFlag = 1;
                            MCUConnectCircle.Fill = Brushes.LimeGreen;
                        }
                    }
                    if (setWindow.portNameDMM != "0")
                    {
                        msg = commRS232DMM.Connect(setWindow.portNameDMM, AllSetData.DMMBorate);
                        //MessageBox.Show("DMM" + msg);
                        if (msg == "Connected!")
                        {
                            AllSetData.DMMConnectFlag = 1;
                            DMMConnectCircle.Fill = Brushes.LimeGreen;
                        }
                    }
                }
            }
            else if (AllSetData.AllConnectFlag == 1) //연결 끊기
            {
                if (AllSetData.MCUConnectFlag == 1)
                {
                    MCUMonitoringTimer.Stop();
                    MCUdisconnect = (int)commRS232MCU.Disconnect();
                }
                if (AllSetData.DMMConnectFlag == 1)
                {
                    DMMMonitoringTimer.Stop();
                    DMMdisconnect = (int)commRS232DMM.Disconnect();
                }
                if (MCUdisconnect == 2)
                {
                    AllSetData.MCUConnectFlag = 0;
                    MessageBox.Show("MCUDisConnect");
                    MCUConnectCircle.Fill = Brushes.Red;
                }
                if (DMMdisconnect == 2)
                {
                    AllSetData.DMMConnectFlag = 0;
                    MessageBox.Show("DMMDisConnect");
                    DMMConnectCircle.Fill = Brushes.Red;
                }
            }
            if (AllSetData.MCUConnectFlag == 1 || AllSetData.MCUConnectFlag == 1)
            {
                AllSetData.AllConnectFlag = 1;
                ConnectTextBlock.Text = "Disconnect";
                ConnectTextBlock.FontSize = 15;
            }
            else
            {
                AllSetData.AllConnectFlag = 0;
                ConnectTextBlock.Text = "Connect";
                ConnectTextBlock.FontSize = 20;
            }
        }

        ///////////////////////////////////////테스트 통신 연결
        private int settingComport(TestSetting settingWindow) //세팅화면 띄우기 전 호출해서 파일 읽기 & 콤보박스 세팅
        {
            settingWindow.PortNameComboMCU.ItemsSource = SerialPort.GetPortNames();
            settingWindow.PortNameComboDMM.ItemsSource = SerialPort.GetPortNames();
            if (settingWindow.PortNameComboMCU.Items.Count > 0)
            {
                //settingWindow.PortNameComboMCU.SelectedIndex = 0;
                //settingWindow.PortNameComboDMM.SelectedIndex = 0;
            }
            else
            {
                string errormsg = "연결된 포트가 없습니다.";
                MessageBox.Show(errormsg);
                return 0; //실패
            }

            return 1;//성공
        }
        //데이터 전송, 수신함수
        private void SendAndReceiveFunc(string sendDatastring) //데이터 전송 후 받는 함수 string 버전
        {
            DMMMonitoringTimer.Stop();
            commRS232DMM.CommSend(sendDatastring);

            for (int i = 0; i < 4000; i++)
            {
                if (commRS232DMM.receiveDataString != null)
                {
                    StringTranslate(commRS232DMM.receiveDataString);
                    //MessageBox.Show("수신 : " + AllSetData.DMMOutputVolt);
                    commRS232DMM.receiveDataString = null;
                    DMMMonitoringTimer.Start();
                    break;
                }
                else
                {
                    if (i == (4000 - 1))
                    {
                        MessageBox.Show("DMM 수신 : 실패");
                        commRS232DMM.receiveDataString = null;
                        DMMMonitoringTimer.Start();
                        break;
                    }
                    //Thread.Sleep(1);
                    Utill.Delay(0.001);
                }
            }
        }
        private void SendAndReceiveFunc(byte[] sendDatabyte) //데이터 전송 후 받는 함수 byte 배열 버전
        {
            int temp = 0;
            MCUMonitoringTimer.Stop();
            commRS232MCU.CommSend(sendDatabyte);

            for (int i = 0; i < 4000; i++)
            {
                if ((commRS232MCU.receiveDataByteETX - 1) >= 0) temp = commRS232MCU.receiveDataByteETX - 1;
                else if ((commRS232MCU.receiveDataByteETX - 1) < 0) temp = 49;
                if (commRS232MCU.receiveDataByte[commRS232MCU.receiveDataByteSTX] == 0x02 && commRS232MCU.receiveDataByte[temp] == 0x03)
                {
                    AsciiTranslate(commRS232MCU.receiveDataByte);
                    //MessageBox.Show("수신 : " + temp);
                    commRS232MCU.receiveDataByte[commRS232MCU.receiveDataByteSTX] = 0;
                    commRS232MCU.receiveDataByte[commRS232MCU.receiveDataByteETX] = 0;
                    MCUMonitoringTimer.Start();
                    break;
                }
                else
                {
                    if (i == (4000 - 1))
                    {
                        MessageBox.Show("MCU 수신 : 실패");
                        //commRS232MCU.receiveDataByte = null;
                        MCUMonitoringTimer.Start();
                        break;
                    }
                    //Thread.Sleep(1);
                    Utill.Delay(0.001);
                }
            }
        }
        //MCU에서 받은 값(채널1,2 모니터링)을 실제 변수에 넣기
        private void AsciiTranslate(byte[] recieveStream)
        {
            //string temp;
            float voltCH1;
            float currCH1;
            float voltCH2;
            float currCH2;

            switch (recieveStream[(commRS232MCU.receiveDataByteSTX + 1) % 50])//Recieve값 구분하기
            {
                case 0x4D://모니터링용 'M'
                    //CH1 전압
                    voltCH1 = (float)((recieveStream[(commRS232MCU.receiveDataByteSTX + 3) % 50] - 0x30) * 10000
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 4) % 50] - 0x30) * 1000
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 5) % 50] - 0x30) * 100
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 6) % 50] - 0x30) * 10
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 7) % 50] - 0x30)) * 0.0001f;

                    currCH1 = (float)((recieveStream[(commRS232MCU.receiveDataByteSTX + 9) % 50] - 0x30) * 100000
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 10) % 50] - 0x30) * 10000
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 11) % 50] - 0x30) * 1000
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 12) % 50] - 0x30) * 100
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 13) % 50] - 0x30) * 10
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 14) % 50] - 0x30)) * 0.001f;

                    voltCH2 = (float)((recieveStream[(commRS232MCU.receiveDataByteSTX + 16) % 50] - 0x30) * 10000
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 17) % 50] - 0x30) * 1000
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 18) % 50] - 0x30) * 100
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 19) % 50] - 0x30) * 10
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 20) % 50] - 0x30)) * 0.0001f;

                    currCH2 = (float)((recieveStream[(commRS232MCU.receiveDataByteSTX + 22) % 50] - 0x30) * 100000
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 23) % 50] - 0x30) * 10000
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 24) % 50] - 0x30) * 1000
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 25) % 50] - 0x30) * 100
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 26) % 50] - 0x30) * 10
                                    + (recieveStream[(commRS232MCU.receiveDataByteSTX + 27) % 50] - 0x30)) * 0.001f;

                    if (recieveStream[(commRS232MCU.receiveDataByteSTX + 2) % 50] == 0x2B) AllSetData.CH1OutputVolt = voltCH1;//'+'
                    else if (recieveStream[(commRS232MCU.receiveDataByteSTX + 2) % 50] == 0x2D) AllSetData.CH1OutputVolt = voltCH1 * -1f;//'-'
                    //CH1 전류
                    if (recieveStream[(commRS232MCU.receiveDataByteSTX + 8) % 50] == 0x2B) AllSetData.CH1OutputCurr = currCH1;//'+'
                    else if (recieveStream[(commRS232MCU.receiveDataByteSTX + 8) % 50] == 0x2D) AllSetData.CH1OutputCurr = currCH1 * -1f;//'-'
                    //CH2 전압
                    if (recieveStream[(commRS232MCU.receiveDataByteSTX + 15) % 50] == 0x2B) AllSetData.CH2OutputVolt = voltCH2;//'+'
                    else if (recieveStream[(commRS232MCU.receiveDataByteSTX + 15) % 50] == 0x2D) AllSetData.CH2OutputVolt = voltCH2 * -1f;//'-'
                    //CH2 전류
                    if (recieveStream[(commRS232MCU.receiveDataByteSTX + 21) % 50] == 0x2B) AllSetData.CH2OutputCurr = currCH2;//'+'
                    else if (recieveStream[(commRS232MCU.receiveDataByteSTX + 21) % 50] == 0x2D) AllSetData.CH2OutputCurr = currCH2 * -1f;//'-'

                    break;

            }
        }
        //DMM 에서 들어오는 문자열 변환
        private void StringTranslate(string stringStream)
        {
            double tempDouble = 0;

            tempDouble = Convert.ToDouble(stringStream);
            AllSetData.DMMOutputVolt = (float)tempDouble * 1000.0f;
        }

        //모니터링용 타이머
        private void MCUMonitorEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            byte[] bytestream = new byte[3];

            if (AllSetData.MCUConnectFlag == 1)
            {
                bytestream[0] = 0x02;//STX
                bytestream[1] = 0x4F;//'O'
                bytestream[2] = 0x03;//ETX
                SendAndReceiveFunc(bytestream);
            }
        }
        private void DMMMonitorEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            string stringStream = "";
            stringStream = "MEASure:VOLTage:DC?";//모니터링 명령어
            if (AllSetData.DMMConnectFlag == 1)
            {
                SendAndReceiveFunc(stringStream);
            }
        }
        //cal, mea 버튼들 이벤특감지
        //private void OutputEvent(Object source, System.Timers.ElapsedEventArgs e)
        private void OutputEvent(object sender, EventArgs e)
        {
            //자동CAL 출력, 자동CAL종료, 수동CAL 출력, 수동CAL 실시, 수동출력정지,  실측시작, 실측정지
            /*
            AllSetData.AutoCalOutStartFlag = 0;//자동CAL 출력
            AllSetData.AutoCalOutEndFlag = 0;//자동CAL종료
            AllSetData.CalOutStartFlag = 0;//수동CAL 출력
            AllSetData.CalOutRealStartFlag = 0;//수동CAL 실시
            AllSetData.CalOutEndFlag = 0;//수동출력정지
            AllSetData.MeaOutStartFlag = 0;//실측시작
            AllSetData.MeaOutEndFlag = 0;//실측정지
            */
            //수동Cal 실행
            if (AllSetData.CalOutStartFlag == 1)
            {
                byte[] bytestream = new byte[17];//출력용 배열
                //출력 전송
                bytestream = OutputVoltCurr(bytestream, AllSetData.ActCalPointArray);
                commRS232MCU.CommSend(bytestream);
                AllSetData.CalOutStartFlag = 0;
            }
            //수동 Cal DMM 전송
            if (AllSetData.CalOutRealStartFlag == 1)
            {
                SendCommand(0x52);//'R'
                AllSetData.CalOutRealStartFlag = 0;
            }
            //자동, 수동 CAL종료, 실측 종료
            if (AllSetData.AutoCalOutEndFlag == 1 || AllSetData.CalOutEndFlag == 1 || AllSetData.MeaOutEndFlag == 1)
            {
                if (AllSetData.AutoCalOutEndFlag == 1) AllSetData.AutoCalOutEndFlag = 0;
                if (AllSetData.CalOutEndFlag == 1) AllSetData.CalOutEndFlag = 0;
                if (AllSetData.MeaOutEndFlag == 1) AllSetData.MeaOutEndFlag = 0;
                byte[] bytestream = new byte[3];

                if (AllSetData.MCUConnectFlag == 1)
                {
                    SendCommand(0x54);//'T'
                }
            }
        }
        //명령에 따른 전송함수
        private void SendCommand(byte sendCmd)
        {
            byte[] bytestream;
            byte[] VoltArray = new byte[5];
            byte[] CurrArray = new byte[7];

            switch (sendCmd)
            {
                case 0x52: //'R'
                    //RI2+0000074
                    //RV200000   
                    if (AllSetData.VoltCurrSelect == 0)//전압
                    {
                        bytestream = new byte[10];
                        bytestream[0] = 0x02;//STX
                        bytestream[1] = sendCmd;
                        bytestream[2] = 0x56;//'V'
                        bytestream[3] = IntToByte(AllSetData.ChannelSelect); //채널선택
                        VoltArray = IntToAsciiByte((int)(AllSetData.DMMOutputVolt*10), VoltArray.Length);
                        for (int i = 0; i < VoltArray.Length; i++)
                        {
                            bytestream[4 + i] = VoltArray[VoltArray.Length - i - 1];
                        }
                        bytestream[9]= 0x03;//ETX
                    }
                    else //전류
                    {
                        bytestream = new byte[13];
                        bytestream[0] = 0x02;//STX
                        bytestream[1] = sendCmd;
                        bytestream[2] = 0x49;//'I'
                        bytestream[3] = IntToByte(AllSetData.ChannelSelect); //채널선택
                        CurrArray = IntToAsciiByte((int)(AllSetData.DMMOutputVolt * 10), CurrArray.Length);
                    }

                    commRS232MCU.CommSend(bytestream);
                    break;
                case 0x54: //'T'
                    bytestream = new byte[3];

                    bytestream[0] = 0x02;//STX
                    bytestream[1] = sendCmd;
                    bytestream[2] = 0x03;//ETX
                    commRS232MCU.CommSend(bytestream);
                    break;
            }
        }
        //출력 입력 함수-1개의 배열
        private byte[] OutputVoltCurr(byte[] bytestream, int[] PointArray)
        {
            byte[] VoltArray = new byte[5];
            byte[] CurrArray = new byte[6];

            bytestream[0] = 0x02;//STX
            bytestream[1] = 0x43;//'C'
            if (AllSetData.VoltCurrSelect == 0) bytestream[2] = 0x56;//'V'
            else if (AllSetData.VoltCurrSelect == 1) bytestream[2] = 0x49;//'I'
            bytestream[3] = IntToByte(AllSetData.ChannelSelect); //채널선택

            //IntToAscii(PointArray[0], bytestream, 0);//전압
            //IntToAscii(PointArray[1], bytestream, 1);//전류

            VoltArray = IntToAsciiByte(PointArray[0], VoltArray.Length);
            CurrArray = IntToAsciiByte(PointArray[1], CurrArray.Length);
            //전압
            for (int i = 0; i < VoltArray.Length; i++)
            {
                bytestream[4 + i] = VoltArray[VoltArray.Length - i - 1];
            }
            //전류
            if(PointArray[1] < 0) bytestream[9] = 0x2D; //-
            else bytestream[9] = 0x2B;//+
            for (int i = 0; i < CurrArray.Length; i++)
            {
                bytestream[10 + i] = VoltArray[CurrArray.Length - i - 1];
            }

            bytestream[16] = 0x03;//ETX
            return bytestream;
        }
        private byte[] IntToAsciiByte(int c, int c_length)
        {
            byte[] tempByte = new byte[c_length];
            int temp = 0;

            while (c > 0)
            {
                tempByte[temp] = IntToByte(c % 10);
                c = c / 10;
                temp++;

                if (temp >= c_length) break;
            }

            return tempByte;
        }

        //입력된 숫자를 전압, 전류 자리수에 맞게 채워넣기 ->개선함수 있음
        private void IntToAscii(int c, byte[] bytestream, int voltcurr)
        {
            if (voltcurr == 0) //전압
            {
                if (c >= 10000) bytestream[4] = IntToByte((c % 100000) / 10000);
                else bytestream[4] = 0x30;

                if (c >= 1000) bytestream[5] = IntToByte((c % 10000) / 1000);
                else bytestream[5] = 0x30;

                if (c >= 100) bytestream[6] = IntToByte((c % 1000) / 100);
                else bytestream[6] = 0x30;

                if (c >= 10) bytestream[7] = IntToByte((c % 100) / 10);
                else bytestream[7] = 0x30;

                bytestream[8] = IntToByte(c % 10);
            }
            else //전류
            {
                if (c < 0)
                {
                    c *= -1;
                    bytestream[9] = 0x2D; //-
                }
                else
                {
                    bytestream[9] = 0x2B;//+
                }
                if (c >= 100000) bytestream[10] = IntToByte(c / 100000);
                else bytestream[10] = 0x30;

                if (c >= 10000) bytestream[11] = IntToByte((c % 100000) / 10000);
                else bytestream[11] = 0x30;

                if (c >= 1000) bytestream[12] = IntToByte((c % 10000) / 1000);
                else bytestream[12] = 0x30;

                if (c >= 100) bytestream[13] = IntToByte((c % 1000) / 100);
                else bytestream[13] = 0x30;

                if (c >= 10) bytestream[14] = IntToByte((c % 100) / 10);
                else bytestream[14] = 0x30;

                bytestream[15] = IntToByte(c % 10);
            }
        }

        //1의 자리를 아스키(헥사값)로 변환할때
        private byte IntToByte(int num)
        {
            byte temp = 0;
            switch (num)
            {
                case 0:
                    temp = 0x30;
                    break;
                case 1:
                    temp = 0x31;
                    break;
                case 2:
                    temp = 0x32;
                    break;
                case 3:
                    temp = 0x33;
                    break;
                case 4:
                    temp = 0x34;
                    break;
                case 5:
                    temp = 0x35;
                    break;
                case 6:
                    temp = 0x36;
                    break;
                case 7:
                    temp = 0x37;
                    break;
                case 8:
                    temp = 0x38;
                    break;
                case 9:
                    temp = 0x39;
                    break;
            }
            return temp;
        }

    }
}
