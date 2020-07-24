using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.ViewModel.Func.EventArgsClass
{
    public class CalMonitorArgs : EventArgs
    {
        public int Index { get; set; }

        public CalMonitorArgs(int index)
        {
            Index = index;
        }
    }
}
