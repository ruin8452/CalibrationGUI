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
        DispatcherTimer SeqMonitor = new DispatcherTimer(); //시퀀스용
        DispatcherTimer RevMonitor = new DispatcherTimer(); //모니터링 리시브용
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
            SeqMonitor.Interval = TimeSpan.FromMilliseconds(20);
            SeqMonitor.Tick += OutSeqEvent;
            RevMonitor.Interval = TimeSpan.FromMilliseconds(10);
            RevMonitor.Tick += ReadRevEvent;
            RevMonitor.Start();
        }
        private void MainWindowInit(object sender, RoutedEventArgs e)
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
            SeqMonitor.Interval = TimeSpan.FromMilliseconds(20);
            SeqMonitor.Tick += OutSeqEvent;
            RevMonitor.Interval = TimeSpan.FromMilliseconds(10);
            RevMonitor.Tick += ReadRevEvent;
            RevMonitor.Start();
        }
        private void MainWindowClosing(object sender, RoutedEventArgs e)
        {
            setWindow.Close();
            Environment.Exit(0);
        }
        //모니터링 페이지 호출
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
                //comportSearch = settingComport(setWindow);
                comportSearch = settingComport();
                if (comportSearch == 1)
                {
                    //setWindow.ShowDialog();
                    //통신 연결
                    if (AllSetData.MCUPortName != "0")
                    {
                        msg = commRS232MCU.Connect(AllSetData.MCUPortName, AllSetData.MCUBorate);
                        //MessageBox.Show("MCU" + msg);
                        if (msg == "Connected!")
                        {
                            AllSetData.MCUConnectFlag = 1;
                            MCUConnectCircle.Fill = Brushes.LimeGreen;
                        }
                        else
                        {
                            MessageBox.Show(msg);
                            MCUConnectCircle.Fill = Brushes.Red;
                        }
                    }
                    if (AllSetData.DMMPortName != "0")
                    {
                        msg = commRS232DMM.Connect(AllSetData.DMMPortName, AllSetData.DMMBorate);
                        //MessageBox.Show("DMM" + msg);
                        if (msg == "Connected!")
                        {
                            AllSetData.DMMConnectFlag = 1;
                            DMMConnectCircle.Fill = Brushes.LimeGreen;
                        }
                        else
                        {
                            MessageBox.Show(msg);
                            DMMConnectCircle.Fill = Brushes.Red;
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
                    //MessageBox.Show("MCUDisConnect");
                    MCUConnectCircle.Fill = Brushes.Gray;
                }
                if (DMMdisconnect == 2)
                {
                    AllSetData.DMMConnectFlag = 0;
                    //MessageBox.Show("DMMDisConnect");
                    DMMConnectCircle.Fill = Brushes.Gray;
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
            if (SerialPort.GetPortNames().Length > 0)
            {
                settingWindow.PortNameComboMCU.ItemsSource = SerialPort.GetPortNames();
                settingWindow.PortNameComboDMM.ItemsSource = SerialPort.GetPortNames();
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
        private int settingComport() //세팅화면 띄우기 전 호출해서 파일 읽기 & 콤보박스 세팅
        {
            if (AllSetData.MCUPortName == "" || AllSetData.MCUPortName == "0")
            {
                string errormsg = "MCU 포트 확인";
                MessageBox.Show(errormsg);
                return 0; //실패
            }
            else if (AllSetData.DMMPortName == "" || AllSetData.DMMPortName == "0")
            {
                string errormsg = "DMM 포트 확인";
                MessageBox.Show(errormsg);
                return 0; //실패
            }

            return 1;//성공
        }
        //데이터 전송, 수신함수
        private void SendAndReceiveFunc(string sendDatastring) //데이터 전송 후 받는 함수 string 버전
        {
            //DMMMonitoringTimer.Stop();
            commRS232DMM.CommSend(sendDatastring);

            //for (int i = 0; i < 4000; i++)
            //{
            //    if (commRS232DMM.receiveDataString != null)
            //    {
            //        StringTranslate(commRS232DMM.receiveDataString);
            //        //MessageBox.Show("수신 : " + AllSetData.DMMOutputVolt);
            //        commRS232DMM.receiveDataString = null;
            //        DMMMonitoringTimer.Start();
            //        break;
            //    }
            //    else
            //    {
            //        if (i == (4000 - 1))
            //        {
            //            MessageBox.Show("DMM 수신 : 실패");
            //            commRS232DMM.receiveDataString = null;
            //            DMMMonitoringTimer.Start();
            //            break;
            //        }
            //        //Thread.Sleep(1);
            //        Utill.Delay(0.001);
            //    }
            //}
        }
        private void SendAndReceiveFunc(byte[] sendDatabyte) //데이터 전송 후 받는 함수 byte 배열 버전
        {
            int temp = 0;
            //MCUMonitoringTimer.Stop();
            commRS232MCU.CommSend(sendDatabyte);

            //for (int i = 0; i < 4000; i++)
            //{
            //    if ((commRS232MCU.receiveDataByteETX - 1) >= 0) temp = commRS232MCU.receiveDataByteETX - 1;
            //    else if ((commRS232MCU.receiveDataByteETX - 1) < 0) temp = 49;
            //    if (commRS232MCU.receiveDataByte[commRS232MCU.receiveDataByteSTX] == 0x02 && commRS232MCU.receiveDataByte[temp] == 0x03)
            //    {
            //        AsciiTranslate(commRS232MCU.receiveDataByte);
            //        //MessageBox.Show("수신 : " + temp);
            //        commRS232MCU.receiveDataByte[commRS232MCU.receiveDataByteSTX] = 0;
            //        commRS232MCU.receiveDataByte[commRS232MCU.receiveDataByteETX] = 0;
            //        //MCUMonitoringTimer.Start();
            //        break;
            //    }
            //    else
            //    {
            //        if (i == (4000 - 1))
            //        {
            //            MessageBox.Show("MCU 수신 : 실패");
            //            //commRS232MCU.receiveDataByte = null;
            //            //MCUMonitoringTimer.Start();
            //            break;
            //        }
            //        //Thread.Sleep(1);
            //        Utill.Delay(0.001);
            //    }
            //}
        }
        //데이터 리시브용 타이머 함수
        private void ReadRevEvent(object sender, EventArgs e)
        {
            int temp = 0;
            //MCU용 리시브 함수
            if (commRS232MCU.IsReceiveByte == true)
            {
                if ((commRS232MCU.receiveDataByteETX - 1) >= 0) temp = commRS232MCU.receiveDataByteETX - 1;//마지막 인덱스 판단
                else if ((commRS232MCU.receiveDataByteETX - 1) < 0) temp = 49;
                if (commRS232MCU.receiveDataByte[commRS232MCU.receiveDataByteSTX] == 0x02 && commRS232MCU.receiveDataByte[temp] == 0x03)
                {
                    AsciiTranslate(commRS232MCU.receiveDataByte);
                    //MessageBox.Show("수신 : " + temp);
                    commRS232MCU.receiveDataByte[commRS232MCU.receiveDataByteSTX] = 0;
                    commRS232MCU.receiveDataByte[commRS232MCU.receiveDataByteETX] = 0;
                    //MCUMonitoringTimer.Start();
                    if (AllSetData.LogMonitoringViewFlag == 0 && AllSetData.LogViewStartFlag == 1)
                    {
                        if (AllSetData.LogMonitoringFlagMCU == 0)
                        {
                            AllSetData.LogMonitoringFlagMCU = 1;
                            //SendLogText(temp, 1);//로그 추가
                        }
                    }
                }
                commRS232MCU.IsReceiveByte = false;
            }
            //DMM용 리시브 함수
            if(commRS232DMM.IsReceiveString == true)
            {
                if (commRS232DMM.receiveDataString != null)
                {
                    StringTranslate(commRS232DMM.receiveDataString);
                    //MessageBox.Show("수신 : " + AllSetData.DMMOutputVolt);
                    commRS232DMM.receiveDataString = null;
                    //DMMMonitoringTimer.Start();
                    //break;
                    if (AllSetData.LogMonitoringViewFlag == 0 && AllSetData.LogViewStartFlag == 1)
                    {
                        if (AllSetData.LogMonitoringFlagDMM == 0)
                        {
                            AllSetData.LogMonitoringFlagDMM = 1;
                            //SendLogText(AllSetData.DMMOutputVolt.ToString(), 1);//로그 추가
                            //AllSetData.LogMonitoringFlag = 0;
                        }
                    }
                }
                commRS232DMM.IsReceiveString = false;
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
            if(AllSetData.VoltCurrSelect == 0) AllSetData.DMMOutputVolt = (float)tempDouble * 1000.0f;//DMM 100mV = 전압 100mV
            else if(AllSetData.VoltCurrSelect == 1) AllSetData.DMMOutputVolt = (float)tempDouble * 1000000.0f;//DMM 100mV = 전류 100A
        }

        //MCU모니터링용 타이머
        private void MCUMonitorEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            byte[] bytestream = new byte[3];

            if (AllSetData.MCUConnectFlag == 1)
            {
                SendCommand(0x4F);//'O'
                //if (AllSetData.LogMonitoringViewFlag == 0 && AllSetData.LogViewStartFlag == 1)
                //{
                //    if (AllSetData.LogMonitoringFlagMCU == 0)
                //    {
                //        AllSetData.LogMonitoringFlagMCU = 1;
                //        //SendLogText(temp, 1);//로그 추가
                //    }
                //}
            }
        }
        //DMM모니터링용 타이머
        private void DMMMonitorEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            string stringStream = "";
            stringStream = "MEASure:VOLTage:DC?";//모니터링 명령어
            if (AllSetData.DMMConnectFlag == 1)
            {
                //SendAndReceiveFunc(stringStream);
                commRS232DMM.CommSend(stringStream); //차후 3가지 DMM 버전을 만들때 sendcmd함수로 만들것
                //if (AllSetData.LogMonitoringViewFlag == 0 && AllSetData.LogViewStartFlag == 1)
                //{
                //    if (AllSetData.LogMonitoringFlagDMM == 0)
                //    {
                //        AllSetData.LogMonitoringFlagDMM = 1;
                //        //SendLogText(AllSetData.DMMOutputVolt.ToString(), 1);//로그 추가
                //        //AllSetData.LogMonitoringFlag = 0;
                //    }
                //}
            }
        }
        //cal, mea 버튼들 이벤트감지
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
                SendCommand(0x43);//'C'
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
                if (AllSetData.AutoCalOutEndFlag == 1) //자동 cal에서 강제 정지 버튼
                {
                    AllSetData.AutoCalOutEndFlag = 0;
                    if(AllSetData.CalSeqStartFlag == 1) AllSetData.CalSeqNum = 4; //시퀀스 강제 넘기기
                }
                if (AllSetData.CalOutEndFlag == 1) AllSetData.CalOutEndFlag = 0;
                if (AllSetData.MeaOutEndFlag == 1)
                {
                    AllSetData.MeaOutEndFlag = 0;
                    if (AllSetData.MeaSeqStartFlag == 1) AllSetData.CalSeqNum = 4; //시퀀스 강제 넘기기
                }
                //명령전송
                if (AllSetData.MCUConnectFlag == 1) SendCommand(0x54);//'T'
            }
            //로그 출력 코드
            if (AllSetData.LogMonitoringFlagDMM == 1 || AllSetData.LogMonitoringFlagMCU == 1)
            {
                if (AllSetData.LogMonitoringFlagDMM == 1)
                {
                    AllSetData.LogMonitoringFlagDMM = 0;
                    SendLogText(AllSetData.DMMOutputVolt.ToString(), 1);//로그 추가
                }
                if (AllSetData.LogMonitoringFlagMCU == 1)
                {
                    AllSetData.LogMonitoringFlagMCU = 0;
                    string temp = "";
                    if (AllSetData.CH1OutputVolt > 0) temp += "+";
                    temp += AllSetData.CH1OutputVolt.ToString();
                    if (AllSetData.CH1OutputCurr > 0) temp += "+";
                    temp += AllSetData.CH1OutputCurr.ToString();
                    if (AllSetData.CH2OutputVolt > 0) temp += "+";
                    temp += AllSetData.CH2OutputVolt.ToString();
                    if (AllSetData.CH2OutputCurr > 0) temp += "+";
                    temp += AllSetData.CH2OutputCurr.ToString();
                    SendLogText(temp, 1);//로그 추가
                }
            }
            if (AllSetData.MeaOutStartFlag == 1 || AllSetData.AutoCalOutStartFlag == 1)
            {
                if (AllSetData.AutoCalOutStartFlag == 1)
                {
                    AllSetData.CalSeqStartFlag = 1;
                    AllSetData.AutoCalOutStartFlag = 0;
                }
                else if (AllSetData.MeaOutStartFlag == 1)
                {
                    AllSetData.MeaSeqStartFlag = 1;
                    AllSetData.MeaOutStartFlag = 0;
                }
                SeqMonitor.Start();
            }
        }
        //명령에 따른 전송함수
        private void SendCommand(byte sendCmd)
        {
            byte[] bytestream;
            byte[] VoltArray = new byte[5];
            byte[] CurrArray = new byte[6];
#if (true)
            int pos = 0;

            bytestream = new byte[50];      // 임시로 최대 50개로 설정함.
            bytestream[pos++] = 0x02;//STX
            bytestream[pos++] = sendCmd;
            switch (sendCmd)
            {
                case 0x4F://'O' 모니터링
                    break;
                case 0x43://'C'
                    //bytestream = new byte[17];//출력용 배열
                    //출력 전송
                    //bytestream = OutputVoltCurr(bytestream, AllSetData.ActCalPointArray);
                    //commRS232MCU.CommSend(bytestream);

                    if (AllSetData.VoltCurrSelect == 0) bytestream[pos++] = 0x56;//'V'
                    else if (AllSetData.VoltCurrSelect == 1) bytestream[pos++] = 0x49;//'I'

                    bytestream[pos++] = IntToByte(AllSetData.ChannelSelect); //채널선택

                    VoltArray = Int2AsciiByte(AllSetData.ActCalPointArray[0], VoltArray.Length);
                    CurrArray = Int2AsciiByte(AllSetData.ActCalPointArray[1], CurrArray.Length);
                    //전압
                    for (int i = 0; i < VoltArray.Length; i++)
                    {
                        bytestream[pos + i] = VoltArray[i];
                    }
                    pos = pos + VoltArray.Length;

                    //전류
                    if (AllSetData.ActCalPointArray[1] < 0) bytestream[pos++] = 0x2D; //-
                    else bytestream[pos++] = 0x2B;//+

                    for (int i = 0; i < CurrArray.Length; i++)
                    {
                        bytestream[pos + i] = CurrArray[i];
                    }
                    pos = pos + CurrArray.Length;

                    if (AllSetData.LogViewStartFlag == 1) SendLogText(bytestream, 0);//로그 추가
                    break;
                case 0x52: //'R'
                    //RI2+0000074
                    //RV200000   
                    if (AllSetData.VoltCurrSelect == 0)//전압
                    {
                        VoltArray = Int2AsciiByte((int)(AllSetData.DMMOutputVolt * 10), VoltArray.Length);

                        bytestream[pos++] = 0x56;//'V'
                        bytestream[pos++] = IntToByte(AllSetData.ChannelSelect); //채널선택
                        Buffer.BlockCopy(VoltArray, 0, bytestream, pos, VoltArray.Length);
                        pos += VoltArray.Length;
                    }
                    else //전류
                    {
                        CurrArray = new byte[7];
                        CurrArray = Int2AsciiByte((int)(AllSetData.DMMOutputVolt * 10), CurrArray.Length);
                        
                        bytestream[pos++] = 0x49;//'I'
                        bytestream[pos++] = IntToByte(AllSetData.ChannelSelect); //채널선택
                        //전류 부호
                        if (AllSetData.ActCalPointArray[1] < 0) bytestream[pos++] = 0x2D; //-
                        else bytestream[pos++] = 0x2B;//+

                        Buffer.BlockCopy(CurrArray, 0, bytestream, pos, CurrArray.Length);
                        pos += CurrArray.Length;
                    }
                    if (AllSetData.LogViewStartFlag == 1) SendLogText(bytestream, 0);//로그 추가
                    break;
                case 0x54: //'T'
                    if (AllSetData.LogViewStartFlag == 1) SendLogText(bytestream, 0);//로그 추가
                    break;
            }
            bytestream[pos++] = 0x03;//ETX

            Array.Resize(ref bytestream, pos);
            commRS232MCU.CommSend(bytestream);

#else
            switch (sendCmd)
            {
                case 0x43://'C'
                    bytestream = new byte[17];//출력용 배열
                    //출력 전송
                    bytestream = OutputVoltCurr(bytestream, AllSetData.ActCalPointArray);
                    commRS232MCU.CommSend(bytestream);
                    if (AllSetData.LogViewStartFlag == 1) SendLogText(bytestream, 0);//로그 추가
                    break;
                case 0x52: //'R'
                    //RI2+0000074 전류
                    //RV200000    전압
                    if (AllSetData.VoltCurrSelect == 0)//전압
                    {
                        bytestream = new byte[10];
                        bytestream[0] = 0x02;//STX
                        bytestream[1] = sendCmd;//'R'
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
                        bytestream[1] = sendCmd;//'R'
                        bytestream[2] = 0x49;//'I'
                        bytestream[3] = IntToByte(AllSetData.ChannelSelect); //채널선택
                        if (AllSetData.DMMOutputVolt < 0) bytestream[4] = 0x2D; //-
                        else bytestream[4] = 0x2B;//+
                        CurrArray = IntToAsciiByte((int)(AllSetData.DMMOutputVolt * 10), CurrArray.Length);
                        for (int i = 0; i < CurrArray.Length; i++)
                        {
                            bytestream[5 + i] = CurrArray[CurrArray.Length - i - 1];
                        }
                        bytestream[12] = 0x03;//ETX
                    }

                    commRS232MCU.CommSend(bytestream);
                    if (AllSetData.LogViewStartFlag == 1) SendLogText(bytestream, 0);//로그 추가
                    break;
                case 0x54: //'T'
                    bytestream = new byte[3];

                    bytestream[0] = 0x02;//STX
                    bytestream[1] = sendCmd;
                    bytestream[2] = 0x03;//ETX
                    commRS232MCU.CommSend(bytestream);
                    if (AllSetData.LogViewStartFlag == 1) SendLogText(bytestream, 0);//로그 추가
                    break;
            }
#endif
        }
#if(false)
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

            VoltArray = Int2AsciiByte(PointArray[0], VoltArray.Length);
            CurrArray = Int2AsciiByte(PointArray[1], CurrArray.Length);
            //전압
            for (int i = 0; i < VoltArray.Length; i++)
            {
                bytestream[4 + i] = VoltArray[i];
            }
            //전류
            if(PointArray[1] < 0) bytestream[9] = 0x2D; //-
            else bytestream[9] = 0x2B;//+
            for (int i = 0; i < CurrArray.Length; i++)
            {
                bytestream[10 + i] = CurrArray[i];
            }

            bytestream[16] = 0x03;//ETX
            return bytestream;
        }
