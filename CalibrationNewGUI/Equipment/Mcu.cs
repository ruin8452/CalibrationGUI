using CalibrationNewGUI.Message;
using GalaSoft.MvvmLight.Messaging;
using J_Project.Communication.CommModule;
using Modbus.Device;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace CalibrationNewGUI.Equipment
{
    [ImplementPropertyChanged]
    public class Mcu
    {
        //Modbus용 변수 20.07.29
        public SerialPort commModbusMCU;
        public ModbusSerialMaster MdMaster;
        public ushort[] MdMasterBuffer;//모드버스 응답용 버퍼

        [StructLayout(LayoutKind.Explicit)]
        public struct UnionConv
        {
            [FieldOffset(0)]
            public float Float;
            [FieldOffset(0)]
            public ushort Byte1;
            [FieldOffset(2)]
            public ushort Byte2;
        }

        const int SLAVE_ID = 1;
        const ushort MONITORING_ADDRESS = 0x1200;
        const ushort REF_SET_ADDRESS = 0x2000;
        const ushort CAL_VALUE_ADDRESS = 0x2010;
        //임시큐 플래그
        public int sendReadMonitoringFlag = 0; //모니터링 읽기
        public int sendWriteOutputStartFlag = 0; //실제 출력
        public int sendWriteOutputStopFlag = 0; //정지
        public int sendWriteDMMStartFlag = 0; //DMM 값 전송
        public struct OutputBuffer //출력용 버퍼
        {
            public int chNum;
            public int volt;
            public int curr;
        }
        public OutputBuffer outputBuffer = new OutputBuffer();
        public struct CalBuffer //cal 실행용 버퍼
        {
            public char calType;
            public int chNum;
            public double dmm;
        }
        public CalBuffer calBuffer = new CalBuffer();

        /////////////////////////////////////////////////////////////////////
        public double Ch1Volt { get; set; }
        public double Ch1Curr { get; set; }
        public double Ch2Volt { get; set; }
        public double Ch2Curr { get; set; }

        public bool IsRun = false;
        public int Ch1Fault;
        public int Ch2Fault;
        public int Version;

        public bool IsConnected { get; set; } = false;
        public int CommErrCount = 0;

        QueueComm McuComm = new QueueComm("byte[]");

        BackgroundWorker moniBack = new BackgroundWorker();
        DispatcherTimer MonitoringTimer = new DispatcherTimer(); //MCU 모니터링 타이머용

        const byte STX = 0x02;
        const byte ETX = 0x03;

        #region 싱글톤 패턴 구현
        private static Mcu SingleTonObj = null;

        private Mcu()
        {
            moniBack.DoWork += new DoWorkEventHandler((object send, DoWorkEventArgs e) =>
            {
                //McuMonitoring(); //기존 모니터링(시리얼)
                CommSendQueue();//모드버스용 임시 큐
            });

            MonitoringTimer.Interval = TimeSpan.FromMilliseconds(150);    // ms
            MonitoringTimer.Tick += new EventHandler((object sender, EventArgs e) =>
            {
                if (moniBack.IsBusy == false)
                    moniBack.RunWorkerAsync();
            });
        }

        public static Mcu GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new Mcu();
            return SingleTonObj;
        }
        #endregion 싱글톤 패턴 구현
#if (modbusDefine==true)
        public string Connect(string portName, int borate)
        {
            string msg = McuComm.Connect(portName, borate);
            if (msg == "Connected!")
                IsConnected = true;

            return msg;
        }
        public bool Disconnect()
        {
            bool result = McuComm.Disconnect();
            if (result == true)
                IsConnected = false;

            return result;
        }

        // 모니터링 관련
        public void MonitorStart()
        {
            MonitoringTimer.Start();
        }
        public void MonitorStop()
        {
            MonitoringTimer.Stop();
        }

        //MCU모니터링용 타이머
        public void McuMonitoring()
        {
            byte[] receiveData = ChMonitoring();

            if (receiveData == null) { CommErrCount++; return; }
            if (receiveData[0] != STX || receiveData[receiveData.Length - 1] != ETX) { CommErrCount++; return; }
            if (receiveData[1] != 'M') { CommErrCount++; return; }

            List<byte> dataList = receiveData.ToList();
            dataList.RemoveRange(0,2);           // STX, Command 삭제
            dataList.RemoveAt(dataList.Count-1); // ETX 삭제

            Ch1Volt = double.Parse(Encoding.ASCII.GetString(dataList.GetRange(0, 6).ToArray())) * 0.1; dataList.RemoveRange(0, 6);  // CH1 전압 추출
            Ch1Curr = double.Parse(Encoding.ASCII.GetString(dataList.GetRange(0, 7).ToArray()));       dataList.RemoveRange(0, 7);  // CH1 전류 추출
            Ch2Volt = double.Parse(Encoding.ASCII.GetString(dataList.GetRange(0, 6).ToArray())) * 0.1; dataList.RemoveRange(0, 6);  // CH2 전압 추출
            Ch2Curr = double.Parse(Encoding.ASCII.GetString(dataList.GetRange(0, 7).ToArray()));       dataList.RemoveRange(0, 7);  // CH2 전류 추출
        }
        /**
        *  @brief MCU 모니터링
        *  @details 채널1,2의 전압,전류 센싱값을 요청
        *  
        *  @param
        *  
        *  @return byte[] MCU로부터의 응답
        *  
        *  @see 모니터링 프로토콜
        *   STX  |  Command  |  ETX
        *  ------|-----------|------
        *   0x02 | 0x4F('O') | 0x03
        */
        public byte[] ChMonitoring()
        {
            bool commFlag = McuComm.CommSend(new byte[] { 0x02, 0x4f, 0x03 }, out int code);
            if (!commFlag) { CommErrCount++; return null; }

            commFlag = McuComm.CommReceive(out byte[] receiveData, code);
            if (!commFlag) { CommErrCount++; return null; }

            Debug.WriteLine(Encoding.ASCII.GetString(receiveData));

            return receiveData;
        }
        /**
         *  @brief 채널 Ref 명령
         *  @details 채널에게 출력 명령
         *  
         *  @param char calType CAL 타입
         *  @param int chNum 채널번호
         *  @param double volt 설정 전압값
         *  @param double curr 설정 전류값
         *  
         *  @return
         *  
         *  @see 채널 출력 프로토콜
         *   STX  |  Command  |        CAL 타입       |        채널번호        | 전압 설정값(mV) | 전류 설정값(mA)  |  ETX
         *  ------|-----------|-----------------------|------------------------|-----------------|------------------|-------
         *   0x02 | 0x43('C') |  'V'or'I'(의미 없음)  |'0':ALL '1':CH1 '2':CH2 |     '00000'     |    '±0000000'    |  0x03
         */
        public void ChSet(int chNum, int volt, int curr)
        {
            List<byte> sendList = new List<byte>();

            sendList.Add(STX);
            sendList.Add(0x43);
            string makeCmd = 'V' + chNum.ToString() + volt.ToString("00000") + curr.ToString("+000000;-000000");
            sendList.AddRange(Encoding.ASCII.GetBytes(makeCmd));
            sendList.Add(ETX);

            bool commFlag = McuComm.CommSend(sendList.ToArray(), out _);
            if (!commFlag) { CommErrCount++; return; }
        }

        /**
         *  @brief CAL 수행 명령
         *  @details 채널에게 CAL 수행을 명령
         *  
         *  @param char calType CAL 타입
         *  @param int chNum 채널번호
         *  @param double volt 설정 전압값
         *  @param double curr 설정 전류값
         *  
         *  @return
         *  
         *  @see 채널 출력 프로토콜
         *   STX  |  Command  |  CAL 타입  |        채널번호        | CAL 설정값(mV/mA) | 전류 설정값(mA) |  ETX
         *  ------|-----------|------------|------------------------|-------------------|-----------------|-------
         *   0x02 | 0x52('R') |  'V'or'I'  |'0':ALL '1':CH1 '2':CH2 | '00000'/'±000000' |    '±0000000'    |  0x03
         */
        public void ChCal(char calType, int chNum, double calValue)
        {
            List<byte> sendList = new List<byte>();
            string makeCmd;

            sendList.Add(STX);
            sendList.Add(0x52);

            if(calType == 'V')
                makeCmd = calType.ToString() + chNum.ToString() + (calValue*10).ToString("00000");
            else
                makeCmd = calType.ToString() + chNum.ToString() + (calValue*10).ToString("+0000000;-0000000");

            sendList.AddRange(Encoding.ASCII.GetBytes(makeCmd));
            sendList.Add(ETX);

            bool commFlag = McuComm.CommSend(sendList.ToArray(), out _);
            if (!commFlag) { CommErrCount++; return; }
        }

        /**
         *  @brief CAL 취소
         *  @details 진행하고 있는 CAL을 취소
         *  
         *  @param
         *  
         *  @return
         *  
         *  @see CAL 정지 프로토콜
         *   STX  |  Command  |  ETX
         *  ------|-----------|------
         *   0x02 | 0x54('T') | 0x03
         */
        public void ChStop()
        {
            bool commFlag = McuComm.CommSend(new byte[] { 0x02, 0x54, 0x03 }, out _);
            if (!commFlag) { CommErrCount++; return; }
        }
        public void CommSendQueue()//플래그를 사용하는 임시 큐 함수(큐 보다는 일정 주기 반복 모니터링 하면서 다른 명령어 들어오면 그거 처리하기)
        {
            //            public int sendReadMonitoringFlag = 0; //모니터링 읽기
            //            public int sendWriteOutputStartFlag = 0; //실제 출력
            //            public int sendWriteOutputStopFlag = 0; //정지
            //            public int sendWriteDMMStartFlag = 0; //DMM 값 전송
            //            public int sendWriteCalPointSaveFlag = 0; //Cal 포인트 준비(1: 준비, 2: 실제저장)
            //            public int sendWriteCH1VoltSaveFlag = 0; //채널 1 전압 Cal 포인트 전송
            //            public int sendWriteCH2VoltSaveFlag = 0; //채널 2 전압 Cal 포인트 전송
            //            public int sendWriteCH1CurrSaveFlag = 0; //채널 1 전압 Cal 포인트 전송
            //            public int sendWriteCH2CurrSaveFlag = 0; //채널 2 전압 Cal 포인트 전송

            //출력
            if (sendWriteOutputStartFlag == 1)
            {
                sendWriteOutputStartFlag = 0;
                ChSet(outputBuffer.chNum, outputBuffer.volt, outputBuffer.curr);
                //ChSet(int chNum, int volt, int curr);
            }
            //정지
            else if (sendWriteOutputStopFlag == 1)
            {
                sendWriteOutputStopFlag = 0;
                ChStop();
                //ChStop();
            }
            //CAL
            else if (sendWriteDMMStartFlag == 1)
            {
                sendWriteDMMStartFlag = 0;
                ChCal(calBuffer.calType, calBuffer.chNum, calBuffer.dmm);
                //ChCal(char calType, int chNum, double calValue);
            }
            //모니터링
            else
            {
                byte[] receiveData = ChMonitoring();

                if (receiveData == null) { CommErrCount++; return; }
                if (receiveData[0] != STX || receiveData[receiveData.Length - 1] != ETX) { CommErrCount++; return; }
                if (receiveData[1] != 'M') { CommErrCount++; return; }

                List<byte> dataList = receiveData.ToList();
                dataList.RemoveRange(0, 2);           // STX, Command 삭제
                dataList.RemoveAt(dataList.Count - 1); // ETX 삭제

                Ch1Volt = double.Parse(Encoding.ASCII.GetString(dataList.GetRange(0, 6).ToArray())) * 0.1; dataList.RemoveRange(0, 6);  // CH1 전압 추출
                Ch1Curr = double.Parse(Encoding.ASCII.GetString(dataList.GetRange(0, 7).ToArray())); dataList.RemoveRange(0, 7);  // CH1 전류 추출
                Ch2Volt = double.Parse(Encoding.ASCII.GetString(dataList.GetRange(0, 6).ToArray())) * 0.1; dataList.RemoveRange(0, 6);  // CH2 전압 추출
                Ch2Curr = double.Parse(Encoding.ASCII.GetString(dataList.GetRange(0, 7).ToArray())); dataList.RemoveRange(0, 7);  // CH2 전류 추출
            }
        }
#else
        public void CommSendQueue()//플래그를 사용하는 임시 큐 함수(큐 보다는 일정 주기 반복 모니터링 하면서 다른 명령어 들어오면 그거 처리하기)
        {
            //            public int sendReadMonitoringFlag = 0; //모니터링 읽기
            //            public int sendWriteOutputStartFlag = 0; //실제 출력
            //            public int sendWriteOutputStopFlag = 0; //정지
            //            public int sendWriteDMMStartFlag = 0; //DMM 값 전송
            //            public int sendWriteCalPointSaveFlag = 0; //Cal 포인트 준비(1: 준비, 2: 실제저장)
            //            public int sendWriteCH1VoltSaveFlag = 0; //채널 1 전압 Cal 포인트 전송
            //            public int sendWriteCH2VoltSaveFlag = 0; //채널 2 전압 Cal 포인트 전송
            //            public int sendWriteCH1CurrSaveFlag = 0; //채널 1 전압 Cal 포인트 전송
            //            public int sendWriteCH2CurrSaveFlag = 0; //채널 2 전압 Cal 포인트 전송

            //출력
            if (sendWriteOutputStartFlag == 1)
            {
                sendWriteOutputStartFlag = 0;
                //ChSet(int chNum, int volt, int curr);
                ushort[] tempStream;
                UnionConv tempfloat = new UnionConv();
                tempStream = new ushort[9];//데이터 크기에 맞게 설정하지 않으면 에러발생

                if (outputBuffer.chNum == 1) //1번채널
                {
                    tempfloat.Float = outputBuffer.volt * 0.001f;
                    //1번채널 전압
                    tempStream[0] = tempfloat.Byte1;
                    tempStream[1] = tempfloat.Byte2;
                    //2번채널 전압
                    tempStream[2] = 0;
                    tempStream[3] = 0;
                    tempfloat.Float = outputBuffer.curr * 0.001f;
                    //1번채널 전류
                    tempStream[4] = tempfloat.Byte1;
                    tempStream[5] = tempfloat.Byte2;
                    //2번채널 전류
                    tempStream[6] = 0;
                    tempStream[7] = 0;
                }
                else if (outputBuffer.chNum == 2) //2번채널
                {
                    tempfloat.Float = outputBuffer.volt * 0.001f;
                    //1번채널 전압
                    tempStream[0] = 0;
                    tempStream[1] = 0;
                    //2번채널 전압
                    tempStream[2] = tempfloat.Byte1;
                    tempStream[3] = tempfloat.Byte2;
                    tempfloat.Float = outputBuffer.curr * 0.001f;
                    //1번채널 전류
                    tempStream[4] = 0;
                    tempStream[5] = 0;
                    //2번채널 전류
                    tempStream[6] = tempfloat.Byte1;
                    tempStream[7] = tempfloat.Byte2;
                }

                tempStream[8] = 1;//출력시작(0:대기, 1: 시작, 2: 정지)
                MdMaster.WriteMultipleRegisters(SLAVE_ID, REF_SET_ADDRESS, tempStream);//레지스터 주소 0x2000
            }
            //정지
            else if (sendWriteOutputStopFlag == 1)
            {
                //ChStop();
                sendWriteOutputStopFlag = 0;
                ushort[] tempStream = new ushort[9]; //데이터 크기에 맞게 설정하지 않으면 에러발생
                tempStream[0] = 0;
                tempStream[1] = 0;
                tempStream[2] = 0;
                tempStream[3] = 0;
                tempStream[4] = 0;
                tempStream[5] = 0;
                tempStream[6] = 0;
                tempStream[7] = 0;
                tempStream[8] = 2;//출력시작(0:대기, 1: 시작, 2: 정지)
                MdMaster.WriteMultipleRegisters(1, (ushort)0x2000, tempStream);//레지스터 주소 0x2000

            }
            //CAL
            else if (sendWriteDMMStartFlag == 1)
            {
                sendWriteDMMStartFlag = 0;
                //ChCal(calBuffer.calType, calBuffer.chNum, calBuffer.dmm);
                //ChCal(char calType, int chNum, double calValue);
                ushort[] tempStream = new ushort[3];//데이터 크기에 맞게 설정하지 않으면 에러발생;
                UnionConv tempfloat = new UnionConv();

                tempfloat.Float = (float)calBuffer.dmm * 0.001f;
                tempStream[0] = tempfloat.Byte1;
                tempStream[1] = tempfloat.Byte2;
                tempStream[2] = 0;

                if (calBuffer.calType == 'V') //전압
                {
                    if (calBuffer.chNum == 1) tempStream[2] = 1; //채널1
                    else if (calBuffer.chNum == 2) tempStream[2] = 2;//채널2
                }
                else //전류
                {
                    if (calBuffer.chNum == 1) tempStream[2] = 3;//채널1
                    else if (calBuffer.chNum == 2) tempStream[2] = 4;//채널2
                }

                MdMaster.WriteMultipleRegisters(SLAVE_ID, CAL_VALUE_ADDRESS, tempStream);//레지스터 주소 0x2010
            }
            //모니터링
            else
            {
                UnionConv tempfloat = new UnionConv();
                MdMasterBuffer = MdMaster.ReadHoldingRegisters(SLAVE_ID, MONITORING_ADDRESS, 12);

                //모니터링값 파싱
                tempfloat.Byte1 = MdMasterBuffer[0];
                tempfloat.Byte2 = MdMasterBuffer[1];
                Ch1Volt = Math.Round(tempfloat.Float * 1000.0f, 2);

                tempfloat.Byte1 = MdMasterBuffer[2];
                tempfloat.Byte2 = MdMasterBuffer[3];
                Ch2Volt = Math.Round(tempfloat.Float * 1000.0f, 2);

                tempfloat.Byte1 = MdMasterBuffer[4];
                tempfloat.Byte2 = MdMasterBuffer[5];
                Ch1Curr = Math.Round(tempfloat.Float * 1000.0f, 2);

                tempfloat.Byte1 = MdMasterBuffer[6];
                tempfloat.Byte2 = MdMasterBuffer[7];
                Ch2Curr = Math.Round(tempfloat.Float * 1000.0f, 2);

                IsRun = MdMasterBuffer[8] == 1 ? true : false;
                Ch1Fault = MdMasterBuffer[9];
                Ch2Fault = MdMasterBuffer[10];
                Version = MdMasterBuffer[11];

                OnLogSend($"[MCU Moni] Ch1 V : {Ch1Volt}, Ch2 V : {Ch2Volt}, Ch1 I : {Ch1Curr}, Ch2 I : {Ch2Curr}, IsRun : {IsRun}, Ch1 F : {Ch1Fault}, Ch2 F : {Ch2Fault}, Ver : {Version}",true);
            }
        }
        //Modbus적용 20.07.20
        public string Connect(string portName, int borate)
        {
            string msg = "";
            commModbusMCU = new SerialPort(portName, borate);
            commModbusMCU.ReadTimeout = 1000;
            commModbusMCU.WriteTimeout = 1000;
            try
            {
                commModbusMCU.Open();
                msg = "Connected!";
                MdMaster = ModbusSerialMaster.CreateRtu(commModbusMCU);
                //CalPointCheck('V', 1);
                if (commModbusMCU.IsOpen == true)
                    IsConnected = true;
            }
            catch (Exception ex)
            {
                msg = "Disconnected!" + ex.Message;
            }
            return msg;
        }
        public bool Disconnect()
        {
            commModbusMCU.Close();
            if (commModbusMCU.IsOpen == false)
                IsConnected = false;

            return IsConnected;
        }

        // 모니터링 관련
        public void MonitorStart()
        {
            MonitoringTimer.Start();
        }
        public void MonitorStop()
        {
            MonitoringTimer.Stop();
        }

        public void McuMonitoring()//최대 30ms 소요될듯(약 25ms이하로 통신중)
        {
            UnionConv tempfloat = new UnionConv();
            MdMasterBuffer = MdMaster.ReadHoldingRegisters(SLAVE_ID, MONITORING_ADDRESS, 12);

            //모니터링값 파싱
            tempfloat.Byte1 = MdMasterBuffer[0];
            tempfloat.Byte2 = MdMasterBuffer[1];
            Ch1Volt = Math.Round(tempfloat.Float * 1000.0f, 2);

            tempfloat.Byte1 = MdMasterBuffer[2];
            tempfloat.Byte2 = MdMasterBuffer[3];
            Ch2Volt = Math.Round(tempfloat.Float * 1000.0f, 2);

            tempfloat.Byte1 = MdMasterBuffer[4];
            tempfloat.Byte2 = MdMasterBuffer[5];
            Ch1Curr = Math.Round(tempfloat.Float * 1000.0f, 2);

            tempfloat.Byte1 = MdMasterBuffer[6];
            tempfloat.Byte2 = MdMasterBuffer[7];
            Ch2Curr = Math.Round(tempfloat.Float * 1000.0f, 2);

            IsRun = MdMasterBuffer[8] == 1 ? true : false;
            Ch1Fault = MdMasterBuffer[9];
            Ch2Fault = MdMasterBuffer[10];
            Version = MdMasterBuffer[11];

            OnLogSend($"[MCU Moni] Ch1 V : {Ch1Volt}, Ch2 V : {Ch2Volt}, Ch1 I : {Ch1Curr}, Ch2 I : {Ch2Curr}, IsRun : {IsRun}, Ch1 F : {Ch1Fault}, Ch2 F : {Ch2Fault}, Ver : {Version}", true);
        }

        public void ChSet(int chNum, int volt, int curr)
        {
#if true
            outputBuffer.chNum = chNum;
            outputBuffer.volt = volt;
            outputBuffer.curr = curr;
            sendWriteOutputStartFlag = 1;
#else
            ushort[] tempStream;
            UnionConv tempfloat = new UnionConv();
            tempStream = new ushort[9];//데이터 크기에 맞게 설정하지 않으면 에러발생

            if (chNum == 1) //1번채널
            {
                tempfloat.Float = volt * 0.001f;
                //1번채널 전압
                tempStream[0] = tempfloat.Byte1;
                tempStream[1] = tempfloat.Byte2;
                //2번채널 전압
                tempStream[2] = 0;
                tempStream[3] = 0;
                tempfloat.Float = curr * 0.001f;
                //1번채널 전류
                tempStream[4] = tempfloat.Byte1;
                tempStream[5] = tempfloat.Byte2;
                //2번채널 전류
                tempStream[6] = 0;
                tempStream[7] = 0;
            }
            else if (chNum == 2) //2번채널
            {
                tempfloat.Float = volt * 0.001f;
                //1번채널 전압
                tempStream[0] = 0;
                tempStream[1] = 0;
                //2번채널 전압
                tempStream[2] = tempfloat.Byte1;
                tempStream[3] = tempfloat.Byte2;
                tempfloat.Float = curr * 0.001f;
                //1번채널 전류
                tempStream[4] = 0;
                tempStream[5] = 0;
                //2번채널 전류
                tempStream[6] = tempfloat.Byte1;
                tempStream[7] = tempfloat.Byte2;
            }

            tempStream[8] = 1;//출력시작(0:대기, 1: 시작, 2: 정지)
            MdMaster.WriteMultipleRegisters(SLAVE_ID, REF_SET_ADDRESS, tempStream);//레지스터 주소 0x2000
#endif
            OnLogSend($"[MCU ChSet] Ch : {chNum}, Volt : {volt}, Curr : {curr}", false);
        }

        public void ChCal(char calType, int chNum, double calValue)
        {
#if true
            calBuffer.calType = calType;
            calBuffer.chNum = chNum;
            calBuffer.dmm = calValue;
            sendWriteDMMStartFlag = 1;
#else
            ushort[] tempStream = new ushort[3];//데이터 크기에 맞게 설정하지 않으면 에러발생;
            UnionConv tempfloat = new UnionConv();

            tempfloat.Float = (float)calValue * 0.001f;
            tempStream[0] = tempfloat.Byte1;
            tempStream[1] = tempfloat.Byte2;
            tempStream[2] = 0;

            if (calType == 'V') //전압
            {
                if (chNum == 1) tempStream[2] = 1; //채널1
                else if (chNum == 2) tempStream[2] = 2;//채널2
            }
            else //전류
            {
                if (chNum == 1) tempStream[2] = 3;//채널1
                else if (chNum == 2) tempStream[2] = 4;//채널2
            }

            MdMaster.WriteMultipleRegisters(SLAVE_ID, CAL_VALUE_ADDRESS, tempStream);//레지스터 주소 0x2010
#endif
            OnLogSend($"[MCU ChCal] Type : {calType}, Ch : {chNum}, DMM : {calValue}", false);
        }

        public void ChStop()
        {
#if true
            sendWriteOutputStopFlag = 1;
#else
            ushort[] tempStream = new ushort[9]; //데이터 크기에 맞게 설정하지 않으면 에러발생

            tempStream[0] = 0;
            tempStream[1] = 0;
            tempStream[2] = 0;
            tempStream[3] = 0;
            tempStream[4] = 0;
            tempStream[5] = 0;
            tempStream[6] = 0;
            tempStream[7] = 0;
            tempStream[8] = 2;//출력시작(0:대기, 1: 시작, 2: 정지)
            MdMaster.WriteMultipleRegisters(1, 0x2000, tempStream);//레지스터 주소 0x2000
#endif
            OnLogSend($"[MCU Stop]", false);
        }

        //Cal 포인트 확인함수
        public float[][] CalPointCheck(char calMode, int chNum)//Cal포인트 확인 구조체 필요?
        {
            UnionConv standardPoint = new UnionConv();
            UnionConv correctionPoint = new UnionConv();

            List<float[]> tempPointList = new List<float[]>();

            //현재 저장되어있는 개수 호출
            //temp = SendPort.ReadHoldingRegisters(slaveID, 8224, 4);
            //Buffer.BlockCopy(temp, 0, buffer, 34 * 2, 8);//(ushort)0x2020
            //Buffer.BlockCopy(MdMaster.ReadHoldingRegisters(SLAVE_ID, 0x2020, 4), 0, buffer, 34 * 2, 8);//(ushort)0x2020

            ushort[] tempBuffer = MdMaster.ReadHoldingRegisters(SLAVE_ID, (ushort)0x2020, 4);

            ushort ch1Voltcnt = tempBuffer[0];
            ushort ch2Voltcnt = tempBuffer[1];
            ushort ch1Currcnt = tempBuffer[2];
            ushort ch2Currcnt = tempBuffer[3];


            if (chNum == 1)
            {
                if (calMode == 'V')
                {
                    try //예외처리 필요
                    { 
                        tempBuffer = MdMaster.ReadHoldingRegisters(SLAVE_ID, 0x2100, (ushort)(ch1Voltcnt * 4));
                        for (int i = 0; i < ch1Voltcnt; i++)
                        {
                            standardPoint.Byte1 = tempBuffer[i * 4 + 0];
                            standardPoint.Byte2 = tempBuffer[i * 4 + 1];
                            correctionPoint.Byte1 = tempBuffer[i * 4 + 2];
                            correctionPoint.Byte2 = tempBuffer[i * 4 + 3];
                            standardPoint.Float = standardPoint.Float * 1000;
                            correctionPoint.Float = standardPoint.Float * 1000;
                            tempPointList.Add(new float[] { standardPoint.Float, correctionPoint.Float });
                        }
                    }
                    catch 
                    {
                        standardPoint.Float = 0;
                        correctionPoint.Float = 0;
                        tempPointList.Add(new float[] { standardPoint.Float, correctionPoint.Float });
                    }

                    //return tempPointList.ToArray();
                }
                else
                {
                    try //예외처리 필요
                    {
                        tempBuffer = MdMaster.ReadHoldingRegisters(SLAVE_ID, 0x2300, (ushort)(ch1Currcnt * 4));
                        for (int i = 0; i < ch1Currcnt; i++)
                        {
                            standardPoint.Byte1   = tempBuffer[i * 4 + 0];
                            standardPoint.Byte2   = tempBuffer[i * 4 + 1];
                            correctionPoint.Byte1 = tempBuffer[i * 4 + 2];
                            correctionPoint.Byte2 = tempBuffer[i * 4 + 3];
                            standardPoint.Float = standardPoint.Float * 1000;
                            correctionPoint.Float = standardPoint.Float * 1000;
                            tempPointList.Add(new float[] { standardPoint.Float, correctionPoint.Float });
                        }
                    }
                    catch
                    {
                        standardPoint.Float = 0;
                        correctionPoint.Float = 0;
                        tempPointList.Add(new float[] { standardPoint.Float, correctionPoint.Float });
                    }
                    //return tempPointList.ToArray();
                }
            }
            else
            {
                if (calMode == 'V')
                {
                    try //예외처리 필요
                    {
                        tempBuffer = MdMaster.ReadHoldingRegisters(SLAVE_ID, 0x3100, (ushort)(ch2Voltcnt * 4));
                        for (int i = 0; i < ch2Voltcnt; i++)
                        {
                            standardPoint.Byte1   = tempBuffer[i * 4 + 0];
                            standardPoint.Byte2   = tempBuffer[i * 4 + 1];
                            correctionPoint.Byte1 = tempBuffer[i * 4 + 2];
                            correctionPoint.Byte2 = tempBuffer[i * 4 + 3];
                            standardPoint.Float = standardPoint.Float * 1000;
                            correctionPoint.Float = standardPoint.Float * 1000;
                            tempPointList.Add(new float[] { standardPoint.Float, correctionPoint.Float });
                        }
                    }
                    catch
                    {
                        standardPoint.Float = 0;
                        correctionPoint.Float = 0;
                        tempPointList.Add(new float[] { standardPoint.Float, correctionPoint.Float });
                    }
                    //return tempPointList.ToArray();
                }
                else
                {
                    try //예외처리 필요
                    {
                        tempBuffer = MdMaster.ReadHoldingRegisters(SLAVE_ID, 0x3300, (ushort)(ch2Currcnt * 4));
                        for (int i = 0; i < ch2Currcnt; i++)
                        {
                            standardPoint.Byte1   = tempBuffer[i * 4 + 0];
                            standardPoint.Byte2   = tempBuffer[i * 4 + 1];
                            correctionPoint.Byte1 = tempBuffer[i * 4 + 2];
                            correctionPoint.Byte2 = tempBuffer[i * 4 + 3];
                            standardPoint.Float = standardPoint.Float * 1000;
                            correctionPoint.Float = standardPoint.Float * 1000;
                            tempPointList.Add(new float[] { standardPoint.Float, correctionPoint.Float });
                        }
                    }
                    catch
                    {
                        standardPoint.Float = 0;
                        correctionPoint.Float = 0;
                        tempPointList.Add(new float[] { standardPoint.Float, correctionPoint.Float });
                    }
                    //return tempPointList.ToArray();
                }
            }
            
            //countbuffer = SendPort.ReadHoldingRegisters(slaveID, 8448, (ushort)(ch1Voltcnt * 2));
            //if(ch1Voltcnt > 0)//채널1 전압
            //{
            //    //기준값
            //    //레지스터 주소 0x2100 채널1 전압 기준값 읽기
            //    //Buffer.BlockCopy(MdMaster.ReadHoldingRegisters(SLAVE_ID, 0x2100, (ushort)(ch1Voltcnt * 2)), 0, buffer, 40 * 2, (ushort)(ch1Voltcnt * 2) * 2);
            //    tempBuffer = MdMaster.ReadHoldingRegisters(SLAVE_ID, 0x2100, (ushort)(ch1Voltcnt * 2));
            //    //보정값
            //    //레지스터 주소 0x2200 채널1 전압 보정값 읽기
            //    Buffer.BlockCopy(MdMaster.ReadHoldingRegisters(SLAVE_ID, 0x2200, (ushort)(ch1Voltcnt * 2)), 0, buffer, 60 * 2, (ushort)(ch1Voltcnt * 2) * 2);
            //}
            //if(ch2Voltcnt > 0)//채널2 전압
            //{
            //    //기준값
            //    //레지스터 주소 0x3100 채널2 전압 기준값 읽기
            //    Buffer.BlockCopy(MdMaster.ReadHoldingRegisters(SLAVE_ID, 0x3100, (ushort)(ch2Voltcnt * 2)), 0, buffer, 160 * 2, (ushort)(ch2Voltcnt * 2) * 2);
            //    //보정값
            //    //레지스터 주소 0x3200 채널2 전압 보정값 읽기
            //    Buffer.BlockCopy(MdMaster.ReadHoldingRegisters(SLAVE_ID, 0x3200, (ushort)(ch2Voltcnt * 2)), 0, buffer, 180 * 2, (ushort)(ch2Voltcnt * 2) * 2);
            //}

            OnLogSend($"[MCU CalPoint Check]", false);
            return tempPointList.ToArray();
        }

        public void CalPointSave(char calMode, int chNum, float[][] calPointList)
        {
            UnionConv standardPoint = new UnionConv();
            UnionConv correctionPoint = new UnionConv();

            MdMaster.WriteMultipleRegisters(SLAVE_ID, 0x2020, new ushort[] { 0, 0, 0, 0, 5 });//레지스터 주소 0x2020 Cal 개수, 명령 쓰기

            List<ushort> dataStream = new List<ushort>();
            foreach (var tempData in calPointList)
            {
                standardPoint.Float = tempData[0];
                correctionPoint.Float = tempData[1];

                dataStream.Add(standardPoint.Byte1);
                dataStream.Add(standardPoint.Byte2);
                dataStream.Add(correctionPoint.Byte1);
                dataStream.Add(correctionPoint.Byte2);
            }

            if(chNum == 1)
            {
                if(calMode == 'V')
                {
                    MdMaster.WriteMultipleRegisters(SLAVE_ID, 0x2100, dataStream.ToArray());
                    MdMaster.WriteMultipleRegisters(SLAVE_ID, 0x2020, new ushort[] { (ushort)calPointList.Length, 0, 0, 0, 1 });
                }
                else
                {
                    MdMaster.WriteMultipleRegisters(SLAVE_ID, 0x2300, dataStream.ToArray());
                    MdMaster.WriteMultipleRegisters(SLAVE_ID, 0x2020, new ushort[] { 0, 0, (ushort)calPointList.Length, 0, 3 });
                }
            }
            else
            {
                if (calMode == 'V')
                {
                    MdMaster.WriteMultipleRegisters(SLAVE_ID, 0x3100, dataStream.ToArray());
                    MdMaster.WriteMultipleRegisters(SLAVE_ID, 0x2020, new ushort[] { 0, (ushort)calPointList.Length, 0, 0, 2 });
                }
                else
                {
                    MdMaster.WriteMultipleRegisters(SLAVE_ID, 0x3300, dataStream.ToArray());
                    MdMaster.WriteMultipleRegisters(SLAVE_ID, 0x2020, new ushort[] { 0, 0, 0, (ushort)calPointList.Length, 4 });
                }
            }

            OnLogSend($"[MCU CalPoint Send]");
        }

        ////Cal 포인트 저장함수(준비 - 저장 순서)
        //public void CalPointSave(ModbusSerialMaster SendPort, byte slaveID, int selectNum, float[,] CalPointArray, int voltCurrCount)
        //{
        //    ushort[] tempStream;
        //    ushort[] tempStream2;
        //    UnionConv tempfloat = new UnionConv();
        //    //Cal 포인트 저장 준비
        //    tempStream = new ushort[5];
        //    for (int i = 0; i < 4; i++) tempStream[i] = 0;
        //    tempStream[4] = 5;//저장 준비
        //    SendPort.WriteMultipleRegisters(slaveID, 0x2020, tempStream);//레지스터 주소 0x2020 Cal 개수, 명령 쓰기

        //    //정해진 Cal포인트 개수만큼 데이터배열만들기
        //    tempStream = new ushort[(voltCurrCount * 2)];//기준값
        //    tempStream2 = new ushort[(voltCurrCount * 2)];//보정값
        //    //Cal 포인트 개수만큼 데이터 저장
        //    for (int i = 0; i < voltCurrCount; i++)
        //    {
        //        tempfloat.Float = CalPointArray[i, 0];//기준값
        //        tempStream[(i * 2)] = tempfloat.Byte1;
        //        tempStream[(i * 2) + 1] = tempfloat.Byte2;

        //        tempfloat.Float = CalPointArray[i, 1];//보정값
        //        tempStream2[(i * 2)] = tempfloat.Byte1;
        //        tempStream2[(i * 2) + 1] = tempfloat.Byte2;
        //    }

        //    //Cal 포인트를 레지스터로 쓰기
        //    switch (selectNum)
        //    {
        //        case 1: //채널1 전압 저장
        //            //정해진 Cal포인트 기준값 저장
        //            SendPort.WriteMultipleRegisters(slaveID, (ushort)0x2100, tempStream);//레지스터 주소 0x2100 채널1 전압 기준값 쓰기

        //            //정해진 Cal포인트 보정값 저장
        //            SendPort.WriteMultipleRegisters(slaveID, (ushort)0x2200, tempStream2);//레지스터 주소 0x2200 채널1 전압 보정값 쓰기
        //            break;
        //        case 2: //채널2 전압 저장
        //            //정해진 Cal포인트 기준값 저장
        //            SendPort.WriteMultipleRegisters(slaveID, (ushort)0x3100, tempStream);//레지스터 주소 0x3100 채널2 전압 기준값 쓰기

        //            //정해진 Cal포인트 보정값 저장
        //            SendPort.WriteMultipleRegisters(slaveID, (ushort)0x3200, tempStream2);//레지스터 주소 0x3200 채널2 전압 보정값 쓰기
        //            break;
        //        case 3: //채널1 전류 저장
        //            //정해진 Cal포인트 기준값 저장
        //            SendPort.WriteMultipleRegisters(slaveID, (ushort)0x2300, tempStream);//레지스터 주소 0x2300 채널1 전류 기준값 쓰기

        //            //정해진 Cal포인트 보정값 저장
        //            SendPort.WriteMultipleRegisters(slaveID, (ushort)0x2400, tempStream2);//레지스터 주소 0x2400 채널1 전류 보정값 쓰기
        //            break;
        //        case 4: //채널2 전류 저장
        //            //정해진 Cal포인트 기준값 저장
        //            SendPort.WriteMultipleRegisters(slaveID, (ushort)0x3300, tempStream);//레지스터 주소 0x3300 채널2 전류 기준값 쓰기

        //            //정해진 Cal포인트 보정값 저장
        //            SendPort.WriteMultipleRegisters(slaveID, (ushort)0x3400, tempStream2);//레지스터 주소 0x3400 채널2 전류 보정값 쓰기
        //            break;
        //        default: //그 외 명령은 0으로 전송
        //            selectNum = 0;
        //            break;
        //    }

        //    //정해진 Cal포인트 저장명령 전송
        //    tempStream = new ushort[5];
        //    for (int i = 0; i < 4; i++) tempStream[i] = 0;
        //    tempStream[selectNum - 1] = (ushort)voltCurrCount;
        //    tempStream[4] = (ushort)selectNum;//저장 준비
        //    SendPort.WriteMultipleRegisters(slaveID, (ushort)0x2020, tempStream);//레지스터 주소 0x2020
        //}
#endif

        /**
         *  @brief 로그 텍스트 메세지 전송
         *  @details 로그 텍스트 메세지를 전송
         *  
         *  @param string text 로그 텍스트
         *  @param bool isMonitoring 모니터링 텍스트 여부
         *  
         *  @return
         */
        private void OnLogSend(string text, bool isMonitoring)
        {
            LogTextMessage Message = new LogTextMessage
            {
                LogText = text,
                IsMonitoring = isMonitoring
            };

            Messenger.Default.Send(Message);
        }

    }
}
