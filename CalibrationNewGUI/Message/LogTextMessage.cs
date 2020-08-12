using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.Message
{
    public class LogTextMessage : MessageBase
    {
        public string LogText { get; set; }
        public bool IsMonitoring { get; set; }
    }
}
