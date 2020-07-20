using CalibrationNewGUI.Model;
using J_Project.ViewModel.CommandClass;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CalibrationNewGUI.VeiwModel.Setting
{
    [ImplementPropertyChanged]
    public class AutoSaveSettingVM
    {
        public AutoSaveInfo AutoSaveInfos { get; set; }

        public ICommand SaveClick { get; set; }

        public AutoSaveSettingVM()
        {
            AutoSaveInfos = AutoSaveInfo.GetObj();

            SaveClick = new BaseCommand(DataSave);
        }

        public void DataSave()
        {
            AutoSaveInfos.Save();
        }
    }
}