#endif
#if (true)
        /// <summary>
        /// Int 를 지정된 자리수의 Ascii의 Byte의 역배열로 처리함.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="c_length"></param>
        /// <returns></returns>
        private byte[] IntToAsciiByte(int c, int c_length)
        {
            byte[] tempByte = new byte[c_length];
            
            if (c < 0) c = c * -1;//음수일경우 양수로 계산
            for (int i = 0; i < tempByte.Length; i++)
            {
                tempByte[i] = (byte)(0x30 + (c % 10));
                c = (int)(c * 0.1);
            }

            return tempByte;
        }
        /// <summary>
        /// Int 를 지정된 자리수의 Ascii Byte 배열로 처리함(swkim)
        /// </summary>
        /// <param name="c"></param>
        /// <param name="c_length"></param>
        /// <returns></returns>
        private byte[] Int2AsciiByte(int c, int c_length)
        {
            byte[] tempByte = new byte[c_length];

            if (c < 0) c = c * -1;//음수일경우 양수로 계산
            for (int pos = c_length - 1; pos >= 0; pos--)
            {
                tempByte[pos] = (byte)(0x30 + (c % 10));
                c = (int)(c * 0.1);
            }

            return tempByte;
        }

        private byte IntToByte(int num)
        {
            return (byte)(num + 0x30);
        }


