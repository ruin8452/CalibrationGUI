using CalibrationNewGUI.Model;
using GalaSoft.MvvmLight.Command;
using PropertyChanged;
using System.Windows;

namespace CalibrationNewGUI.ViewModel.Setting
{
    [ImplementPropertyChanged]
    public class ShuntSettingVM
    {
        public ShuntInfo ShuntInfos { get; set; }

        public RelayCommand SaveClick { get; set; }

        public ShuntSettingVM()
        {
            ShuntInfos = ShuntInfo.GetObj();
            ShuntInfos.shuntReg = (float)ShuntInfos.ShuntStandardReg / (float)ShuntInfos.ShuntNewReg; //첫 실행 할때 션트 보정치를 계산
            SaveClick = new RelayCommand(DataSave);
        }

        public void DataSave()
        {
            ShuntInfos.Save();
            ShuntInfos.shuntReg = (float)ShuntInfos.ShuntStandardReg / (float)ShuntInfos.ShuntNewReg; //션트값 변화를 할수도 있기 때문에 저장할때 다시 계산
            MessageBox.Show(App.GetString("SaveOkMsg"));
        }
    }
}
