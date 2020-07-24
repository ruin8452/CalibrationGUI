using J_Project.Communication.CommModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CalibrationNewGUI.Equipment.DigitalMeter
{
    public class Keithley2000 : IDmm
    {
        public string ErrMsg { get; private set; }

        public int CommErrCount = 0;

        QueueComm DmmComm = new QueueComm("string");

        public Keithley2000()
        {

        }

        public bool Connect(string portName, int borate)
        {
            ErrMsg = DmmComm.Connect(portName, borate);
            if (ErrMsg != "Connected!")
                return false;

            return true;
        }
        public bool Disconnect()
        {
            return DmmComm.Disconnect();
        }

        public double RealSensing()
        {
            bool commFlag = DmmComm.CommSend("READ?", out int code);
            if (!commFlag) { CommErrCount++; return double.NaN; }

            commFlag = DmmComm.CommReceive(out string receiveData, code);
            if (!commFlag) { CommErrCount++; return double.NaN; }

            commFlag = double.TryParse(receiveData, out double sensingData);
            if (!commFlag) { CommErrCount++; return double.NaN; }

            return sensingData * 1000;
        }

        public void Setting()
        {
            DmmComm.CommSend("SYST:REM", out int _);
            DmmComm.CommSend("CONF:VOLT:DC", out int _);

            //DmmComm.CommSend("SENS:VOLT:DC:NPLC 0.1", out int _);   // FAST
            //DmmComm.CommSend("SENS:VOLT:DC:NPLC 1", out int _);     // MID
            DmmComm.CommSend("SENS:VOLT:DC:NPLC 10", out int _);    // LOW
        }
    }
}