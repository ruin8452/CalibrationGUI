using CalibrationNewGUI.Equipment;
using CalibrationNewGUI.Model;
using CalibrationNewGUI.ViewModel.Func.EventArgsClass;
using J_Project.Manager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        int delay;
        object[][] CalPointList;
        object[][] MeaPointList;

        BackgroundWorker calBack = new BackgroundWorker();
        BackgroundWorker meaBack = new BackgroundWorker();

        DispatcherTimer calTimer = new DispatcherTimer();
        DispatcherTimer meaTimer = new DispatcherTimer();

        public event EventHandler<CalMonitorArgs> CalMonitor;
        public event EventHandler CalEnd;
        public event EventHandler<CalMonitorArgs> MeaMonitor;
        public event EventHandler MeaEnd;

        public Calibration()
        {
            calBack.DoWork += new DoWorkEventHandler((object send, DoWorkEventArgs e) =>
            {
                CalTimer_Tick();
            });
            meaBack.DoWork += new DoWorkEventHandler((object send, DoWorkEventArgs e) =>
            {
                MeaTimer_Tick();
            });


            calTimer.Interval = TimeSpan.FromMilliseconds(50);    // ms
            calTimer.Tick += new EventHandler((object sender, EventArgs e) =>
            {
                if (calBack.IsBusy == false)
                    calBack.RunWorkerAsync();
            });

            meaTimer.Interval = TimeSpan.FromMilliseconds(50);    // ms
            meaTimer.Tick += new EventHandler((object sender, EventArgs e) =>
            {
                if (meaBack.IsBusy == false)
                    meaBack.RunWorkerAsync();
            });
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

        public void AutoCalPointSet(char calType, int chNum, int delay, object[][] calPointList, object[][] meaPointList, bool isFullRun)
        {
            CalType = calType;
            ChNum = chNum;
            this.delay = delay;
            CalPointList = calPointList;
            MeaPointList = meaPointList;
            errRate = calType == 'V' ? calInfo.CalErrRangeVolt : calInfo.CalErrRangeCurr;
            IsFullRun = isFullRun;
        }

        int SeqStepNum = 0;
        int PointIndex = 0;
        private void CalTimer_Tick()
        {
            if (CalPointList.Length <= PointIndex)
            {
                calTimer.Stop();
                OnCalEnd();

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
                    isCalEnd = false;
                    //임시 큐 함수용
                    //mcu.outputBuffer.chNum = ChNum;
                    //mcu.outputBuffer.volt = voltPoint;
                    //mcu.outputBuffer.curr = currPoint;
                    //mcu.sendWriteOutputStartFlag = 1;
                    //isCalEnd = false;
                    break;

                case CalSeq.DELAY1:
                    Utill.Delay(calInfo.CalDelayTime * 0.001);
                    OnCalMonitor(new CalMonitorArgs(PointIndex));
                    isCalEnd = false;
                    break;

                case CalSeq.REAL_VALUE_SEND:
                    for (int i = 0; i < calInfo.CalErrRetryCnt; i++)
                    {
                        if (CalType == 'V')
                        {
                            if (Math.Abs(dmm.Volt - voltPoint) > errRate)
                            {
                                mcu.ChCal(CalType, ChNum, dmm.Volt);
                                //임시 큐 함수용
                                //mcu.calBuffer.calType = CalType;
                                //mcu.calBuffer.chNum = ChNum;
                                //mcu.calBuffer.dmm = dmm.Volt;
                                //mcu.sendWriteDMMStartFlag = 1;
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
                            if (Math.Abs(dmm.Curr - currPoint) > errRate)
                            {
                                mcu.ChCal(CalType, ChNum, dmm.Curr);
                                //임시 큐 함수용
                                //mcu.calBuffer.calType = CalType;
                                //mcu.calBuffer.chNum = ChNum;
                                //mcu.calBuffer.dmm = dmm.Curr;
                                //mcu.sendWriteDMMStartFlag = 1;
                                OnCalMonitor(new CalMonitorArgs(PointIndex));
                            }
                            else
                            {
                                OnCalMonitor(new CalMonitorArgs(PointIndex));
                                break;
                            }
                        }
                        Utill.Delay(calInfo.CalDelayTime * 0.001);
                        OnCalMonitor(new CalMonitorArgs(PointIndex));
                    }
                    isCalEnd = false;
                    break;

                case CalSeq.DELAY2:
                    OnCalMonitor(new CalMonitorArgs(PointIndex));
                    Utill.Delay(calInfo.CalDelayTime * 0.001);
                    OnCalMonitor(new CalMonitorArgs(PointIndex));
                    isCalEnd = false;
                    break;

                case CalSeq.END_CAL:
                    OnCalMonitor(new CalMonitorArgs(PointIndex));
                    mcu.ChStop();
                    isCalEnd = false;

                    //임시 큐 함수용
                    //OnCalMonitor(new CalMonitorArgs(PointIndex));
                    //mcu.sendWriteOutputStopFlag = 1;
                    //isCalEnd = false;
                    break;

                case CalSeq.DELAY3:
                    Utill.Delay(calInfo.CalDelayTime * 0.001);
                    isCalEnd = true;
                    break;
            }

            return isCalEnd;
        }

        int MeaRetryCnt = 0;
        private void MeaTimer_Tick()
        {
            if (MeaPointList.Length <= PointIndex)
            {
                MeaRetryCnt = 0;
                meaTimer.Stop();
                OnMeaEnd();
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
                        Utill.Delay(calInfo.MeaDelayTime * 0.001);
                        return;
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
                    isMeaEnd = false;

                    //임시 큐 함수용
                    //mcu.outputBuffer.chNum = ChNum;
                    //mcu.outputBuffer.volt = voltPoint;
                    //mcu.outputBuffer.curr = currPoint;
                    //mcu.sendWriteOutputStartFlag = 1;
                    //isMeaEnd = false;
                    break;

                case MeaSeq.DELAY1:
                    Utill.Delay(calInfo.MeaDelayTime * 0.001);
                    OnMeaMonitor(new CalMonitorArgs(PointIndex));
                    isMeaEnd = false;
                    break;

                case MeaSeq.OUT_CHECK:
                    //Utill.Delay(0.3);
                    
                    if (CalType == 'V')
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            OnMeaMonitor(new CalMonitorArgs(PointIndex));
                            if (Math.Abs(dmm.Volt - voltPoint) < 100)
                            {
                                break;
                            }
                            Utill.Delay(0.1);
                        }
                        //OnMeaMonitor(new CalMonitorArgs(PointIndex));
                        if (Math.Abs(dmm.Volt - voltPoint) > errRate)
                        {
                            OnMeaMonitor(new CalMonitorArgs(PointIndex));
                            mcu.ChStop();
                            isSuccess = false;
                            //임시 큐 함수용
                            //mcu.sendWriteOutputStopFlag = 1;
                            //isSuccess = false;
                        }
                        else
                            OnMeaMonitor(new CalMonitorArgs(PointIndex));
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            OnMeaMonitor(new CalMonitorArgs(PointIndex));
                            if (Math.Abs(dmm.Volt - currPoint) < 1000)
                            {
                                break;
                            }
                            Utill.Delay(0.1);
                        }
                        //OnMeaMonitor(new CalMonitorArgs(PointIndex));
                        if (Math.Abs(dmm.Curr - currPoint) > errRate)
                        {
                            OnMeaMonitor(new CalMonitorArgs(PointIndex));
                            
                            mcu.ChStop();
                            isSuccess = false;

                            //임시 큐 함수용
                            //mcu.sendWriteOutputStopFlag = 1;
                            //isSuccess = false;
                        }
                        else
                            OnMeaMonitor(new CalMonitorArgs(PointIndex));
                    }
                    isMeaEnd = false;
                    break;

                case MeaSeq.OUT_STOP:
                    OnMeaMonitor(new CalMonitorArgs(PointIndex));
                    mcu.ChStop();
                    OnMeaMonitor(new CalMonitorArgs(PointIndex));
                    isMeaEnd = false;

                    //임시 큐 함수용
                    //OnMeaMonitor(new CalMonitorArgs(PointIndex));
                    //mcu.sendWriteOutputStopFlag = 1;
                    //isMeaEnd = false;
                    break;

                case MeaSeq.DELAY2:
                    Utill.Delay(calInfo.MeaDelayTime * 0.001);
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

        public void OnCalEnd()
        {
            CalEnd?.Invoke(this, EventArgs.Empty);
        }

        public void OnMeaEnd()
        {
            MeaEnd?.Invoke(this, EventArgs.Empty);
        }
    }
}
