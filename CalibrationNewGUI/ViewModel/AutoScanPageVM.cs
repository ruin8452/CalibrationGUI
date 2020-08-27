using CalibrationNewGUI.Equipment;
using CalibrationNewGUI.Message;
using CalibrationNewGUI.Model;
using CalibrationNewGUI.UI;
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
using System.Runtime.InteropServices;
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

        public int ScanTableSelectIndex { get; set; }
        public int McuTableSelectIndex { get; set; }

        public RelayCommand ModeSelectClick { get; set; }

        public RelayCommand PointCreateClick { get; set; }
        public RelayCommand PointAddClick { get; set; }
        public RelayCommand PointDelClick { get; set; }
        public RelayCommand PointAllAddClick { get; set; }
        public RelayCommand PointAllDelClick { get; set; }
        public RelayCommand<object> PointUpClick { get; set; }
        public RelayCommand<object> PointDownClick { get; set; }
        public RelayCommand McuPointPreviewClick { get; set; }

        public RelayCommand ScanStartClick { get; set; }
        public RelayCommand PointApplyClick { get; set; }

        [DllImportAttribute("user32.dll", EntryPoint = "FindWindow")]
        public static extern int FindWindow(string clsName, string wndName);

        public AutoScanPageVM()
        {
            minPoint = othersInfo.InputVoltMin;
            MaxPoint = othersInfo.InputVoltMax;
            //pointInterval = MIN_VOTE_INTERVAL;

            calManager.MeaMonitor += CalManager_MeaMonitor;


            string[] cloumnName = new string[] { "NO", "SetVolt", "SetCurr", "Correction", "OutVolt", "OutCurr", "OutDMM", "IsRangeIn" };
            Type[] cloumnType = new Type[] { typeof(int), typeof(float), typeof(float), typeof(float), typeof(double), typeof(double), typeof(double), typeof(bool) };
            ScanPointTable = new DataTable();
            ScanPointTable = TableManager.ColumnAdd(ScanPointTable, cloumnName, cloumnType);

            McuPointTable = new DataTable();
            McuPointTable = TableManager.ColumnAdd(McuPointTable, cloumnName);

            ModeSelectClick = new RelayCommand(ModeSelected);

            PointCreateClick = new RelayCommand(PointCreate);
            PointAddClick = new RelayCommand(PointAdd);
            PointDelClick = new RelayCommand(PointDel);
            PointAllAddClick = new RelayCommand(PointAllAdd);
            PointAllDelClick = new RelayCommand(PointAllDel);
            PointUpClick = new RelayCommand<object>(PointUp);
            PointDownClick = new RelayCommand<object>(PointDown);
            McuPointPreviewClick = new RelayCommand(McuPointPreview);

            ScanStartClick = new RelayCommand(ScanStart);
            PointApplyClick = new RelayCommand(PointApply);
        }

        private void ModeSelected()
        {
            OnChangeCalOption();
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

        private void PointAllAdd()
        {
            if (ScanPointTable.Rows.Count == 0)
                return;

            foreach(DataRow addRow in ScanPointTable.Rows)
            {
                McuPointTable = TableManager.RowAdd(McuPointTable, McuPointTable.Rows.Count, addRow);

                if (ModeSelecte)
                {
                    if (TableManager.OverlapCheck(McuPointTable, McuPointTable.Columns["SetVolt"].Ordinal))
                        McuPointTable = TableManager.RowDelete(McuPointTable, McuPointTable.Rows.Count - 1);
                }
                else
                {
                    if (TableManager.OverlapCheck(McuPointTable, McuPointTable.Columns["SetCurr"].Ordinal))
                        McuPointTable = TableManager.RowDelete(McuPointTable, McuPointTable.Rows.Count - 1);
                }
            }
        }

        private void PointDel()
        {
            if (McuPointTable.Rows.Count == 0)
                return;

            McuPointTable = TableManager.RowDelete(McuPointTable, McuTableSelectIndex);
        }

        private void PointAllDel()
        {
            if (McuPointTable.Rows.Count == 0)
                return;

            McuPointTable.Clear();
        }

        /**
         *  @brief 포인트 Up
         *  @details 포인트 타입에 따라 테이블의 포인트 인덱스를 Up
         *  
         *  @param object type 호출한 객체의 타입(SCAN, CAL)
         *  
         *  @return
         */
        private void PointUp(object type)
        {
            //<C>20.SSW 07.15 : 선택되어 있는 Row가 삭제될 시 바인딩 되어 있는 Index 변수가 -1로 변경되기 때문에 다른 변수로 컨트롤
            int tempIndex;

            // CAL포인트 UP
            if (type.ToString() == "SCAN")
            {
                tempIndex = ScanTableSelectIndex;
                ScanPointTable = TableManager.RowUp(ScanPointTable, tempIndex);

                //<C>20.SSW 07.15 : 움직인 포인터의 초점을 유지
                if (tempIndex <= 0)
                    ScanTableSelectIndex = tempIndex;
                else
                    ScanTableSelectIndex = tempIndex - 1;
            }
            // 실측포인트 UP
            else
            {
                tempIndex = McuTableSelectIndex;
                McuPointTable = TableManager.RowUp(McuPointTable, tempIndex);

                //<C>20.SSW 07.15 : 움직인 포인터의 초점을 유지
                if (tempIndex <= 0)
                    McuTableSelectIndex = tempIndex;
                else
                    McuTableSelectIndex = tempIndex - 1;
            }
        }

        /**
         *  @brief 포인트 Down
         *  @details 포인트 타입에 따라 테이블의 포인트 인덱스를 Down
         *  
         *  @param object type 호출한 객체의 타입(SCAN, CAL)
         *  
         *  @return
         */
        private void PointDown(object type)
        {
            //<C>20.SSW 07.15 : 선택되어 있는 Row가 삭제될 시 바인딩 되어 있는 Index 변수가 -1로 변경되기 때문에 다른 변수로 컨트롤
            int tempIndex;

            // CAL포인트 DOWN
            if (type.ToString() == "SCAN")
            {
                tempIndex = ScanTableSelectIndex;
                ScanPointTable = TableManager.RowDown(ScanPointTable, tempIndex);

                //<C>20.SSW 07.15 : 움직인 포인터의 초점을 유지
                if (tempIndex >= ScanPointTable.Rows.Count - 1)
                    ScanTableSelectIndex = tempIndex;
                else
                    ScanTableSelectIndex = tempIndex + 1;
            }
            // 실측포인트 DOWN
            else
            {
                tempIndex = McuTableSelectIndex;
                McuPointTable = TableManager.RowDown(McuPointTable, tempIndex);

                //<C>20.SSW 07.15 : 움직인 포인터의 초점을 유지
                if (tempIndex >= McuPointTable.Rows.Count - 1)
                    McuTableSelectIndex = tempIndex;
                else
                    McuTableSelectIndex = tempIndex + 1;
            }
        }

        private void McuPointPreview()
        {
            McuPointViewVM.SetValue(ModeSelecte ? 'V' : 'I', ChSelected ? 1 : 2);

            McuPointViewWindow pointView = new McuPointViewWindow
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (FindWindow(null, pointView.Title) == 0)
                pointView.Show();
            else
                return;
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

            calManager.MeaSeqSet(ModeSelecte ? 'V' : 'I', ChSelected ? 1 : 2, Delay, ModeSelecte ? voltErrRange : currErrRange, tempPoint.ToArray(), false);
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

                if (Math.Abs(tempVolt - Dmm.Volt) > voltErrRange)
                    ScanPointTable.Rows[e.Index]["IsRangeIn"] = false;
                else
                    ScanPointTable.Rows[e.Index]["IsRangeIn"] = true;
            }
            else   // 전류
            {
                ScanPointTable.Rows[e.Index]["OutDMM"] = Dmm.Curr;

                float tempCurr = ScanPointTable.Rows[e.Index].Field<float>("SetCurr");

                if (Math.Abs(tempCurr - Dmm.Curr) > currErrRange)
                    ScanPointTable.Rows[e.Index]["IsRangeIn"] = false;
                else
                    ScanPointTable.Rows[e.Index]["IsRangeIn"] = true;
            }
        }

        /**
         *  @brief CAL 옵션(모드, 채널번호) 메세지 전송
         *  @details CAL 옵션(모드, 채널번호) 메세지를 전송
         *  
         *  @param
         *  
         *  @return
         */
        private void OnChangeCalOption()
        {
            CalOptionMessage Message = new CalOptionMessage
            {
                CalType = ModeSelecte ? 'V' : 'I',
                ChNumber = ChSelected ? 1 : 2
            };

            Messenger.Default.Send(Message);
        }
    }
}
