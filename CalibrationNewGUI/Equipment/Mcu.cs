using CalibrationNewGUI.Model;
using J_Project.Communication.CommFlags;
using J_Project.Communication.CommModule;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CalibrationNewGUI.Equipment
{
    [ImplementPropertyChanged]
    public class Mcu
    {
        public double Ch1Volt { get; private set; }
        public double Ch1Curr { get; private set; }
        public double Ch2Volt { get; private set; }
        public double Ch2Curr { get; private set; }

        public bool IsConnected { get; set; } = false;
        public int CommErrCount = 0;

        QueueComm McuComm = new QueueComm("byte[]");
        DispatcherTimer MonitoringTimer = new DispatcherTimer(); //MCU 모니터링 타이머용

        const byte STX = 0x02;
        const byte ETX = 0x03;

        #region 싱글톤 패턴 구현
        private static Mcu SingleTonObj = null;

        private Mcu()
        {
            MonitoringTimer.Interval = TimeSpan.FromMilliseconds(200);    // ms
            MonitoringTimer.Tick += McuMonitoring;
        }

        public static Mcu GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new Mcu();
            return SingleTonObj;
        }
        #endregion 싱글톤 패턴 구현

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
        public void McuMonitoring(object sender, EventArgs e)
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
    }
}
