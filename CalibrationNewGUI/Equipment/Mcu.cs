using CalibrationNewGUI.Model;
using J_Project.Communication.CommFlags;
using J_Project.Communication.CommModule;
using Modbus.Device;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
        public struct FloatData
        {
            [FieldOffset(0)]
            public float floattemp;
            [FieldOffset(0)]
            public ushort floatByte1;
            [FieldOffset(2)]
            public ushort floatByte2;
            //[FieldOffset(0)]
            //public fixed ushort floatByte[2];
        }
        ushort[] buffer = new ushort[300];
        /////////////////////////////////////////////////////////////////////
        public double Ch1Volt { get; set; }
        public double Ch1Curr { get; set; }
        public double Ch2Volt { get; set; }
        public double Ch2Curr { get; set; }
        
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
                McuMonitoring();
            });

            MonitoringTimer.Interval = TimeSpan.FromMilliseconds(200);    // ms
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
#if (modbus == true)
//#if(modbus == false)
        public string Connect(string portName, int borate)
        {
            string msg = McuComm.Connect(portName, borate);
            if (msg == "Connected!")
                IsConnected = true;

            return msg;
        }
#else
        //Modbus적용 20.07.20
        public string Connect(string portName, int borate)
        {
            string msg = "";
            commModbusMCU = new SerialPort(portName, borate);
            commModbusMCU.ReadTimeout = 500;
            commModbusMCU.WriteTimeout = 500;
            try
            {
                commModbusMCU.Open();
                msg = "Connected!";
                MdMaster = ModbusSerialMaster.CreateRtu(commModbusMCU);
                if (msg == "Connected!")
                    IsConnected = true;
            }
            catch(Exception ex)
            {
                msg = "Disconnected!";
            }
            return msg;
        }
#endif
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
#if (modbusDefine == true)
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
            Ch1Curr = double.Parse(Encoding.ASCII.GetString(dataList.GetRange(0, 7).ToArray())); dataList.RemoveRange(0, 7);  // CH1 전류 추출
            Ch2Volt = double.Parse(Encoding.ASCII.GetString(dataList.GetRange(0, 6).ToArray())) * 0.1; dataList.RemoveRange(0, 6);  // CH2 전압 추출
            Ch2Curr = double.Parse(Encoding.ASCII.GetString(dataList.GetRange(0, 7).ToArray())); dataList.RemoveRange(0, 7);  // CH2 전류 추출
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
         *   STX  |  Command  |        CAL 타입       |        채널번호        | 전압 설정값(mV) | 전류 설정값(mA) |  ETX
         *  ------|-----------|-----------------------|------------------------|-----------------|-----------------|-------
         *   0x02 | 0x43('C') |  'V'or'I'(의미 없음)  |'0':ALL '1':CH1 '2':CH2 |     '00000'     |    '±000000'    |  0x03
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
         *   0x02 | 0x52('R') |  'V'or'I'  |'0':ALL '1':CH1 '2':CH2 | '00000'/'±000000' |    '±000000'    |  0x03
         */
        public void ChCal(char calType, int chNum, double calValue)
        {
            List<byte> sendList = new List<byte>();
            string makeCmd;

            sendList.Add(STX);
            sendList.Add(0x52);

            if(calType == 'V')
                makeCmd = calType.ToString() + chNum.ToString() + calValue.ToString("00000");
            else
                makeCmd = calType.ToString() + chNum.ToString() + calValue.ToString("+000000;-000000");

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
#else
        public void McuMonitoring()
        {
            FloatData tempfloat = new FloatData();
            MdMasterBuffer = MdMaster.ReadHoldingRegisters(1, (ushort)0x1200, 12);//0x1200 읽기 시작
            //모니터링값 파싱
            tempfloat.floatByte1 = MdMasterBuffer[0];
            tempfloat.floatByte2 = MdMasterBuffer[1];
            Ch1Volt = Math.Round(tempfloat.floattemp * 1000.0f,2);
            tempfloat.floatByte1 = MdMasterBuffer[2];
            tempfloat.floatByte2 = MdMasterBuffer[3];
            Ch2Volt = Math.Round(tempfloat.floattemp * 1000.0f, 2);
            tempfloat.floatByte1 = MdMasterBuffer[4];
            tempfloat.floatByte2 = MdMasterBuffer[5];
            Ch1Curr = Math.Round(tempfloat.floattemp * 1000.0f, 2);
            tempfloat.floatByte1 = MdMasterBuffer[6];
            tempfloat.floatByte2 = MdMasterBuffer[7];
            Ch2Curr = Math.Round(tempfloat.floattemp * 1000.0f, 2);
            //AllSetData.runMode = MdMasterBuffer[8]; //출력상태
            //AllSetData.faultCH1 = MdMasterBuffer[9];//ch1 fault
            //AllSetData.faultCH2 = MdMasterBuffer[10];//ch2 fault
            //AllSetData.MCUVersion = MdMasterBuffer[11].ToString();//MCU 펌웨어 버전
        }
        public void ChSet(int chNum, int volt, int curr)
        {
            ushort[] tempStream;
            FloatData tempfloat = new FloatData();
            tempStream = new ushort[9];//데이터 크기에 맞게 설정하지 않으면 에러발생

            if (chNum == 1) //1번채널
            {
                tempfloat.floattemp = (float)volt * 0.001f;
                //1번채널 전압
                tempStream[0] = tempfloat.floatByte1;
                tempStream[1] = tempfloat.floatByte2;
                //2번채널 전압
                tempStream[2] = 0;
                tempStream[3] = 0;
                tempfloat.floattemp = (float)curr * 0.001f;
                //1번채널 전류
                tempStream[4] = tempfloat.floatByte1;
                tempStream[5] = tempfloat.floatByte2;
                //2번채널 전류
                tempStream[6] = 0;
                tempStream[7] = 0;
            }
            else if (chNum == 2) //2번채널
            {
                tempfloat.floattemp = (float)volt * 0.001f;
                //1번채널 전압
                tempStream[0] = 0;
                tempStream[1] = 0;
                //2번채널 전압
                tempStream[2] = tempfloat.floatByte1;
                tempStream[3] = tempfloat.floatByte2;
                tempfloat.floattemp = (float)curr * 0.001f;
                //1번채널 전류
                tempStream[4] = 0;
                tempStream[5] = 0;
                //2번채널 전류
                tempStream[6] = tempfloat.floatByte1;
                tempStream[7] = tempfloat.floatByte2;
            }
            tempStream[8] = 1;//출력시작(0:대기, 1: 시작, 2: 정지)
            MdMaster.WriteMultipleRegisters(1, (ushort)0x2000, tempStream);//레지스터 주소 0x2000
        }
        public void ChCal(char calType, int chNum, double calValue)
        {
            ushort[] tempStream;
            FloatData tempfloat = new FloatData();
            tempStream = new ushort[3];//데이터 크기에 맞게 설정하지 않으면 에러발생
            tempfloat.floattemp = ((float)calValue * 0.001f);
            tempStream[0] = tempfloat.floatByte1;
            tempStream[1] = tempfloat.floatByte2;
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
            MdMaster.WriteMultipleRegisters(1, (ushort)0x2010, tempStream);//레지스터 주소 0x2010
        }
        public void ChStop()
        {
            ushort[] tempStream;
            tempStream = new ushort[9];//데이터 크기에 맞게 설정하지 않으면 에러발생
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
#endif


    }
}
