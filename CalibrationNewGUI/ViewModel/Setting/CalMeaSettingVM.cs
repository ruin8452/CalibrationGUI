using CalibrationNewGUI.Model;
using GalaSoft.MvvmLight.Command;
using PropertyChanged;
using System.Windows;

namespace CalibrationNewGUI.ViewModel.Setting
{
    [ImplementPropertyChanged]
    public class CalMeaSettingVM
    {
        public CalMeasureInfo CalMeasureInfos { get; set; }

        public RelayCommand SaveClick { get; set; }

        public CalMeaSettingVM()
        {
            CalMeasureInfos = CalMeasureInfo.GetObj();

            SaveClick = new RelayCommand(DataSave);
        }

        public void DataSave()
        {
            CalMeasureInfos.Save();
            MessageBox.Show(App.GetString("SaveOkMsg"));
        }
    }
}
