using CalibrationNewGUI.Equipment;
using CalibrationNewGUI.Message;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.ViewModel
{
    [ImplementPropertyChanged]
    public class ManualCalVM
    {
        Mcu Mcu = Mcu.GetObj();
        Dmm Dmm = Dmm.GetObj();

        private int ChNum;
        public char CalType { get; set; } = 'V';
        public string SelectedCh { get; set; } = "CH1";
        public int CalVolt { get; set; }
        public int CalCurr { get; set; }

        public RelayCommand OutputStartClick { get; set; }
        public RelayCommand CalStartClick { get; set; }
        public RelayCommand OutputStopClick { get; set; }

        public ManualCalVM()
        {
            Messenger.Default.Register<CalOptionMessege>(this, OnReceiveMessageAction);

            OutputStartClick = new RelayCommand(OutputStart);
            CalStartClick = new RelayCommand(CalStart);
            OutputStopClick = new RelayCommand(OutputStop);
        }

        public void SetCalOption(char calType, int chNum)
        {
            CalType = calType;
            ChNum = chNum;

            if (chNum == 0) SelectedCh = "ALL";
            else if (chNum == 1) SelectedCh = "CH1";
            else SelectedCh = "CH2";
        }

        private void OutputStart()
        {
            Mcu.ChSet(ChNum, CalVolt, CalCurr);
        }
        private void CalStart()
        {
            Mcu.ChCal(CalType, ChNum, Dmm.SensingData);
        }
        private void OutputStop()
        {
            Mcu.ChStop();
        }

        /**
         *  @brief 메세지 수신
         *  @details 다른 ViewModel 클래스에서 보낸 메세지를 수신
         *  
         *  @param EquiMessage obj 수신받을 데이터가 담겨있는 클래스
         *  
         *  @return
         */
        private void OnReceiveMessageAction(CalOptionMessege option)
        {
            CalType = option.CalType;
            ChNum = option.ChNumber;

            if (ChNum == 0) SelectedCh = "ALL";
            else if (ChNum == 1) SelectedCh = "CH1";
            else SelectedCh = "CH2";
        }
    }
}
