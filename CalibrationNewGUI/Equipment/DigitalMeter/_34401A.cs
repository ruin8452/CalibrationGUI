using J_Project.Communication.CommModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CalibrationNewGUI.Equipment.DigitalMeter
{
    public class _34401A : IDmm
    {
        public string ErrMsg { get; private set; }
        public int CommErrCount = 0;

        QueueComm DmmComm = new QueueComm("string");

        public _34401A()
        {
        }

        public bool Connect(string portName, int borate)
        {
            string msg = DmmComm.Connect(portName, borate);
            if (msg != "Connected!")
            {
                ErrMsg = msg;
                return false;
            }

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

            return sensingData;
        }

        public void Setting()
        {
            DmmComm.CommSend("SYSTem:REMote", out int _);
            DmmComm.CommSend("ZERO:AUTO ONCE", out int _);
        }
    }
}
