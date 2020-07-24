using J_Project.Manager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows.Threading;

namespace J_Project.Communication.CommModule
{
    public class QueueComm
    {
        SerialPort ComPort;

        int idCode = 1;
        readonly object lockObj = 0;
        string dataType;

        struct CommPacket
        {
            public int ID;
            public byte[] ByteData;
            public string StrData;

            public CommPacket(int id, byte[] data)
            {
                ID = id;
                ByteData = data;
                StrData = string.Empty;
            }
            public CommPacket(int id, string data)
            {
                ID = id;
                ByteData = null;
                StrData = data;
            }
        }

        Queue<CommPacket> SendCommQueue = new Queue<CommPacket>();
        List<CommPacket> ReceiveCommQueue = new List<CommPacket>();
        CommPacket tempPacket = new CommPacket();
        //Timer timer = new Timer(TimerSenderByte, null, 0, 100);
        DispatcherTimer timers = new DispatcherTimer();

        public QueueComm(string dataPacketType)
        {
            dataType = dataPacketType;

            timers.Interval = TimeSpan.FromMilliseconds(200);
        }

        public string Connect(string portName, int baudRate)
        {
            try
            {
                ComPort = new SerialPort(portName, baudRate);

                ComPort.Open();

                if (ComPort.IsOpen)
                {
                    if (dataType == "string")
                    {
                        ComPort.DataReceived += IntterruptReceiverString;
                        timers.Tick += TimerSenderString;
                    }
                    else if (dataType == "byte[]")
                    {
                        ComPort.DataReceived += IntterruptReceiverByte;
                        timers.Tick += TimerSenderByte;
                    }
                    timers.Start();

                    return "Connected!";
                }
                else
                    ComPort.Close();
            }
            catch (IOException)
            {
                ComPort.Close();
                return "Not Find Port";
            }
            catch (UnauthorizedAccessException)
            {
                ComPort.Close();
                return "Alread Using Port";
            }
            catch (InvalidOperationException)
            {
                ComPort.Close();
                return "Alread Open Port";
            }
            catch (ArgumentException)
            {
                ComPort.Close();
                return "Wrong Port Name";
            }

            return "Couldn't Connected!";
        }

        public bool Disconnect()
        {
            try
            {
                if (ComPort != null)
                {
                    ComPort.Close();
                    ComPort.Dispose();

                    SendCommQueue.Clear();
                    ReceiveCommQueue.Clear();

                    timers.Stop();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("ErrPos07" + e.Message);
                return false;
            }

            return true;
        }

        public bool CommSend(string cmd, out int code)
        {
            CommPacket data = new CommPacket();

            lock (lockObj)
            {
                data.ID = code = idCode++;
                Debug.WriteLine($"Send code : {code}");
            }
            data.StrData = cmd;

            SendCommQueue.Enqueue(data);

            return true;
        }

        public bool CommSend(byte[] cmd, out int code)
        {
            CommPacket data = new CommPacket();

            lock(lockObj)
            {
                data.ID = code = idCode++;
                Debug.WriteLine($"Send code : {code}");
            }
            data.ByteData = cmd;

            SendCommQueue.Enqueue(data);

            return true;
        }

        public bool CommReceive(out string data, int code)
        {
            StringBuilder receiveData = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                List<CommPacket> dataPacket = ReceiveCommQueue.FindAll(x => x.ID == code);

                Debug.WriteLine($"Command Receive Count : {dataPacket.Count}");

                // 모든 데이터를 하나로 합치기
                foreach (var packet in dataPacket)
                    receiveData.Append(packet.StrData);

                data = receiveData.ToString();

                if (dataPacket.Count != 0 && dataPacket[0].ID != 0)
                {
                    Debug.WriteLine($"Receive Succ - code : {code}");
                    return true;
                }

                Utill.Delay(0.1);
            }

            data = string.Empty;
            return false;
        }

        public bool CommReceive(out byte[] data, int code)
        {
            List<byte> receiveData = new List<byte>();
            for (int i = 0; i < 10; i ++)
            {
                List<CommPacket> dataPacket = ReceiveCommQueue.FindAll(x => x.ID == code);

                Debug.WriteLine($"Command Receive Count : {dataPacket.Count}");

                // 모든 데이터를 하나로 합치기
                foreach (var packet in dataPacket)
                    receiveData.AddRange(packet.ByteData);

                data = receiveData.ToArray();

                if (dataPacket.Count != 0 && dataPacket[0].ID != 0)
                {
                    Debug.WriteLine($"Receive Succ - code : {code}");
                    return true;
                }

                Utill.Delay(0.1);
            }

            data = null;
            return false;
        }

        private void TimerSenderString(object sender, EventArgs e)
        {
            Debug.WriteLine($"Send QUEUE Count : {SendCommQueue.Count}");
            if (SendCommQueue.Count <= 0)
                return;

            tempPacket = SendCommQueue.Dequeue();

            if (ComPort.IsOpen)
                ComPort.WriteLine(tempPacket.StrData);
        }

        private void TimerSenderByte(object sender, EventArgs e)
        {
            Debug.WriteLine($"Send QUEUE Count : {SendCommQueue.Count}");
            if (SendCommQueue.Count <= 0)
                return;

            tempPacket = SendCommQueue.Dequeue();

            if (ComPort.IsOpen)
                ComPort.Write(tempPacket.ByteData, 0, tempPacket.ByteData.Length);
            else
                ComPort.Open();

        }

        private void IntterruptReceiverString(object sender, EventArgs e)
        {
            Debug.WriteLine("QUEUE Intterrupt Receive");
            string receiveString = string.Empty;

            if (ComPort.IsOpen)
                receiveString = ComPort.ReadLine();

            Debug.WriteLine($"Receive Data : {tempPacket.ID}, {receiveString}");
            ReceiveCommQueue.Add(new CommPacket(tempPacket.ID, receiveString));
        }

        private void IntterruptReceiverByte(object sender, EventArgs e)
        {
            Debug.WriteLine("QUEUE Intterrupt Receive");
            byte[] receiveStream = new byte[ComPort.BytesToRead];

            if (ComPort.IsOpen)
                ComPort.Read(receiveStream, 0, receiveStream.Length);

            Debug.WriteLine($"Receive Data : {tempPacket.ID}, {Encoding.ASCII.GetString(receiveStream)}");
            ReceiveCommQueue.Add(new CommPacket(tempPacket.ID, receiveStream));
        }
    }
}
