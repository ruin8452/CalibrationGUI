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
        END_CAL
    }
    enum MeaSeq
    {
        OUT_CHECK,
        END_MEA
    }

    public class Calibration
    {
        CalMeasureInfo calInfo = CalMeasureInfo.GetObj();
        Mcu mcu = Mcu.GetObj();
        Dmm dmm = Dmm.GetObj();

        char CalType;
        int ChNum;
        int errRate;
        object[][] PointList;

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

        public void AutoCalPointSet(char calType, int chNum, object[][] pointList)
        {
            CalType = calType;
            ChNum = chNum;
            PointList = pointList;
            errRate = calType == 'V' ? calInfo.CalErrRangeVolt : calInfo.CalErrRangeCurr;
        }

        int SeqStepNum = 0;
        int PointIndex = 0;
        private void CalTimer_Tick(object sender, EventArgs e)
        {
            if (PointList.Length <= PointIndex)
            {
                calTimer.Stop();
                return;
            }

            OnCalMonitor(new CalMonitorArgs(PointIndex));

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

            int voltPoint = int.Parse(PointList[pointIndex][1].ToString());
            int currPoint = int.Parse(PointList[pointIndex][2].ToString());

            switch (stepName)
            {
                case CalSeq.REF_SET:
                    mcu.ChSet(ChNum, voltPoint, currPoint);
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
                                mcu.ChCal(CalType, ChNum, dmm.SensingData * 10);
                            else
                                break;
                        }
                        else
                        {
                            if (Math.Abs(dmm.SensingData - currPoint) > errRate)
                                mcu.ChCal(CalType, ChNum, dmm.SensingData * 10);
                            else
                                break;
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
                    isCalEnd = true;
                    break;
            }

            return isCalEnd;
        }

        private void MeaTimer_Tick(object sender, EventArgs e)
        {
            if (PointList.Length <= PointIndex)
            {
                meaTimer.Stop();
                return;
            }

            OnMeaMonitor(new CalMonitorArgs(PointIndex));

            if (MeaSequence(SeqStepNum++, PointIndex))
            {
                PointIndex++;
                SeqStepNum = 0;
            }
        }

        private bool MeaSequence(int stepNum, int pointIndex)
        {
            bool isMeaEnd = true;
            MeaSeq stepName = (MeaSeq)stepNum;

            int voltPoint = int.Parse(PointList[pointIndex][1].ToString());
            int currPoint = int.Parse(PointList[pointIndex][2].ToString());

            switch (stepName)
            {
                case MeaSeq.OUT_CHECK:
                    for (int i = 0; i < calInfo.MeaErrRetryCnt; i++)
                    {
                        mcu.ChSet(ChNum, voltPoint, currPoint);
                        Utill.Delay(calInfo.MeaDelayTime * 0.001);

                        if (CalType == 'V')
                        {
                            if (Math.Abs(dmm.SensingData - voltPoint) > errRate)
                                mcu.ChStop();
                            else
                                break;
                        }
                        else
                        {
                            if (Math.Abs(dmm.SensingData - currPoint) > errRate)
                                mcu.ChStop();
                            else
                                break;
                        }
                        Utill.Delay(calInfo.MeaDelayTime * 0.001);
                    }
                    isMeaEnd = false;
                    break;

                case MeaSeq.END_MEA:
                    mcu.ChStop();
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
