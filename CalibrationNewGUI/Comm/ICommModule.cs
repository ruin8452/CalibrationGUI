
using J_Project.Communication.CommFlags;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J_Project.Communication.CommModule
{
    public interface ICommModule
    {
        string Connect(string visaAddress);
        string Connect(string portName, int baudRate);
        TryResultFlag Disconnect();
        TryResultFlag CommSend(string cmd);
        TryResultFlag CommSend(byte[] cmd);
        TryResultFlag CommReceive(out string receiveString);
        TryResultFlag CommReceive(out byte[] receiveString);
        void TokenReset();
    }
}
