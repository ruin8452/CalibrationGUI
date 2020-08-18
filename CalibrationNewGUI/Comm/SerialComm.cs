using J_Project.Communication.CommFlags;
using J_Project.Manager;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace J_Project.Communication.CommModule
{
    /**
     *  @brief 통신 클래스
     *  @details 시리얼을 사용하여 반이중 통신을 관리
     *
     *  @author SSW
     *  @date 2020.08.18
     *  @version 1.0.0
     */
    public class SerialComm : ICommModule
    {
        const double SEND_DELAY = 0.1;
        const double RECIVE_DELAY = 0.8;
        const int MAX_RETRY = 3;

        SerialPort ComPort;
        bool IsReceive = false;
        public bool IsReceiveByte = false;
        public bool IsReceiveString = false;

        readonly object lockObj = 0;

        int token = 0;
        string id;
        int baudRate;
        public string receiveDataString; //리시브 데이터를 받기 위한 변수(string)

        public byte[] receiveDataByte = new byte[50]; //리시브 데이터를 받기 위한 변수(byte) 링버퍼
        public int receiveDataByteSTX = 0;
        public int receiveDataByteETX = 0;

        public SerialComm(string id, int baudRate)
        {
            this.id = id;
            this.baudRate = baudRate;
        }

        /**
         *  @brief 장비 접속
         *  @details Comport로 장비에 접속
         *  
         *  @param portName 포트번호
         *  @param baudRate 통신 속도
         *  
         *  @return 접속 시도 결과
         */
        public string Connect(string visaAddress)
        {
            return "Couldn't Connected!";
        }

        /**
         *  @brief 장비 접속
         *  @details Comport로 장비에 접속
         *  
         *  @param string portName 포트번호
         *  @param int baudRate 통신 속도
         *  
         *  @return 접속 시도 결과
         */
        public string Connect(string portName, int baudRate)
        {
            try
            {
                ComPort = new SerialPort(portName, baudRate);

                ComPort.Open();

                if (ComPort.IsOpen)
                {
                    //ComPort.DataReceived += CommReciveFlag;
                    if(this.id == "DMM") ComPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandlerString);
                    else if (this.id == "MCU") ComPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandlerByte);
                    token = 0;

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
         *  @brief 장비 접속 해제
         *  @details
         *  
         *  @param
         *  
         *  @return 접속 해제 시도 결과
         */
        public TryResultFlag Disconnect()
        {
            try
            {
                if(ComPort != null)
                {
                    ComPort.Close();
                    ComPort.Dispose();
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine("ErrPos07" + e.Message);
            }

            return TryResultFlag.SUCCESS;
        }

        /**
         *  @brief 명령 송신
         *  @details 문자열 형식의 명령문 전송
         *  
         *  @param string cmd 명령 문자열
         *  
         *  @return TryResultFlag 명령 송신 성공 여부
         */
        public TryResultFlag CommSend(string cmd)
        {
            for (int i = 0; i < MAX_RETRY; i++)
            {
                /*
                // 토큰 할당
                lock (lockObj)
                {
                    if (token == 0)
                        token = Thread.CurrentThread.ManagedThreadId;
                    else
                    {
                        Util.Delay(SEND_DELAY);
                        continue;
                    }
                }
                */
                // 명령 전송
                try
                {
                    ComPort.WriteLine(cmd);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("ErrPos04 : " + e.Message);
                    continue;
                }
                Util.Delay(0.3);

                return TryResultFlag.SUCCESS;
            }
            return TryResultFlag.FAIL;
        }

        /**
         *  @brief 명령 송신
         *  @details 바이트 배열 형식의 명령문 전송
         *  
         *  @param byte[] cmd 명령 문자열
         *  
         *  @return TryResultFlag 명령 송신 성공 여부
         */
        public TryResultFlag CommSend(byte[] cmd)
        {
            for (int i = 0; i < MAX_RETRY; i++)
            {
                /*
                // 토큰 할당
                lock (lockObj)
                {
                    Debug.WriteLine("send : " + Thread.CurrentThread.ManagedThreadId);
                    if (token == 0)
                        token = Thread.CurrentThread.ManagedThreadId;
                    else
                    {
                        Debug.WriteLine("Now Token : " + token);
                        Util.Delay(SEND_DELAY);
                        continue;
                    }
                }
                */
                // 명령 전송
                try
                {
                    ComPort.Write(cmd, 0, cmd.Length);
                }
                catch (InvalidOperationException e)
                {
                    Debug.WriteLine("ErrPos08 : " + e.Message);
                    token = 0;
                    Disconnect();
                    Connect(id, baudRate);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("ErrPos05 : " + e.Message);
                    token = 0;
                    continue;
                }
                Util.Delay(0.3);

                return TryResultFlag.SUCCESS;
            }
            return TryResultFlag.FAIL;
        }

        /**
         *  @brief 명령 수신
         *  @details
         *  
         *  @param out string receiveString 수신 문자열
         *  
         *  @return TryResultFlag 수신 문자열
         */
        public TryResultFlag CommReceive(out string receiveString)
        {
            receiveString = string.Empty;

            for (int i = 0; i < MAX_RETRY; i++)
            //for (int i = 0; i < 8000; i++)
            {
                // 토큰 검사
                if (token != Thread.CurrentThread.ManagedThreadId)
                {
                    Util.Delay(RECIVE_DELAY);
                    //Thread.Sleep(1);
                    //Util.Delay(0.01);
                    continue;
                }

                // 데이터 수신
                if (IsReceive == true)
                {
                    IsReceive = false;
                    receiveString = ComPort.ReadLine().Trim('\r', '\n', ' ');

                    if (receiveString == "ON") receiveString = "1";
                    else if (receiveString == "OFF") receiveString = "0";

                    lock (lockObj)
                    {
                        token = 0;
                    }

                    return TryResultFlag.SUCCESS;
                }
                //Util.Delay(0.01);
                Util.Delay(RECIVE_DELAY);
                //Thread.Sleep(1);
            }
            lock (lockObj)
                token = 0;

            return TryResultFlag.FAIL;
        }

        /**
         *  @brief 명령 수신
         *  @details
         *  
         *  @param out byte[] receiveStream 수신 바이트 배열
         *  
         *  @return TryResultFlag 수신 바이트배열
         */
        public TryResultFlag CommReceive(out byte[] receiveStream, string callPosition)
        {
            receiveStream = null;

            for (int i = 0; i < MAX_RETRY; i++)
            {
                // 토큰 검사
                if (token != Thread.CurrentThread.ManagedThreadId)
                {
                    Util.Delay(RECIVE_DELAY);
                    continue;
                }

                Debug.WriteLine(callPosition + " Receive : " + Thread.CurrentThread.ManagedThreadId);

                try
                {
                    if (!IsReceive) continue;

                    IsReceive = false;
                    int reciveNum = ComPort.BytesToRead;
                    //receiveStream = new byte[51];
                    receiveStream = new byte[reciveNum];

                    Debug.WriteLine("수신 바이트 : " + reciveNum);

                    //ComPort.Read(receiveStream, 0, 51);
                    ComPort.Read(receiveStream, 0, reciveNum);

                    lock (lockObj)
                    {
                        token = 0;
                    }

                    return TryResultFlag.SUCCESS;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("ErrPos06 : " + e.Message);
                    token = 0;
                    continue;
                }
            }
            lock (lockObj)
                token = 0;

            return TryResultFlag.FAIL;
        }

        /**
         *  @brief 토큰 리셋
         *  @details
         *  
         *  @param 
         *  
         *  @return 
         */
        public void TokenReset()
        {
            lock (lockObj)
            {
                token = 0;
            }
        }

        /**
         *  @brief RS232 통신 수신 알림
         *  @details RS232 통신 수신 시 수신 플래그 세움
         *  
         *  @param object sender 함수를 호출한 객체(사용 안함)
         *  @param EventArgs e 이벤트 변수(사용 안함)
         *  
         *  @return 
         */
        private void CommReciveFlag(object sender, EventArgs e)
        {
            IsReceive = true;
        }

        public TryResultFlag CommReceive(out byte[] receiveString)
        {
            throw new NotImplementedException();
        }

        //스트링 데이터용 리시브 핸들러
        private void DataReceivedHandlerString(object sender, SerialDataReceivedEventArgs e)
        {
            string indata = ComPort.ReadLine().Trim('\r', '\n', ' ');
            
            if (indata == "ON") indata = "1";
            else if (indata == "OFF") indata = "0";

            if(indata != null) receiveDataString = indata;
            IsReceiveString = true;
        }

        //배열 데이터용 리시브 핸들러
        private void DataReceivedHandlerByte(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] receiveStream = null;
            int reciveNum = ComPort.BytesToRead;
            receiveStream = new byte[reciveNum];
            ComPort.Read(receiveStream, 0, reciveNum);
            for (int i = 0; i < reciveNum; i++)
            {
                if (receiveStream[i] == 0x02)
                {
                    //STX가 들어오면 그 배열부터 시작할것
                    receiveDataByteSTX = (receiveDataByteETX+1)%50;//최대길이50

                    receiveDataByteETX = receiveDataByteSTX;
                }
                receiveDataByte[receiveDataByteETX] = receiveStream[i];
                receiveDataByteETX++;
                receiveDataByteETX = receiveDataByteETX % 50;
            }
            IsReceiveByte = true;
        }
    }
}
