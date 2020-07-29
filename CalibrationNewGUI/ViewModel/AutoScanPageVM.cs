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
using System.Threading.Tasks;
using System.Windows;

namespace CalibrationNewGUI.ViewModel
{
    [ImplementPropertyChanged]
    public class AutoScanPageVM : ViewModelBase
    {
        const int MIN_VOTE_INTERVAL = 10;
        const int MIN_CURR_INTERVAL = 1000;
        const int DEFAULT_CURR = 1000;

        CalMeasureInfo CalMeaInfo = CalMeasureInfo.GetObj();
        OthersInfo othersInfo = OthersInfo.GetObj();
        Calibration calManager = new Calibration();

        Mcu Mcu = Mcu.GetObj();
        Dmm Dmm = Dmm.GetObj();

        public bool ModelSelecte { get; set; } = true;
        public bool ChSelected { get; set; } = true;
        private int minPoint;
        public int MinPoint
        {
            get { return minPoint; }
            set
            {
                if(ModelSelecte)
                {
                    if (value < othersInfo.InputVoltMin)
                    {
                        MessageBox.Show("최소 전압 설정값 보다 작은 값은 입력할 수 없습니다.");
                        minPoint = othersInfo.InputVoltMin;
                        return;
                    }

                    if(value > maxPoint)
                    {
                        MessageBox.Show("최대값 보다 큰 값은 입력할 수 없습니다.");
                        minPoint = maxPoint;
                        return;
                    }
                    minPoint = value;
                }
                else
                {
                    if (value < othersInfo.InputCurrMin)
                    {
                        MessageBox.Show("최소 전류 설정값 보다 작은 값은 입력할 수 없습니다.");
                        minPoint = othersInfo.InputCurrMin;
                        return;
                    }

                    if (value > maxPoint)
                    {
                        MessageBox.Show("최대값 보다 큰 값은 입력할 수 없습니다.");
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
                if (ModelSelecte)
                {
                    if (value > othersInfo.InputVoltMax)
                    {
                        MessageBox.Show("최대 전압 설정값 보다 큰 값은 입력할 수 없습니다.");
                        maxPoint = othersInfo.InputVoltMax;
                        return;
                    }

                    if (value < minPoint)
                    {
                        MessageBox.Show("최소값 보다 작은 값은 입력할 수 없습니다.");
                        maxPoint = minPoint;
                        return;
                    }
                    maxPoint = value;
                }
                else
                {
                    if (value > othersInfo.InputCurrMax)
                    {
                        MessageBox.Show("최대 전류 설정값 보다 큰 값은 입력할 수 없습니다.");
                        maxPoint = othersInfo.InputCurrMax;
                        return;
                    }

                    if (value < minPoint)
                    {
                        MessageBox.Show("최소값 보다 작은 값은 입력할 수 없습니다.");
                        maxPoint = minPoint;
                        return;
                    }
                    maxPoint = value;
                }
            }
        }
        private int pointInterval;
        public int PointInterval
        {
            get { return pointInterval; }
            set
            {
                if(value < MIN_VOTE_INTERVAL)
                {
                    MessageBox.Show($"최소 간격({MIN_VOTE_INTERVAL})보다 낮은 값은 입력할 수 없습니다.");
                    pointInterval = MIN_VOTE_INTERVAL;
                    return;
                }
                pointInterval = value;
            }
        }
        public int ErrRange { get; set; }

        public DataTable ScanPointTable { get; set; }
        public DataTable McuPointTable { get; set; }

        public int ScanTableSelectIndex { get; set; }
        public int McuTableSelectIndex { get; set; }

        public RelayCommand PointCreateClick { get; set; }
        public RelayCommand PointAddClick { get; set; }
        public RelayCommand PointDelClick { get; set; }
        public RelayCommand ScanStartClick { get; set; }
        public RelayCommand PointApplyClick { get; set; }

        public AutoScanPageVM()
        {
            minPoint = othersInfo.InputVoltMin;
            MaxPoint = othersInfo.InputVoltMax;
            pointInterval = MIN_VOTE_INTERVAL;

            calManager.MeaMonitor += CalManager_MeaMonitor;

            ScanPointTable = new DataTable();
            ScanPointTable = TableManager.ColumnAdd(ScanPointTable, new string[] { "NO", "SetVolt", "SetCurr", "OutVolt", "OutCurr", "OutDMM", "IsRangeIn" });

            McuPointTable = new DataTable();
            McuPointTable = TableManager.ColumnAdd(McuPointTable, new string[] { "NO", "SetVolt", "SetCurr", "OutVolt", "OutCurr", "OutDMM", "IsRangeIn" });

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
            if (ModelSelecte)
            {
                for (int setVolt = MaxPoint; setVolt > MinPoint; setVolt -= PointInterval)
                    ScanPointTable = TableManager.RowAdd(ScanPointTable, ScanPointTable.Rows.Count, setVolt, DEFAULT_CURR);

                ScanPointTable = TableManager.RowAdd(ScanPointTable, ScanPointTable.Rows.Count, MinPoint, DEFAULT_CURR);
            }
            // 전류 스캔
            else
            {
                for (int setCurr = MaxPoint; setCurr > MinPoint; setCurr -= PointInterval)
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
                MessageBox.Show("장비 연결이 끊겨있습니다.");
                return;
            }

            List<object[]> tempPoint = new List<object[]>();

            foreach (DataRow row in ScanPointTable.Rows)
            {
                if (string.IsNullOrEmpty(row[1].ToString()) || string.IsNullOrEmpty(row[2].ToString()))
                {
                    MessageBox.Show("비어있는 셀이 있습니다.");
                    return;
                }
                tempPoint.Add(row.ItemArray);
            }

            calManager.AutoCalPointSet(ModelSelecte ? 'V' : 'I', ChSelected ? 1 : 2, null, tempPoint.ToArray(), false);
            calManager.MeaStart();
        }

        private void PointApply()
        {
            List<object[]> tempPoint = new List<object[]>();

            foreach (DataRow row in McuPointTable.Rows)
            {
                if (string.IsNullOrEmpty(row[1].ToString()) || string.IsNullOrEmpty(row[2].ToString()))
                {
                    MessageBox.Show("비어있는 셀이 있습니다.");
                    return;
                }
                tempPoint.Add(row.ItemArray);
            }

            CalPointMessege Message = new CalPointMessege { CalPointList = tempPoint };
            Messenger.Default.Send(Message);
        }

        private void CalManager_MeaMonitor(object sender, CalMonitorArgs e)
        {
            if (ChSelected) // 채널1
            {
                ScanPointTable.Rows[e.Index][3] = Mcu.Ch1Volt;
                ScanPointTable.Rows[e.Index][4] = Mcu.Ch1Curr;
            }
            else // 채널2
            {
                ScanPointTable.Rows[e.Index][3] = Mcu.Ch2Volt;
                ScanPointTable.Rows[e.Index][4] = Mcu.Ch2Curr;
            }
            ScanPointTable.Rows[e.Index][5] = Dmm.SensingData;

            // DMM이 오차범위 안에 들어있는지 검사
            if (ModelSelecte)   // 전압
            {
                int tempVolt = int.Parse(ScanPointTable.Rows[e.Index][1].ToString());

                if (Math.Abs(tempVolt - Dmm.SensingData) > CalMeaInfo.MeaErrRangeVolt)
                    ScanPointTable.Rows[e.Index][6] = false;
                else
                    ScanPointTable.Rows[e.Index][6] = true;
            }
            else   // 전류
            {
                int tempCurr = int.Parse(ScanPointTable.Rows[e.Index][2].ToString());

                if (Math.Abs(tempCurr - Dmm.SensingData) > CalMeaInfo.MeaErrRangeVolt)
                    ScanPointTable.Rows[e.Index][6] = false;
                else
                    ScanPointTable.Rows[e.Index][6] = true;
            }
        }
    }
}
