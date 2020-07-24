using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CalibrationNewGUI.Equipment.DigitalMeter
{
    public interface IDmm
    {
        string ErrMsg { get; }
        bool Connect(string portName, int borate);
        bool Disconnect();

        void Setting();
        double RealSensing();
    }
}
