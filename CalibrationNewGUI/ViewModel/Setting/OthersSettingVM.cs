using CalibrationNewGUI.Model;
using J_Project.ViewModel.CommandClass;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CalibrationNewGUI.ViewModel.Setting
{
    [ImplementPropertyChanged]
    class OthersSettingVM
    {
        public OthersInfo OthersInfos { get; set; }

        public ObservableCollection<string> LanguageList { get; set; }

        public ICommand SaveClick { get; set; }

        public OthersSettingVM()
        {
            OthersInfos = OthersInfo.GetObj();
            LanguageList = new ObservableCollection<string>(new string[] { "한국어", "English" });

            SaveClick = new BaseCommand(DataSave);
        }

        public void DataSave()
        {
            OthersInfos.Save();
        }
    }
}
