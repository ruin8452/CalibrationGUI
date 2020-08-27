using CalibrationNewGUI.Model;
using GalaSoft.MvvmLight.Command;
using PropertyChanged;
using System;
using System.Windows;

namespace CalibrationNewGUI.ViewModel.Setting
{
    [ImplementPropertyChanged]
    public class AutoSaveSettingVM
    {
        public AutoSaveInfo AutoSaveInfos { get; set; }

        public RelayCommand SaveClick { get; set; }

        public AutoSaveSettingVM()
        {
            AutoSaveInfos = AutoSaveInfo.GetObj();

            SaveClick = new RelayCommand(DataSave);
        }

        public void DataSave()
        {
            AutoSaveInfos.Save();
            MessageBox.Show(App.GetString("SaveOkMsg"));
        }
    }
}
