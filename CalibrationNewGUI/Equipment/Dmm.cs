using CalibrationNewGUI.Equipment.DigitalMeter;
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
        public double SensingData { get; set; }
        public bool IsConnected { get; private set; } = false;

        public int CommErrCount = 0;

        public QueueComm DmmComm = new QueueComm("string");

        BackgroundWorker moniBack = new BackgroundWorker();
        DispatcherTimer MonitoringTimer = new DispatcherTimer();

        #region 싱글톤 패턴 구현
        private static Dmm SingleTonObj = null;

        private Dmm()
        {
            moniBack.DoWork += new DoWorkEventHandler((object send, DoWorkEventArgs e) =>
            {
                DmmMonitoring();
            });

            MonitoringTimer.Interval = TimeSpan.FromMilliseconds(500);    // ms
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

        public string Connect(string portName, int borate)
        {
            string msg = DmmComm.Connect(portName, borate);
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
            SensingData = Math.Round(RealSensing(), 2);
        }


        public double RealSensing()
        {
            bool commFlag = DmmComm.CommSend("READ?", out int code);
            if (!commFlag) { CommErrCount++; return double.NaN; }

            commFlag = DmmComm.CommReceive(out string receiveData, code);
            if (!commFlag) { CommErrCount++; return SensingData; }

            commFlag = double.TryParse(receiveData, out double sensingData);
            if (!commFlag) { CommErrCount++; return SensingData; }

            return sensingData * 1000;
        }

        public void Setting()
        {
            DmmComm.CommSend("SYST:REM", out int _);
            DmmComm.CommSend("CONF:VOLT:DC", out int _);

            //DmmComm.CommSend("SENS:VOLT:DC:NPLC 0.1", out int _); // FAST
            //DmmComm.CommSend("SENS:VOLT:DC:NPLC 1", out int _);   // MID
            DmmComm.CommSend("SENS:VOLT:DC:NPLC 10", out int _);    // LOW
        }
    }
}
