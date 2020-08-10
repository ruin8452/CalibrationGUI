using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.Message
{
    public class CalOptionMessage : MessageBase
    {
        public char CalType { get; set; }
        public int ChNumber { get; set; }
    }
}
