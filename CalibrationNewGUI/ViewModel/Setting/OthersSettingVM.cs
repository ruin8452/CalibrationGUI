using CalibrationNewGUI.Globalization;
using CalibrationNewGUI.Model;
using GalaSoft.MvvmLight.Command;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Resources;
using System.Windows;

namespace CalibrationNewGUI.ViewModel.Setting
{
    [ImplementPropertyChanged]
    class OthersSettingVM
    {
        private string preLang;
        public OthersInfo OthersInfos { get; set; }

        public ObservableCollection<string> LanguageList { get; set; }

        public RelayCommand SaveClick { get; set; }

        public OthersSettingVM()
        {
            OthersInfos = OthersInfo.GetObj();
            preLang = OthersInfos.Language;

            LanguageList = new ObservableCollection<string>(new string[] { "한국어", "English" });

            SaveClick = new RelayCommand(DataSave);
        }

        public void DataSave()
        {
            if (preLang != OthersInfos.Language)
            {
                var result = MessageBox.Show(App.GetString("LangChangeMsg"), App.GetString("LangChangeCaption"), MessageBoxButton.YesNo); ;
                if(result == MessageBoxResult.Yes)
                {
                    OthersInfos.Save();
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                }
                else
                {
                    OthersInfos.Language = preLang;
                    OthersInfos.Save();
                }
            }
            MessageBox.Show(App.GetString("SaveOkMsg"));
        }
    }
}
