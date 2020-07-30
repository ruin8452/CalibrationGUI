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

            SaveClick = new RelayCommand(DataSave);
        }

        public void DataSave()
        {
            ShuntInfos.Save();
            MessageBox.Show("저장 완료");
        }
    }
}
