using CalibrationNewGUI.Model;
using GalaSoft.MvvmLight.Command;
using PropertyChanged;
using System.Windows;
using System.Windows.Controls;

namespace CalibrationNewGUI.ViewModel.Setting
{
    [ImplementPropertyChanged]
    public class ShuntSettingVM
    {
        public bool a { get; set; }
        public ShuntInfo ShuntInfos { get; set; }

        public RelayCommand SaveClick { get; set; }

        public ShuntSettingVM()
        {
            ShuntInfos = ShuntInfo.GetObj();
            if (ShuntInfos.CorrectionMode == true)
                ShuntInfos.shuntReg = (float)ShuntInfos.ShuntStandardCurr / (float)ShuntInfos.ShuntNewCurr; //첫 실행 할때 션트 보정치를 계산
            else
            {
                if (ShuntInfos.ShuntNewReg == 0)
                {
                    ShuntInfos.shuntReg = 1;
                }
                else
                {
                    ShuntInfos.shuntReg = (1 / (ShuntInfos.ShuntNewReg * 0.001f)) * 1000;
                }
            }
                
            SaveClick = new RelayCommand(DataSave);
        }

        public void DataSave()
        {
            bool b = a;

            ShuntInfos.Save();
            if (ShuntInfos.CorrectionMode == true)
                ShuntInfos.shuntReg = (float)ShuntInfos.ShuntStandardCurr / (float)ShuntInfos.ShuntNewCurr;
            else
            {
                if (ShuntInfos.ShuntNewReg == 0)
                {
                    ShuntInfos.shuntReg = 1;
                }
                else
                {
                    ShuntInfos.shuntReg = (1 / (ShuntInfos.ShuntNewReg * 0.001f)) * 1000;
                }
            }
            //MessageBox.Show(App.GetString("SaveOkMsg"));
        }
    }
}
