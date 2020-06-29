﻿using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI
{
    [ImplementPropertyChanged]
    public class SettingData
    {
        /// <summary>
        /// GUI에서 사용되는 모든 세팅 정보
        /// </summary>
        ///CAL, MEA 버튼 명령어들(자동CAL 출력, 자동CAL종료, 수동CAL 출력, 수동CAL 실시, 수동출력정지,  실측시작, 실측정지)
        public int AutoCalOutStartFlag = 0;
        public int AutoCalOutEndFlag = 0;
        public int CalOutStartFlag = 0;
        public int CalOutRealStartFlag = 0;
        public int CalOutEndFlag = 0;
        public int MeaOutStartFlag = 0;
        public int MeaOutEndFlag = 0;

        public int[,] CalPointArray { get; set; }//CAL 포인트용 배열
        public int[] ActCalPointArray { get; set; }//수동 CAL 포인트용 배열
        public int[,] MeaPointArray { get; set; }//실측 포인트용 배열

        ///메인화면 출력 변수
        public string GUIVersion { get; set; } //GUI 버전
        public string SerialNumber { get; set; } //시리얼 번호
        public int AllConnectFlag { get; set; } //전체 연결상태 확인용
        public int MCUConnectFlag { get; set; } //MCU 연결상태 확인용
        public int DMMConnectFlag { get; set; } //DMM 연결상태 확인용
        public float DMMOutputVolt { get; set; } //DMM 실측값(받아오는값)
        public float CH1OutputVolt { get; set; } //채널1 출력전압 모니터링값
        public float CH1OutputCurr { get; set; } //채널1 출력전류 모니터링값
        public float CH2OutputVolt { get; set; } //채널2 출력전압 모니터링값
        public float CH2OutputCurr { get; set; } //채널2 출력전류 모니터링값
        public int MeaCount { get; set; }  //오토세이브를 사용하여 실측 측정 횟수
        public int CalCount { get; set; }  //오토세이브를 사용하여 Cal 측정 횟수
        public string CHSelectString { get; set; }  //현재 선택된 전압, 전류, 채널 상태 출력용
        public int ChannelSelect { get; set; } //채널 선택하기(1: 1CH, 2: 2CH, 3: DUAL MODE)
        public int VoltCurrSelect { get; set; } //전압, 전류 선택하기(0: 전압, 1: 전류)
        public int ActiveInputVolt { get; set; } //수동조작 입력 전압
        public int ActiveInputCurr { get; set; } //수동조작 입력 전류
        ///통신 세팅
        public string MCUVersion { get; set; } //펌웨어 버전
        public string MCUPortName { get; set; }
        public int MCUBorate { get; set; }
        public int MCUDataBit { get; set; }
        public string MCUParity { get; set; }
        public int MCUStopBit { get; set; }
        public string MCUFlowCtrl { get; set; }
        public string DMMModel { get; set; } //DMM 모델 이름
        public string DMMPortName { get; set; }
        public int DMMBorate { get; set; }
        public int DMMDataBit { get; set; }
        public string DMMParity { get; set; }
        public int DMMStopBit { get; set; }
        public string DMMFlowCtrl { get; set; }
        public int DMMOffsetUseFlag { get; set; } //DMM offset 사용여부(0: 사용안함, 1: 사용)
        ///션트&전압미터 세팅
        public int ShuntStandardCurr { get; set; } //기준 전류(mA)
        public int ShuntStandardReg { get; set; } //기준 저항(uOhm)
        public int ShuntStandardCurr2 { get; set; } //보정 전류 값(mA)
        public int ShuntNewCurr { get; set; } //새로 연결하는 션트 기준 전류(mA)
        public int ShuntNewReg { get; set; } //새로 연결하는 션트 기준 저항(uOhm)
        public int ShuntNewCurr2 { get; set; } //새로 연결하는 션트 기준 보정 전류값(mA)
        public int StandardVoltMeter { get; set; } //기준 전압미터
        public int NewVoltMeter { get; set; } //보정 전압미터
        ///오토세이브
        public int AutoSaveFlag { get; set; } //오토세이브 기능 ON/OFF
        public string SaveFilePrixNum { get; set; } //오토 세이브할때 접두사
        ///자동 CAL&MEA 세팅
        public int CalErrRange { get; set; } //CAL 통합 에러 오차값
        public int MeaErrRange { get; set; } //MEA 통합 에러 오차값
        public int CalErrRangeVolt { get; set; } //CAL 전압 에러 오차값(mV)
        public int CalErrRangeCurr { get; set; } //CAL 전류 에러 오차값(mA)
        public int CalErrRetryCnt { get; set; } //CAL 에러 재측정 횟수
        public int CalErrDelayTime { get; set; } //CAL 측정 딜레이 시간(ms)
        public int MeaErrRangeVolt { get; set; } //실측 전압 에러 오차값(mV)
        public int MeaErrRangeCurr { get; set; } //실측 전류 에러 오차값(mA)
        public int MeaErrRetryCnt { get; set; } //실측 에러 재측정 횟수
        public int MeaErrDelayTime { get; set; } //실측 측정 딜레이 시간(ms)
        ///기타세팅
        public long InputRangeVoltMin { get; set; } //입력 전압 범위(mV) 최소값
        public long InputRangeVoltMax { get; set; } //입력 전압 범위(mV) 최대값
        public long InputRangeCurrentMin { get; set; } //입력 전류 범위(mA) 최소값
        public long InputRangeCurrentMax { get; set; } //입력 전류 범위(mA) 최대값
        public string Language { get; set; } //현재 언어 설정값

        #region 싱글톤 패턴 구현
        private static SettingData SingleTonObj = null;

        private SettingData()
        {
            //임시로 세팅데이터 고정시키기 -> 세팅 페이지 만들면 변경할것
            GUIVersion = "200626";
            AllConnectFlag = 0;
            MCUConnectFlag = 0;
            DMMConnectFlag = 0;
            ChannelSelect = 1;//채널 선택하기(1: 1CH, 2: 2CH, 3: DUAL MODE)
            VoltCurrSelect = 0;//전압,전류 선택
            //채널표시를 출력
            if(VoltCurrSelect == 0) CHSelectString = "Volt";
            else if(VoltCurrSelect == 1) CHSelectString = "Curr";
            if (ChannelSelect == 1) CHSelectString = "CH1 "+ CHSelectString;
            else if (ChannelSelect == 2) CHSelectString = "CH2 " + CHSelectString;
            else if (ChannelSelect == 3) CHSelectString = "DUAL " + CHSelectString;
            ActiveInputVolt = 2700; //수동조작 입력 전압
            ActiveInputCurr = 1000; //수동조작 입력 전류
            DMMOutputVolt = 0; //DMM 실측값 저장용
            CH1OutputVolt = 0; //채널1 출력 전압 모니터링
            CH1OutputCurr = 0; //채널1 출력 전류 모니터링
            CH2OutputVolt = 0; //채널2 출력 전압 모니터링
            CH2OutputCurr = 0; //채널2 출력 전류 모니터링
            ///통신 세팅
            MCUVersion = ""; //펌웨어 버전
            MCUPortName = "";
            MCUBorate = 57600;
            MCUDataBit = 8;
            MCUParity = "NONE";
            MCUStopBit = 1;
            MCUFlowCtrl = "NONE";
            DMMModel = "34450A"; //DMM 모델 이름
            DMMPortName = "";
            DMMBorate = 9600;
            DMMDataBit = 8;
            DMMParity = "NONE";
            DMMStopBit = 1;
            DMMFlowCtrl = "NONE";
            DMMOffsetUseFlag = 0;//DMM offset 사용여부(0: 사용안함, 1: 사용)
            ///션트&전압미터 세팅
            ShuntStandardCurr = 50000;//기준 전류(mA)
            ShuntStandardReg = 0;//기준 저항(uOhm)
            ShuntStandardCurr2 = 50000;//보정 전류 값(mA)
            ShuntNewCurr = 50000; //새로 연결하는 션트 기준 전류(mA)
            ShuntNewReg = 0; //새로 연결하는 션트 기준 저항(uOhm)
            ShuntNewCurr2 = 50000;//새로 연결하는 션트 기준 보정 전류값(mA)
            StandardVoltMeter = 4200;//기준 전압미터
            NewVoltMeter = 4200;//보정 전압미터
            ///오토세이브
            MeaCount = 0;  //오토세이브를 사용하여 실측 측정 횟수
            CalCount = 0;  //오토세이브를 사용하여 Cal 측정 횟수
            AutoSaveFlag = 0;//오토세이브 기능 ON/OFF
            SaveFilePrixNum = "1"; //오토 세이브할때 접두사
            ///CAL&MEA 세팅
            CalErrRangeVolt = 2;//CAL 전압 에러 오차값(mV)
            CalErrRangeCurr = 5;//CAL 전류 에러 오차값(mA)
            CalErrRetryCnt = 0; //CAL 에러 재측정 횟수
            CalErrDelayTime = 2000; //CAL 측정 딜레이 시간(ms)
            MeaErrRangeVolt = 2; //실측 전압 에러 오차값(mV)
            MeaErrRangeCurr = 5; //실측 전류 에러 오차값(mA)
            MeaErrRetryCnt = 0; //실측 에러 재측정 횟수
            MeaErrDelayTime = 2000;//실측 측정 딜레이 시간(ms)
            CalErrRange = CalErrRangeVolt; //CAL 통합 에러 오차값
            MeaErrRange = MeaErrRangeVolt; //MEA 통합 에러 오차값
            ///기타세팅
            InputRangeVoltMin = 2700;//입력 전압 범위(mV) 최소값
            InputRangeVoltMax = 4200;//입력 전압 범위(mV) 최대값
            InputRangeCurrentMin = -40000; //입력 전류 범위(mA) 최소값
            InputRangeCurrentMax = 40000; //입력 전류 범위(mA) 최대값
            Language = "KOR";//현재 언어 설정값
        }

        public static SettingData GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new SettingData();
            return SingleTonObj;
        }
        #endregion 싱글톤 패턴 구현
    }
}
