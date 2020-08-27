using CalibrationNewGUI.Equipment;
using CalibrationNewGUI.Message;
using CalibrationNewGUI.ViewModel.Func;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using PropertyChanged;
using System.Data;

namespace CalibrationNewGUI.ViewModel
{
    [ImplementPropertyChanged]
    public class McuPointViewVM
    {
        public DataTable McuPointTable { get; set; }
        public char CalMode { get; set; } = 'V';
        public string SelectedCh { get; set; } = "CH1";

        static char mode = 'V';
        static int ChNumber = 1;

        Mcu Mcu = Mcu.GetObj();

        public RelayCommand RefreshClick { get; set; }

        public McuPointViewVM()
        {
            Messenger.Default.Register<CalOptionMessage>(this, OnReceiveMessageAction);

            string[] cloumnName = new string[] { "NO", "SetVolt", "SetCurr", "Correction", "OutVolt", "OutCurr", "OutDMM", "IsRangeIn" };
            McuPointTable = new DataTable();
            McuPointTable = TableManager.ColumnAdd(McuPointTable, cloumnName);

            PointDownload();

            CalMode = mode;
            SelectedCh = ChNumber == 1 ? "CH1" : "CH2";

            RefreshClick = new RelayCommand(Refresh);
        }

        public static void SetValue(char calMode, int chNum)
        {
            mode = calMode;
            ChNumber = chNum;
        }

        private void Refresh()
        {
            PointDownload();

            CalMode = mode;
            SelectedCh = ChNumber == 1 ? "CH1" : "CH2";
        }

        private void PointDownload()
        {
            if (!Mcu.IsConnected)
                return;

            float[][] pointList = Mcu.CalPointCheck(CalMode, ChNumber);

            McuPointTable.Clear();
            if (CalMode == 'V')
            {
                foreach (float[] tempPoint in pointList)
                    McuPointTable = TableManager.RowAdd(McuPointTable, McuPointTable.Rows.Count, (int)tempPoint[0], 2000, (int)tempPoint[1]);
            }
            else
            {
                foreach (float[] tempPoint in pointList)
                {
                    if (tempPoint[0] >= 0)
                        McuPointTable = TableManager.RowAdd(McuPointTable, McuPointTable.Rows.Count, 4200, (int)tempPoint[0], (int)tempPoint[1]);
                    else
                        McuPointTable = TableManager.RowAdd(McuPointTable, McuPointTable.Rows.Count, 2700, (int)tempPoint[0], (int)tempPoint[1]);
                }
            }
        }


        /**
         *  @brief 메세지 수신
         *  @details 다른 ViewModel 클래스에서 보낸 메세지를 수신
         *  
         *  @param EquiMessage obj 수신받을 데이터가 담겨있는 클래스
         *  
         *  @return
         */
        private void OnReceiveMessageAction(CalOptionMessage option)
        {
            mode = option.CalType;
            ChNumber = option.ChNumber;
        }
    }
}
