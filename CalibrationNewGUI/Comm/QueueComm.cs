using J_Project.Manager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows.Threading;

namespace J_Project.Communication.CommModule
{
    /**
     *  @brief 통신 클래스
     *  @details 큐를 사용하여 반이중 통신을 관리
     *
     *  @author SSW
     *  @date 2020.08.18
     *  @version 1.0.0
     */
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

        DispatcherTimer timers = new DispatcherTimer();
        BackgroundWorker queueBackString = new BackgroundWorker();
        BackgroundWorker queueBackByte = new BackgroundWorker();

        public QueueComm(string dataPacketType)
        {
            dataType = dataPacketType;

            timers.Interval = TimeSpan.FromMilliseconds(100);

            queueBackString.DoWork += new DoWorkEventHandler((object send, DoWorkEventArgs e) =>
            {
                TimerSenderString(null, EventArgs.Empty);
            });
            queueBackByte.DoWork += new DoWorkEventHandler((object send, DoWorkEventArgs e) =>
            {
                TimerSenderByte(null, EventArgs.Empty);
            });
        }

        /**
         *  @brief 통신 연결
         *  @details 시리얼 통신 연결
         *  @version 1.0.0
         *  
         *  @param string portName 포트 이름
         *  @param int baudRate 통신 속도
         *  
         *  @return string 연결 결과
         */
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
                        //timers.Tick += TimerSenderString;
                        timers.Tick += new EventHandler((object sender, EventArgs e) =>
                         {
                             if (queueBackString.IsBusy == false)
                                 queueBackString.RunWorkerAsync();
                         });
                    }
                    else if (dataType == "byte[]")
                    {
                        ComPort.DataReceived += IntterruptReceiverByte;
                        //timers.Tick += TimerSenderByte;
                        timers.Tick += new EventHandler((object sender, EventArgs e) =>
                        {
                            if (queueBackByte.IsBusy == false)
                                queueBackByte.RunWorkerAsync();
                        });
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

        /**
         *  @brief 통신 연결 해제
         *  @details 시리얼 통신 연결 해제
         *  @version 1.0.0
         *  
         *  @param
         *  
         *  @return bool 연결 해제 결과@n
         *               True : 명령저장 성공@n
         *               False : 명령저장 실패
         */
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

        /**
         *  @brief 명령 전송 스택(명령 타입 : string)
         *  @details 전송할 명령을 큐에 쌓는다.
         *  @version 1.0.0
         *  
         *  @param string cmd 전송할 명령
         *  @param out int code 명령에 대한 고유코드
         *  
         *  @return bool 명령 스택 성공여부@n
         *               True : 명령저장 성공@n
         *               False : 명령저장 실패
         */
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

        /**
         *  @brief 명령 전송 스택(명령 타입 : byte[])
         *  @details 전송할 명령을 큐에 쌓는다.
         *  @version 1.0.0
         *  
         *  @param byte[] cmd 전송할 명령
         *  @param out int code 명령에 대한 고유코드
         *  
         *  @return bool 명령 스택 성공여부@n
         *               True : 명령저장 성공@n
         *               False : 명령저장 실패
         */
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

        /**
         *  @brief 수신받은 데이터 가져가기(데이터 타입 : string)
         *  @details 수신받은 데이터가 쌓여있는 큐에서 코드에 해당하는 데이터를 찾아 리턴한다.
         *  @version 1.0.0
         *  
         *  @param out string data 수신받은 응답 데이터
         *  @param int code 찾고 싶은 데이터에 대한 고유코드
         *  
         *  @return bool 데이터 서치 성공여부@n
         *               True : 서치 성공@n
         *               False : 서치 실패
         */
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

                Util.Delay(0.1);
            }

            data = string.Empty;
            return false;
        }

        /**
         *  @brief 수신받은 데이터 가져가기(데이터 타입 : byte[])
         *  @details 수신받은 데이터가 쌓여있는 큐에서 코드에 해당하는 데이터를 찾아 리턴한다.
         *  @version 1.0.0
         *  
         *  @param out byte[] data 수신받은 응답 데이터
         *  @param int code 찾고 싶은 데이터에 대한 고유코드
         *  
         *  @return bool 데이터 서치 성공여부@n
         *               True : 서치 성공@n
         *               False : 서치 실패
         */
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

                Util.Delay(0.1);
            }

            data = null;
            return false;
        }

        /**
         *  @brief 명령 전송 타이머 함수(데이터 타입 : string)
         *  @details 주기적으로 큐에 쌓여있는 명령을 전송한다.
         *  @version 1.0.0
         *  
         *  @param object sender 함수를 호출한 객체(사용 안함)
         *  @param EventArgs e 이벤트 변수(사용 안함)
         *  
         *  @return 
         */
        private void TimerSenderString(object sender, EventArgs e)
        {
            Debug.WriteLine($"Send QUEUE Count : {SendCommQueue.Count}");
            if (SendCommQueue.Count <= 0)
                return;

            tempPacket = SendCommQueue.Dequeue();

            if (ComPort.IsOpen)
                ComPort.WriteLine(tempPacket.StrData);
        }

        /**
         *  @brief 명령 전송 타이머 함수(데이터 타입 : byte[])
         *  @details 주기적으로 큐에 쌓여있는 명령을 전송한다.
         *  @version 1.0.0
         *  
         *  @param object sender 함수를 호출한 객체(사용 안함)
         *  @param EventArgs e 이벤트 변수(사용 안함)
         *  
         *  @return 
         */
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

        /**
         *  @brief 데이터 수신 인터럽트 함수(데이터 타입 : string)
         *  @details 데이터가 
         *  @version 1.0.0
         *  
         *  @param object sender 함수를 호출한 객체(사용 안함)
         *  @param EventArgs e 이벤트 변수(사용 안함)
         *  
         *  @return 
         */
        private void IntterruptReceiverString(object sender, EventArgs e)
        {
            Debug.WriteLine("QUEUE Intterrupt Receive");
            string receiveString = string.Empty;

            if (ComPort.IsOpen)
                receiveString = ComPort.ReadLine();

            Debug.WriteLine($"Receive Data : {tempPacket.ID}, {receiveString}");
            ReceiveCommQueue.Add(new CommPacket(tempPacket.ID, receiveString));
        }

        /**
         *  @brief 데이터 수신 타이머 함수(데이터 타입 : byte[])
         *  @details 주기적으로 큐에 쌓여있는 명령을 전송한다.
         *  @version 1.0.0
         *  
         *  @param object sender 함수를 호출한 객체(사용 안함)
         *  @param EventArgs e 이벤트 변수(사용 안함)
         *  
         *  @return 
         */
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