#else //<D>20.swkim.NOW 불필요 함수
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
#endif
        private void SendLogText(byte[] bytestream, int direction)
        {
            if (LogTextBox.LineCount > 50) LogTextBox.Text = string.Empty; //로그가 50줄 이상 쌓이면 초기화
            switch (direction)
            {
                case 0://send
                    LogTextBox.Text += "Send : ";
                    break;
                case 1://recieve
                    LogTextBox.Text += "Recieve : ";
                    break;
            }
            
            for (int i = 0; i < bytestream.Length; i++) LogTextBox.Text += (Convert.ToChar(bytestream[i]).ToString());
            LogTextBox.Text += ("\n");
            LogTextScroll.ScrollToEnd();
        }
        private void SendLogText(string tempString, int direction)
        {
            if (LogTextBox.LineCount > 50) LogTextBox.Text = string.Empty; //로그가 50줄 이상 쌓이면 초기화
            switch (direction)
            {
                case 0://send
                    LogTextBox.Text += "Send : ";
                    break;
                case 1://recieve
                    LogTextBox.Text += "Recieve : ";
                    break;
            }
            
            LogTextBox.Text += tempString;
            LogTextBox.Text += ("\n");
            LogTextScroll.ScrollToEnd();
        }
        //로그창 출력
        private void LogCheckBtn_Checked(object sender, RoutedEventArgs e)
        {
            PropertiesPanel.Visibility = Visibility.Collapsed;
            LogCheckPanel.Visibility = Visibility.Visible;
            PropertiesBtn.IsChecked = false;
        }
        private void LogCheckBtn_UnChecked(object sender, RoutedEventArgs e)
        {
            LogCheckPanel.Visibility = Visibility.Collapsed;
        }
        //properties창 출력
        private void PropertiesBtn_Checked(object sender, RoutedEventArgs e)
        {
            LogCheckPanel.Visibility = Visibility.Collapsed;
            PropertiesPanel.Visibility = Visibility.Visible;
            LogCheckBtn.IsChecked = false;
        }
        private void PropertiesBtn_UnChecked(object sender, RoutedEventArgs e)
        {
            PropertiesPanel.Visibility = Visibility.Collapsed;
        }
        //로그 시작 체크
        private void LogViewCheck(object sender, RoutedEventArgs e)
        {
            AllSetData.LogViewStartFlag = 1;
        }
        private void LogViewUnCheck(object sender, RoutedEventArgs e)
        {
            AllSetData.LogViewStartFlag = 0;
        }
        //로그 모니터링 제외 체크
        private void LogMonitoringCheck(object sender, RoutedEventArgs e)
        {
            AllSetData.LogMonitoringViewFlag = 1;
        }
        private void LogMonitoringUnCheck(object sender, RoutedEventArgs e)
        {
            AllSetData.LogMonitoringViewFlag = 0;
        }
        //로그창 클리어
        private void LogViewClear(object sender, RoutedEventArgs e)
        {
            LogTextBox.Text = string.Empty;
        }

        //Cal, 실측 시퀀스 함수
        private void OutSeqEvent(object sender, EventArgs e)
        {
            int RowCnt = 0;
            if (AllSetData.CalSeqStartFlag == 1)
            {
                if (AllSetData.VoltCurrSelect == 0) RowCnt = AllSetData.VoltageCalTable.Rows.Count;//전압 데이터 개수
                else if (AllSetData.VoltCurrSelect == 1) RowCnt = AllSetData.CurrentCalTable.Rows.Count;//전류 데이터 개수
                if (AllSetData.DelayStart == 1)
                {
                    AllSetData.DelayCnt = AllSetData.DelayCnt + 20;//인터벌만큼 더해서 카운트 비교
                    if (AllSetData.DelayCnt > AllSetData.CalErrDelayTime)
                    {
                        AllSetData.DelayStart = 0;
                        AllSetData.DelayCnt = 0;
                    }
                }
                switch (AllSetData.CalSeqNum)//(0: 대기, 1: Cal 시작, 2: DMM 전송, 3: 출력 종료)
                {
                    case 0:
                        AllSetData.CalSeqNum = 1;
                        AllSetData.CalRowCntNum = 0;
                        break;
                    case 1:
                        //if (AllSetData.VoltCurrSelect == 0)//전압, 전류 출력
                        //{
                            if (AllSetData.DelayStart == 0)
                            {
                                AllSetData.ActCalPointArray = new int[2];
                                AllSetData.ActCalPointArray[0] = AllSetData.CalPointArray[AllSetData.CalRowCntNum, 0];//전압
                                AllSetData.ActCalPointArray[1] = AllSetData.CalPointArray[AllSetData.CalRowCntNum, 1];//전류
                                AllSetData.CalOutStartFlag = 1;//수동출력으로 전송
                                AllSetData.DelayStart = 1;
                                AllSetData.CalSeqNum = 2;
                            }
                        //}
                        break;
                    case 2:
                        //그리드에 출력하기
                        if (AllSetData.DelayStart == 0)
                        {
                            int DMMCheck = 0;//에러 레인지 범위 안에 들어오는지 판단
                            if (AllSetData.VoltCurrSelect == 0)//전압 출력
                            {
                                if (AllSetData.ChannelSelect == 1) //채널선택
                                {   //단위를 mV, mA 단위로 할것
                                    AllSetData.VoltageCalTable.Rows[AllSetData.CalRowCntNum][3] = ((int)(AllSetData.CH1OutputVolt * 1000)).ToString();
                                    AllSetData.VoltageCalTable.Rows[AllSetData.CalRowCntNum][4] = ((int)(AllSetData.CH1OutputCurr * 1000)).ToString();
                                    AllSetData.VoltageCalTable.Rows[AllSetData.CalRowCntNum][5] = ((AllSetData.DMMOutputVolt)).ToString();
                                }
                                else if (AllSetData.ChannelSelect == 2) 
                                {
                                    AllSetData.VoltageCalTable.Rows[AllSetData.CalRowCntNum][3] = ((int)(AllSetData.CH2OutputVolt * 1000)).ToString();
                                    AllSetData.VoltageCalTable.Rows[AllSetData.CalRowCntNum][4] = ((int)(AllSetData.CH2OutputCurr * 1000)).ToString();
                                    AllSetData.VoltageCalTable.Rows[AllSetData.CalRowCntNum][5] = ((AllSetData.DMMOutputVolt)).ToString();
                                }
                                if (Math.Abs(Convert.ToDouble(AllSetData.VoltageCalTable.Rows[AllSetData.CalRowCntNum][1]) - Convert.ToDouble(AllSetData.VoltageCalTable.Rows[AllSetData.CalRowCntNum][5])) > AllSetData.CalErrRangeVolt)//에러 범위 판단
                                {
                                    DMMCheck = 1;//에러 범위를 넘으면 Cal 실시
                                }
                            }
                            else if (AllSetData.VoltCurrSelect == 1)//전류 출력
                            {
                                if (AllSetData.ChannelSelect == 1) //채널선택
                                {
                                    AllSetData.CurrentCalTable.Rows[AllSetData.CalRowCntNum][3] = ((int)(AllSetData.CH1OutputVolt * 1000)).ToString();
                                    AllSetData.CurrentCalTable.Rows[AllSetData.CalRowCntNum][4] = ((int)(AllSetData.CH1OutputCurr * 1000)).ToString();
                                    AllSetData.CurrentCalTable.Rows[AllSetData.CalRowCntNum][5] = ((int)(AllSetData.DMMOutputVolt)).ToString();
                                }
                                else if (AllSetData.ChannelSelect == 2)
                                {
                                    AllSetData.CurrentCalTable.Rows[AllSetData.CalRowCntNum][3] = ((int)(AllSetData.CH2OutputVolt * 1000)).ToString();
                                    AllSetData.CurrentCalTable.Rows[AllSetData.CalRowCntNum][4] = ((int)(AllSetData.CH2OutputCurr * 1000)).ToString();
                                    AllSetData.CurrentCalTable.Rows[AllSetData.CalRowCntNum][5] = ((int)(AllSetData.DMMOutputVolt)).ToString();
                                }
                                if (Math.Abs(Convert.ToDouble(AllSetData.CurrentCalTable.Rows[AllSetData.CalRowCntNum][2]) - Convert.ToDouble(AllSetData.CurrentCalTable.Rows[AllSetData.CalRowCntNum][5])) > AllSetData.CalErrRangeCurr)//에러 범위 판단
                                {
                                    DMMCheck = 1;//에러 범위를 넘으면 Cal 실시
                                }
                            }
                            if (AllSetData.ErrorCnt >= AllSetData.CalErrRetryCnt) //에러 카운트 판단
                            {
                                AllSetData.ErrorCnt = 0;//재측정 초기화
                                DMMCheck = 0; //DMM 실측 전송 하지말고 다음으로 넘어가기
                            }
                            if (DMMCheck == 1) //DMM 전송명령
                            {
                                DMMCheck = 0;
                                AllSetData.DelayStart = 1;//딜레이 시작
                                AllSetData.CalOutRealStartFlag = 1; //DMM 전송명령
                                AllSetData.ErrorCnt++;//재측정 횟수 증가
                            }
                            else
                            {
                                AllSetData.CalRowCntNum++; //데이터 카운트 증가
                                AllSetData.CalSeqNum = 3;//Cal 출력으로 다시 돌아가기
                            }
                        }
                        break;
                    case 3:
                        AllSetData.CalOutEndFlag = 1;//포인트 출력 끝나면 종료명령 전송
                        if (AllSetData.CalRowCntNum >= RowCnt)//모든 데이터를 출력했으면 끝
                        {
                            AllSetData.CalSeqNum = 4;
                            AllSetData.DelayStart = 0;//딜레이 없음
                        }
                        else //출력값이 남아있다면
                        {
                            AllSetData.CalSeqNum = 1;
                            AllSetData.DelayStart = 1;//딜레이 시작
                        }
                        break;
                    case 4://종료메세지 띄우기
                        AllSetData.CalSeqStartFlag = 0;
                        AllSetData.CalSeqNum = 0;
                        AllSetData.CalRowCntNum = 0;
                        AllSetData.ErrorCnt = 0;
                        MessageBox.Show("Calibration이 종료되었습니다.");
                        SeqMonitor.Stop();
                        break;
                }
            }
            else if (AllSetData.MeaSeqStartFlag == 1)//실측 시퀀스
            {
                if (AllSetData.VoltCurrSelect == 0) RowCnt = AllSetData.VoltageMeaTable.Rows.Count;//전압 데이터 개수
                else if (AllSetData.VoltCurrSelect == 1) RowCnt = AllSetData.CurrentMeaTable.Rows.Count;//전류 데이터 개수
                if (AllSetData.DelayStart == 1)
                {
                    AllSetData.DelayCnt = AllSetData.DelayCnt + 20;
                    if (AllSetData.DelayCnt > AllSetData.MeaErrDelayTime)
                    {
                        AllSetData.DelayStart = 0;
                        AllSetData.DelayCnt = 0;
                    }
                }
                switch (AllSetData.CalSeqNum)//(0: 대기, 1: Cal 시작, 2: DMM 전송, 3: 출력 종료)
                {
                    case 0:
                        AllSetData.CalSeqNum = 1;
                        AllSetData.CalRowCntNum = 0;
                        break;
                    case 1:
                        if (AllSetData.VoltCurrSelect == 0)//전압 출력
                        {
                            if (AllSetData.DelayStart == 0)
                            {
                                AllSetData.ActCalPointArray = new int[2];
                                AllSetData.ActCalPointArray[0] = AllSetData.MeaPointArray[AllSetData.CalRowCntNum, 0];//전압
                                AllSetData.ActCalPointArray[1] = AllSetData.MeaPointArray[AllSetData.CalRowCntNum, 1];//전류
                                AllSetData.CalOutStartFlag = 1;//수동출력으로 전송
                                AllSetData.DelayStart = 1;
                                AllSetData.CalSeqNum = 2;
                            }
                        }
                        break;
                    case 2:
                        //그리드에 출력하기
                        if (AllSetData.DelayStart == 0)
                        {
                            int DMMCheck = 0;//에러 레인지 범위 안에 들어오는지 판단
                            if (AllSetData.VoltCurrSelect == 0)//전압 출력
                            {
                                if (AllSetData.ChannelSelect == 1) //채널선택
                                {   //단위를 mV, mA 단위로 할것
                                    AllSetData.VoltageMeaTable.Rows[AllSetData.CalRowCntNum][3] = ((int)(AllSetData.CH1OutputVolt * 1000)).ToString();
                                    AllSetData.VoltageMeaTable.Rows[AllSetData.CalRowCntNum][4] = ((int)(AllSetData.CH1OutputCurr * 1000)).ToString();
                                    AllSetData.VoltageMeaTable.Rows[AllSetData.CalRowCntNum][5] = ((AllSetData.DMMOutputVolt)).ToString();
                                }
                                else if (AllSetData.ChannelSelect == 2)
                                {
                                    AllSetData.VoltageMeaTable.Rows[AllSetData.CalRowCntNum][3] = ((int)(AllSetData.CH2OutputVolt * 1000)).ToString();
                                    AllSetData.VoltageMeaTable.Rows[AllSetData.CalRowCntNum][4] = ((int)(AllSetData.CH2OutputCurr * 1000)).ToString();
                                    AllSetData.VoltageMeaTable.Rows[AllSetData.CalRowCntNum][5] = ((AllSetData.DMMOutputVolt)).ToString();
                                }
                                if (Math.Abs(Convert.ToDouble(AllSetData.VoltageMeaTable.Rows[AllSetData.CalRowCntNum][1]) - Convert.ToDouble(AllSetData.VoltageMeaTable.Rows[AllSetData.CalRowCntNum][5])) > AllSetData.MeaErrRangeVolt)//에러 범위 판단
                                {
                                    DMMCheck = 1;//에러 범위를 넘으면 재측정만 실시
                                }
                            }
                            else if (AllSetData.VoltCurrSelect == 1)//전류 출력
                            {
                                if (AllSetData.ChannelSelect == 1) //채널선택
                                {
                                    AllSetData.CurrentMeaTable.Rows[AllSetData.CalRowCntNum][3] = ((int)(AllSetData.CH1OutputVolt * 1000)).ToString();
                                    AllSetData.CurrentMeaTable.Rows[AllSetData.CalRowCntNum][4] = ((int)(AllSetData.CH1OutputCurr * 1000)).ToString();
                                    AllSetData.CurrentMeaTable.Rows[AllSetData.CalRowCntNum][5] = ((int)(AllSetData.DMMOutputVolt)).ToString();
                                }
                                else if (AllSetData.ChannelSelect == 2)
                                {
                                    AllSetData.CurrentMeaTable.Rows[AllSetData.CalRowCntNum][3] = ((int)(AllSetData.CH2OutputVolt * 1000)).ToString();
                                    AllSetData.CurrentMeaTable.Rows[AllSetData.CalRowCntNum][4] = ((int)(AllSetData.CH2OutputCurr * 1000)).ToString();
                                    AllSetData.CurrentMeaTable.Rows[AllSetData.CalRowCntNum][5] = ((int)(AllSetData.DMMOutputVolt)).ToString();
                                }
                                if (Math.Abs(Convert.ToDouble(AllSetData.CurrentMeaTable.Rows[AllSetData.CalRowCntNum][2]) - Convert.ToDouble(AllSetData.CurrentMeaTable.Rows[AllSetData.CalRowCntNum][5])) > AllSetData.MeaErrRangeCurr)//에러 범위 판단
                                {
                                    DMMCheck = 1;//에러 범위를 넘으면 재측정만 실시
                                }
                            }
                            if (AllSetData.ErrorCnt >= AllSetData.MeaErrRetryCnt) //에러 카운트 판단
                            {
                                AllSetData.ErrorCnt = 0;//재측정 초기화
                                DMMCheck = 0; //DMM 실측 전송 하지말고 다음으로 넘어가기
                            }
                            if (DMMCheck == 1) //DMM 전송명령
                            {
                                DMMCheck = 0;
                                AllSetData.ErrorCnt++;//재측정 횟수 증가
                                AllSetData.CalSeqNum = 3;
                            }
                            else
                            {
                                AllSetData.CalRowCntNum++; //데이터 카운트 증가
                                AllSetData.CalSeqNum = 3;
                            }
                        }
                        break;
                    case 3:
                        AllSetData.CalOutEndFlag = 1;//포인트 출력 끝나면 종료명령 전송
                        if (AllSetData.CalRowCntNum >= RowCnt)//모든 데이터를 출력했으면 끝
                        {
                            AllSetData.CalSeqNum = 4;
                            AllSetData.DelayStart = 0;//딜레이 없음
                        }
                        else //출력값이 남아있다면
                        {
                            AllSetData.CalSeqNum = 1;
                            AllSetData.DelayStart = 1;//딜레이 시작
                        }

                        break;
                    case 4://종료메세지 띄우기
                        AllSetData.MeaSeqStartFlag = 0;
                        AllSetData.CalSeqNum = 0;
                        AllSetData.CalRowCntNum = 0;
                        AllSetData.ErrorCnt = 0;
                        MessageBox.Show("실측이 종료되었습니다.");
                        SeqMonitor.Stop();
                        break;
                }
            }
        }
        //시리얼 번호입력(실시간변화)
        private void SerialTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AllSetData.SerialNumber = SerialTextBox.Text;
        }
        //세팅페이지 호출버튼 이벤트
        private void SettingCommClick(object sender, RoutedEventArgs e)
        {
            PageFrame.Source = new Uri("SettingCommPage.xaml", UriKind.Relative);
        }
        private void SettingShuntClick(object sender, RoutedEventArgs e)
        {
            PageFrame.Source = new Uri("SettingShuntPage.xaml", UriKind.Relative);
        }
        private void SettingAutoSaveClick(object sender, RoutedEventArgs e)
        {
            PageFrame.Source = new Uri("SettingAutoSavePage.xaml", UriKind.Relative);
        }
        private void SettingCalClick(object sender, RoutedEventArgs e)
        {
            PageFrame.Source = new Uri("SettingCalPage.xaml", UriKind.Relative);
        }
        private void SettingMeaClick(object sender, RoutedEventArgs e)
        {
            PageFrame.Source = new Uri("SettingMeaPage.xaml", UriKind.Relative);
        }
        private void SettingOthersClick(object sender, RoutedEventArgs e)
        {
            PageFrame.Source = new Uri("SettingOthersPage.xaml", UriKind.Relative);
        }
    }
}
