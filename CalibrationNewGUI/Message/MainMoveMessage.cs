using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.Message
{
    class MainMoveMessage : MessageBase
    {
        public bool LangMove { get; set; }
        public bool ShuntMode { get; set; }
    }
}
