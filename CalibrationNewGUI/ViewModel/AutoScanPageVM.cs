using CalibrationNewGUI.Equipment;
using CalibrationNewGUI.Message;
using CalibrationNewGUI.Model;
using CalibrationNewGUI.ViewModel.Func;
using CalibrationNewGUI.ViewModel.Func.EventArgsClass;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CalibrationNewGUI.ViewModel
{
    [ImplementPropertyChanged]
    public class AutoScanPageVM : ViewModelBase
    {
        const int MIN_VOTE_INTERVAL = 100;
        const int MIN_CURR_INTERVAL = 5000;
        const int DEFAULT_CURR = 1000;

        const int MIN_VOLT_ERR_RANGE = 1;
        const int MIN_CURR_ERR_RANGE = 5;

        const int MIN_DELAY = 2000;

        CalMeasureInfo CalMeaInfo = CalMeasureInfo.GetObj();
        OthersInfo othersInfo = OthersInfo.GetObj();
        Calibration calManager = new Calibration();

        Mcu Mcu = Mcu.GetObj();
        Dmm Dmm = Dmm.GetObj();

        public bool ModeSelecte { get; set; } = true;
        public bool ChSelected { get; set; } = true;

        private int minPoint;
        public int MinPoint
        {
            get { return minPoint; }
            set
            {
                if(ModeSelecte)
                {
                    if (value < othersInfo.InputVoltMin)
                    {
                        MessageBox.Show(App.GetString("MinimumVoltErrMsg"));
                        minPoint = othersInfo.InputVoltMin;
                        return;
                    }

                    if(value > maxPoint)
                    {
                        MessageBox.Show(App.GetString("OverValueErrMsg"));
                        minPoint = maxPoint;
                        return;
                    }
                    minPoint = value;
                }
                else
                {
                    if (value < othersInfo.InputCurrMin)
                    {
                        MessageBox.Show(App.GetString("MinimumCurrErrMsg"));
                        minPoint = othersInfo.InputCurrMin;
                        return;
                    }

                    if (value > maxPoint)
                    {
                        MessageBox.Show(App.GetString("OverValueErrMsg"));
                        minPoint = maxPoint;
                        return;
                    }
                    minPoint = value;
                }
            }
        }

        private int maxPoint;
        public int MaxPoint
        {
            get { return maxPoint; }
            set
            {
                if (ModeSelecte)
                {
                    if (value > othersInfo.InputVoltMax)
                    {
                        MessageBox.Show(App.GetString("MaximumVoltErrMsg"));
                        maxPoint = othersInfo.InputVoltMax;
                        return;
                    }

                    if (value < minPoint)
                    {
                        MessageBox.Show(App.GetString("UnderValueErrMsg"));
                        maxPoint = minPoint;
                        return;
                    }
                    maxPoint = value;
                }
                else
                {
                    if (value > othersInfo.InputCurrMax)
                    {
                        MessageBox.Show(App.GetString("MaximumCurrErrMsg"));
                        maxPoint = othersInfo.InputCurrMax;
                        return;
                    }

                    if (value < minPoint)
                    {
                        MessageBox.Show(App.GetString("UnderValueErrMsg"));
                        maxPoint = minPoint;
                        return;
                    }
                    maxPoint = value;
                }
            }
        }

        private int voltInterval = MIN_VOTE_INTERVAL;
        public int VoltInterval
        {
            get { return voltInterval; }
            set
            {
                if (value < MIN_VOTE_INTERVAL)
                {
                    voltInterval = MIN_VOTE_INTERVAL;
                    return;
                }
                voltInterval = value;
            }
        }
        private int currInterval = MIN_CURR_INTERVAL;
        public int CurrInterval
        {
            get { return currInterval; }
            set
            {
                if (value < MIN_CURR_INTERVAL)
                {
                    currInterval = MIN_CURR_INTERVAL;
                    return;
                }
                currInterval = value;
            }
        }

        private int voltErrRange = MIN_VOLT_ERR_RANGE;
        public int VoltErrRange
        {
            get { return voltErrRange; }
            set
            {
                if (value < MIN_VOLT_ERR_RANGE) voltErrRange = MIN_VOLT_ERR_RANGE;
                else voltErrRange = value;
            }
        }
        private int currErrRange = MIN_CURR_ERR_RANGE;
        public int CurrErrRange
        {
            get { return currErrRange; }
            set
            {
                if (value < MIN_CURR_ERR_RANGE) currErrRange = MIN_CURR_ERR_RANGE;
                else currErrRange = value;
            }
        }

        private int delay = MIN_DELAY;
        public int Delay
        {
            get { return delay; }
            set
            {
                if (value < MIN_DELAY) delay = MIN_DELAY;
                else delay = value;
            }
        }

        public DataTable ScanPointTable { get; set; }
        public DataTable McuPointTable { get; set; }

        private int scanTableSelectIndex;
        public int ScanTableSelectIndex
        {
            get { return scanTableSelectIndex; }
            set
            {
                if (value == -1) scanTableSelectIndex = 0;
                else             scanTableSelectIndex = value;
            }
        }
        private int mcuTableSelectIndex;
        public int McuTableSelectIndex
        {
            get { return mcuTableSelectIndex; }
            set
            {
                if (value == -1) mcuTableSelectIndex = 0;
                else             mcuTableSelectIndex = value;
            }
        }

        public RelayCommand PointCreateClick { get; set; }
        public RelayCommand PointAddClick { get; set; }
        public RelayCommand PointDelClick { get; set; }
        public RelayCommand ScanStartClick { get; set; }
        public RelayCommand PointApplyClick { get; set; }

        public AutoScanPageVM()
        {
            minPoint = othersInfo.InputVoltMin;
            MaxPoint = othersInfo.InputVoltMax;
            //pointInterval = MIN_VOTE_INTERVAL;

            calManager.MeaMonitor += CalManager_MeaMonitor;


            string[] cloumnName = new string[] { "NO", "SetVolt", "SetCurr", "Correction", "OutVolt", "OutCurr", "OutDMM", "IsRangeIn" };
            ScanPointTable = new DataTable();
            ScanPointTable = TableManager.ColumnAdd(ScanPointTable, cloumnName);

            McuPointTable = new DataTable();
            McuPointTable = TableManager.ColumnAdd(McuPointTable, cloumnName);

            PointCreateClick = new RelayCommand(PointCreate);
            PointAddClick = new RelayCommand(PointAdd);
            PointDelClick = new RelayCommand(PointDel);
            ScanStartClick = new RelayCommand(ScanStart);
            PointApplyClick = new RelayCommand(PointApply);
        }

        private void PointCreate()
        {
            ScanPointTable.Clear();

            // 전압 스캔
            if (ModeSelecte)
            {
                for (int setVolt = MaxPoint; setVolt > MinPoint; setVolt -= voltInterval)
                    ScanPointTable = TableManager.RowAdd(ScanPointTable, ScanPointTable.Rows.Count, setVolt, DEFAULT_CURR);

                ScanPointTable = TableManager.RowAdd(ScanPointTable, ScanPointTable.Rows.Count, MinPoint, DEFAULT_CURR);
            }
            // 전류 스캔
            else
            {
                for (int setCurr = MaxPoint; setCurr > MinPoint; setCurr -= currInterval)
                {
                    if(setCurr >= 0)
                        ScanPointTable = TableManager.RowAdd(ScanPointTable, ScanPointTable.Rows.Count, 4200, setCurr);
                    else
                        ScanPointTable = TableManager.RowAdd(ScanPointTable, ScanPointTable.Rows.Count, 2700, setCurr);
                }

                if (MinPoint >= 0)
                    ScanPointTable = TableManager.RowAdd(ScanPointTable, ScanPointTable.Rows.Count, 4200, MinPoint);
                else
                    ScanPointTable = TableManager.RowAdd(ScanPointTable, ScanPointTable.Rows.Count, 2700, MinPoint);
            }
        }

        private void PointAdd()
        {
            if (ScanPointTable.Rows.Count == 0)
                return;

            McuPointTable = TableManager.RowAdd(McuPointTable, McuPointTable.Rows.Count, ScanPointTable.Rows[ScanTableSelectIndex]);

            if(ModeSelecte)
            {
                if (TableManager.OverlapCheck(McuPointTable, McuPointTable.Columns["SetVolt"].Ordinal))
                {
                    McuPointTable = TableManager.RowDelete(McuPointTable, McuPointTable.Rows.Count - 1);
                    MessageBox.Show(App.GetString("PointOverlapErrMsg"));
                }
            }
            else
            {
                if (TableManager.OverlapCheck(McuPointTable, McuPointTable.Columns["SetCurr"].Ordinal))
                {
                    McuPointTable = TableManager.RowDelete(McuPointTable, McuPointTable.Rows.Count - 1);
                    MessageBox.Show(App.GetString("PointOverlapErrMsg"));
                }
            }
        }

        private void PointDel()
        {
            if (McuPointTable.Rows.Count == 0)
                return;

            McuPointTable = TableManager.RowDelete(McuPointTable, McuTableSelectIndex);
        }

        private void ScanStart()
        {
            if (!Mcu.IsConnected || !Dmm.IsConnected)
            {
                MessageBox.Show(App.GetString("EquiErrMsg"));
                return;
            }

            List<object[]> tempPoint = new List<object[]>();

            foreach (DataRow row in ScanPointTable.Rows)
                tempPoint.Add(row.ItemArray);

            calManager.MeaSeqSet(ModeSelecte ? 'V' : 'I', ChSelected ? 1 : 2, Delay, tempPoint.ToArray(), false);
            calManager.MeaStart();
        }

        private void PointApply()
        {
            // 테이블 데이터 정상 체크
            if(ModeSelecte)
            {
                if (TableManager.OverlapCheck(McuPointTable, McuPointTable.Columns["SetVolt"].Ordinal))
                {
                    MessageBox.Show(App.GetString("PointOverlapErrMsg"));
                    return;
                }
                if (TableManager.WrongDataCheck(McuPointTable, McuPointTable.Columns["SetVolt"].Ordinal, new Regex("[0-9]+")))
                {
                    MessageBox.Show(App.GetString("WrongDataErrMsg"));
                    return;
                }
                foreach(DataRow row in McuPointTable.Rows)
                {
                    if(int.Parse(row["SetVolt"].ToString()) > othersInfo.InputVoltMax || int.Parse(row["SetVolt"].ToString()) < othersInfo.InputVoltMin)
                    {
                        MessageBox.Show(string.Format(App.GetString("VoltRangeOutErrMsg"), othersInfo.InputVoltMax, othersInfo.InputVoltMin));
                        return;
                    }
                }
            }
            else
            {
                if (TableManager.OverlapCheck(McuPointTable, McuPointTable.Columns["SetCurr"].Ordinal))
                {
                    MessageBox.Show(App.GetString("PointOverlapErrMsg"));
                    return;
                }
                if (TableManager.WrongDataCheck(McuPointTable, McuPointTable.Columns["SetCurr"].Ordinal, new Regex("[0-9]+")))
                {
                    MessageBox.Show(App.GetString("WrongDataErrMsg"));
                    return;
                }
                foreach (DataRow row in McuPointTable.Rows)
                {
                    if (int.Parse(row["SetCurr"].ToString()) > othersInfo.InputVoltMax || int.Parse(row["SetCurr"].ToString()) < othersInfo.InputVoltMin)
                    {
                        MessageBox.Show(string.Format(App.GetString("CurrRangeOutErrMsg"), othersInfo.InputCurrMax, othersInfo.InputCurrMin));
                        return;
                    }
                }
            }

            List<object[]> tempPoint = new List<object[]>();

            foreach (DataRow row in McuPointTable.Rows)
            {
                if (string.IsNullOrEmpty(row["SetVolt"].ToString()) || string.IsNullOrEmpty(row["SetCurr"].ToString()))
                {
                    MessageBox.Show(App.GetString("EmptyCellErrMsg"));
                    return;
                }
                tempPoint.Add(row.ItemArray);
            }

            CalPointMessage Message = new CalPointMessage
            {
                CalMode = ModeSelecte,
                CalPointList = tempPoint
            };
            Messenger.Default.Send(Message);
        }

        private void CalManager_MeaMonitor(object sender, CalMonitorArgs e)
        {
            if (ChSelected) // 채널1
            {
                ScanPointTable.Rows[e.Index]["OutVolt"] = Mcu.Ch1Volt;
                ScanPointTable.Rows[e.Index]["OutCurr"] = Mcu.Ch1Curr;
            }
            else // 채널2
            {
                ScanPointTable.Rows[e.Index]["OutVolt"] = Mcu.Ch2Volt;
                ScanPointTable.Rows[e.Index]["OutCurr"] = Mcu.Ch2Curr;
            }

            // DMM이 오차범위 안에 들어있는지 검사
            if (ModeSelecte)   // 전압
            {
                ScanPointTable.Rows[e.Index]["OutDMM"] = Dmm.Volt;

                float tempVolt = ScanPointTable.Rows[e.Index].Field<float>("SetVolt");

                if (Math.Abs(tempVolt - Dmm.Volt) > CalMeaInfo.MeaErrRangeVolt)
                    ScanPointTable.Rows[e.Index]["IsRangeIn"] = false;
                else
                    ScanPointTable.Rows[e.Index]["IsRangeIn"] = true;
            }
            else   // 전류
            {
                ScanPointTable.Rows[e.Index]["OutDMM"] = Dmm.Curr;

                float tempCurr = ScanPointTable.Rows[e.Index].Field<float>("SetCurr");

                if (Math.Abs(tempCurr - Dmm.Curr) > CalMeaInfo.MeaErrRangeVolt)
                    ScanPointTable.Rows[e.Index]["IsRangeIn"] = false;
                else
                    ScanPointTable.Rows[e.Index]["IsRangeIn"] = true;
            }
        }
    }
}
