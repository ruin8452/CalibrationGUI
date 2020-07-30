using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.Message
{
    public class CalPointMessege : MessageBase
    {
        public bool CalMode { get; set; }
        public List<object[]> CalPointList { get; set; }
    }
}
