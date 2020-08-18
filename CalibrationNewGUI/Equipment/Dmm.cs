using CalibrationNewGUI.Equipment.DigitalMeter;
using CalibrationNewGUI.Message;
using CalibrationNewGUI.Model;
using GalaSoft.MvvmLight.Messaging;
using J_Project.Communication.CommModule;
using PropertyChanged;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace CalibrationNewGUI.Equipment
{
    [ImplementPropertyChanged]
    public class Dmm
    {
        double SensingData;
        public DmmInfo DmmInfos { get; set; }//using 추가 및 DMM 정보 데이터 가져오기
        public ShuntInfo shuntInfos { get; set; }//shunt 정보 데이터 가져오기
        public double Volt { get; set; }
        public double Curr { get; set; }
        public bool IsConnected { get; private set; } = false;

        public int CommErrCount = 0;

        public QueueComm DmmComm = new QueueComm("string");

        BackgroundWorker moniBack = new BackgroundWorker();
        DispatcherTimer MonitoringTimer = new DispatcherTimer();

        #region 싱글톤 패턴 구현
        private static Dmm SingleTonObj = null;

        private Dmm()
        {
            DmmInfos = DmmInfo.GetObj();//DMM 데이터 가져오기
            shuntInfos = ShuntInfo.GetObj();//shunt 데이터 가져오기
            moniBack.DoWork += new DoWorkEventHandler((object send, DoWorkEventArgs e) =>
            {
                DmmMonitoring();
            });

            MonitoringTimer.Interval = TimeSpan.FromMilliseconds(100);    // ms
            MonitoringTimer.Tick += new EventHandler((object sender, EventArgs e) =>
            {
                if (moniBack.IsBusy == false)
                    moniBack.RunWorkerAsync();
            });
        }

        public static Dmm GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new Dmm();
            return SingleTonObj;
        }
        #endregion 싱글톤 패턴 구현

        public string Connect(string portName, int baudRate)
        {
            string msg = DmmComm.Connect(portName, baudRate);
            if (msg == "Connected!")
                IsConnected = true;

            return msg;
        }
        public bool Disconnect()
        {
            bool result = DmmComm.Disconnect();
            if (result == true)
                IsConnected = false;

            return result;
        }

        // 모니터링 관련
        public void MonitorStart()
        {
            MonitoringTimer.Start();
        }
        public void MonitorStop()
        {
            MonitoringTimer.Stop();
        }

        //DMM모니터링용 타이머
        private void DmmMonitoring()
        {
            SensingData = RealSensing();

            Volt = Math.Round(SensingData * 1000, 1);
            Curr = Math.Round(SensingData * shuntInfos.shuntReg * 1000000, 1);
        }
        

        public double RealSensing()
        {
            bool commFlag = DmmComm.CommSend("READ?", out int code);
            if (!commFlag) { CommErrCount++; return double.NaN; }

            commFlag = DmmComm.CommReceive(out string receiveData, code);
            if (!commFlag) { CommErrCount++; return SensingData; }

            OnLogSend(receiveData, true);

            commFlag = double.TryParse(receiveData, out double tempData);
            if (!commFlag) { CommErrCount++; return SensingData; }

            return tempData;
        }

        public void Setting()
        {
            //DMM데이터에 따라 세팅값 다르게 하기
            switch (DmmInfos.ModelName)
            {
                case "34450A":
                    break;
                case "34401A":
                    DmmComm.CommSend("SYSTem:REMote", out int _);
                    OnLogSend("SYSTem:REMote", false);
                    DmmComm.CommSend("ZERO:AUTO ONCE", out int _);
                    OnLogSend("ZERO:AUTO ONCE", false);
                    break;
                case "Keithley2000":
                    DmmComm.CommSend("SYST:REM", out int _);
                    OnLogSend("SYST:REM", false);
                    DmmComm.CommSend("CONF:VOLT:DC", out int _);
                    OnLogSend("CONF:VOLT:DC", false);

                    //DmmComm.CommSend("SENS:VOLT:DC:NPLC 0.1", out int _); // FAST
                    //DmmComm.CommSend("SENS:VOLT:DC:NPLC 1", out int _);   // MID
                    DmmComm.CommSend("SENS:VOLT:DC:NPLC 10", out int _);    // LOW
                    OnLogSend("SENS:VOLT:DC:NPLC 10", false);
                    break;
            }

        }

        /**
         *  @brief 로그 텍스트 메세지 전송
         *  @details 로그 텍스트 메세지를 전송
         *  
         *  @param string text 로그 텍스트
         *  @param bool isMonitoring 모니터링 텍스트 여부
         *  
         *  @return
         */
        private void OnLogSend(string text, bool isMonitoring)
        {
            LogTextMessage Message = new LogTextMessage
            {
                LogText = text,
                IsMonitoring = isMonitoring
            };

            Messenger.Default.Send(Message);
        }
    }
}
