using CalibrationNewGUI.Equipment;
using CalibrationNewGUI.Model;
using CalibrationNewGUI.ViewModel.Func.EventArgsClass;
using J_Project.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CalibrationNewGUI.ViewModel.Func
{
    enum CalSeq
    {
        REF_SET,
        DELAY1,
        REAL_VALUE_SEND,
        DELAY2,
        END_CAL,
        DELAY3,
    }
    enum MeaSeq
    {
        REF_SET,
        DELAY1,
        OUT_CHECK,
        OUT_STOP,
        DELAY2,
    }

    public class Calibration
    {
        CalMeasureInfo calInfo = CalMeasureInfo.GetObj();
        Mcu mcu = Mcu.GetObj();
        Dmm dmm = Dmm.GetObj();

        bool IsFullRun;
        char CalType;
        int ChNum;
        int errRate;
        object[][] CalPointList;
        object[][] MeaPointList;

        DispatcherTimer calTimer = new DispatcherTimer();
        DispatcherTimer meaTimer = new DispatcherTimer();

        public event EventHandler<CalMonitorArgs> CalMonitor;
        public event EventHandler<CalMonitorArgs> MeaMonitor;

        public Calibration()
        {
            calTimer.Interval = TimeSpan.FromMilliseconds(50);    // ms
            calTimer.Tick += CalTimer_Tick;

            meaTimer.Interval = TimeSpan.FromMilliseconds(50);    // ms
            meaTimer.Tick += MeaTimer_Tick;
        }

        public void CalStart()
        {
            SeqStepNum = 0;
            PointIndex = 0;

            calTimer.Start();
        }

        public void CalStop()
        {
            mcu.ChStop();
            calTimer.Stop();
        }

        public void MeaStart()
        {
            SeqStepNum = 0;
            PointIndex = 0;

            meaTimer.Start();
        }

        public void MeaStop()
        {
            mcu.ChStop();
            meaTimer.Stop();
        }

        public void AutoCalPointSet(char calType, int chNum, object[][] calPointList, object[][] meaPointList, bool isFullRun)
        {
            IsFullRun = isFullRun;

            CalType = calType;
            ChNum = chNum;
            CalPointList = calPointList;
            MeaPointList = meaPointList;
            errRate = calType == 'V' ? calInfo.CalErrRangeVolt : calInfo.CalErrRangeCurr;
        }

        int SeqStepNum = 0;
        int PointIndex = 0;
        private void CalTimer_Tick(object sender, EventArgs e)
        {
            if (CalPointList.Length <= PointIndex)
            {
                calTimer.Stop();

                if (IsFullRun)
                    MeaStart();
                return;
            }

            if (CalSequence(SeqStepNum++, PointIndex))
            {
                PointIndex++;
                SeqStepNum = 0;
            }
        }

        private bool CalSequence(int stepNum, int pointIndex)
        {
            bool isCalEnd = true;
            CalSeq stepName = (CalSeq)stepNum;

            int voltPoint = int.Parse(CalPointList[pointIndex][1].ToString());
            int currPoint = int.Parse(CalPointList[pointIndex][2].ToString());

            switch (stepName)
            {
                case CalSeq.REF_SET:
                    mcu.ChSet(ChNum, voltPoint, currPoint);
                    OnCalMonitor(new CalMonitorArgs(PointIndex));
                    isCalEnd = false;
                    break;

                case CalSeq.DELAY1:
                    Utill.Delay(calInfo.CalDelayTime * 0.001);
                    isCalEnd = false;
                    break;

                case CalSeq.REAL_VALUE_SEND:
                    for(int i = 0; i < calInfo.CalErrRetryCnt; i++)
                    {
                        if (CalType == 'V')
                        {
                            if (Math.Abs(dmm.SensingData - voltPoint) > errRate)
                            {
                                mcu.ChCal(CalType, ChNum, dmm.SensingData * 10);
                                OnCalMonitor(new CalMonitorArgs(PointIndex));
                            }
                            else
                            {
                                OnCalMonitor(new CalMonitorArgs(PointIndex));
                                break;
                            }
                        }
                        else
                        {
                            if (Math.Abs(dmm.SensingData - currPoint) > errRate)
                            {
                                mcu.ChCal(CalType, ChNum, dmm.SensingData * 10);
                                OnCalMonitor(new CalMonitorArgs(PointIndex));
                            }
                            else
                            {
                                OnCalMonitor(new CalMonitorArgs(PointIndex));
                                break;
                            }
                        }
                        Utill.Delay(calInfo.CalDelayTime * 0.001);
                    }
                    isCalEnd = false;
                    break;

                case CalSeq.DELAY2:
                    Utill.Delay(calInfo.CalDelayTime * 0.001);
                    isCalEnd = false;
                    break;

                case CalSeq.END_CAL:
                    mcu.ChStop();
                    isCalEnd = false;
                    break;

                case CalSeq.DELAY3:
                    Utill.Delay(calInfo.CalDelayTime * 0.001);
                    isCalEnd = true;
                    break;
            }

            return isCalEnd;
        }

        int MeaRetryCnt = 0;
        private void MeaTimer_Tick(object sender, EventArgs e)
        {
            if (MeaPointList.Length <= PointIndex)
            {
                meaTimer.Stop();
                return;
            }

            // 재시도 횟수를 초과했을 경우 다음 포인트로 이동
            if(MeaRetryCnt > calInfo.MeaErrRetryCnt)
            {
                PointIndex++;
                SeqStepNum = 0;
                MeaRetryCnt = 0;
            }

            // 실측 시퀀스 실행 후 결과 판단
            if (MeaSequence(SeqStepNum, PointIndex, out bool IsSuccess))
            {
                PointIndex++;
                SeqStepNum = 0;
                MeaRetryCnt = 0;
            }
            else
            {
                if(SeqStepNum == (int)MeaSeq.OUT_CHECK)
                {
                    if (!IsSuccess)
                    {
                        SeqStepNum = 0;
                        MeaRetryCnt++;
                        Utill.Delay(calInfo.CalDelayTime * 0.001);
                    }
                }
                SeqStepNum++;
            }
        }

        private bool MeaSequence(int stepNum, int pointIndex, out bool isSuccess)
        {
            bool isMeaEnd = isSuccess = true;
            MeaSeq stepName = (MeaSeq)stepNum;

            int voltPoint = int.Parse(MeaPointList[pointIndex][1].ToString());
            int currPoint = int.Parse(MeaPointList[pointIndex][2].ToString());

            switch (stepName)
            {
                case MeaSeq.REF_SET:
                    mcu.ChSet(ChNum, voltPoint, currPoint);
                    OnMeaMonitor(new CalMonitorArgs(PointIndex));
                    isMeaEnd = false;
                    break;

                case MeaSeq.DELAY1:
                    Utill.Delay(calInfo.CalDelayTime * 0.001);
                    isMeaEnd = false;
                    break;

                case MeaSeq.OUT_CHECK:
                    if (CalType == 'V')
                    {
                        if (Math.Abs(dmm.SensingData - voltPoint) > errRate)
                        {
                            OnMeaMonitor(new CalMonitorArgs(PointIndex));
                            isSuccess = false;
                            mcu.ChStop();
                        }
                        else
                        {
                            OnMeaMonitor(new CalMonitorArgs(PointIndex));
                            break;
                        }
                    }
                    else
                    {
                        if (Math.Abs(dmm.SensingData - currPoint) > errRate)
                        {
                            OnMeaMonitor(new CalMonitorArgs(PointIndex));
                            isSuccess = false;
                            mcu.ChStop();
                        }
                        else
                        {
                            OnMeaMonitor(new CalMonitorArgs(PointIndex));
                            break;
                        }
                    }
                    isMeaEnd = false;
                    break;

                case MeaSeq.OUT_STOP:
                    OnMeaMonitor(new CalMonitorArgs(PointIndex));
                    mcu.ChStop();
                    isMeaEnd = false;
                    break;

                case MeaSeq.DELAY2:
                    Utill.Delay(calInfo.CalDelayTime * 0.001);
                    isMeaEnd = true;
                    break;
            }

            return isMeaEnd;
        }

        public void OnCalMonitor(CalMonitorArgs e)
        {
            CalMonitor?.Invoke(this, e);
        }

        public void OnMeaMonitor(CalMonitorArgs e)
        {
            MeaMonitor?.Invoke(this, e);
        }
    }
}
