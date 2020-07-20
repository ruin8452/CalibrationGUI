using CalibrationNewGUI.Equipment.DigitalMeter;
using CalibrationNewGUI.Model;
using J_Project.Communication.CommFlags;
using J_Project.Communication.CommModule;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CalibrationNewGUI.Equipment
{
    [ImplementPropertyChanged]
    public class Dmm
    {
        public double SensingData { get; private set; }
        public bool IsConnected { get; private set; }

        IDmm dmmObj;

        public QueueComm DmmComm = new QueueComm("string");//MCU 용
        DispatcherTimer MonitoringTimer = new DispatcherTimer(); //MCU 모니터링 타이머용

        #region 싱글톤 패턴 구현
        private static Dmm SingleTonObj = null;

        public Dmm(IDmm dmm)
        {
            dmmObj = dmm;
        }

        private Dmm()
        {
            MonitoringTimer.Interval = TimeSpan.FromMilliseconds(500);    // ms
            MonitoringTimer.Tick += DmmMonitoring;
        }

        public static Dmm GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new Dmm();
            return SingleTonObj;
        }
        #endregion 싱글톤 패턴 구현

        public string Connect(string portName, int borate)
        {
            if (dmmObj.Connect(portName, borate))
                IsConnected = true;
            else
                IsConnected = false;

            return dmmObj.ErrMsg;
        }
        public void Disconnect()
        {
            if (dmmObj.Disconnect())
                IsConnected = false;
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
        private void DmmMonitoring(object sender, EventArgs e)
        {
            SensingData = dmmObj.RealSensing();
        }
    }
}
