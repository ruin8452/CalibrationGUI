using CalibrationNewGUI.Model;
using GalaSoft.MvvmLight.Command;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Windows;

namespace CalibrationNewGUI.ViewModel.Setting
{
    [ImplementPropertyChanged]
    class OthersSettingVM
    {
        public OthersInfo OthersInfos { get; set; }

        public ObservableCollection<string> LanguageList { get; set; }

        public RelayCommand SaveClick { get; set; }

        public OthersSettingVM()
        {
            OthersInfos = OthersInfo.GetObj();
            LanguageList = new ObservableCollection<string>(new string[] { "한국어", "English" });

            SaveClick = new RelayCommand(DataSave);
        }

        public void DataSave()
        {
            OthersInfos.Save();
            MessageBox.Show("저장 완료");
        }
    }
}
