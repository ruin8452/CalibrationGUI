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
    public class ShuntSettingVM
    {
        public ShuntInfo ShuntInfos { get; set; }

        public ICommand SaveClick { get; set; }

        public ShuntSettingVM()
        {
            ShuntInfos = ShuntInfo.GetObj();

            SaveClick = new BaseCommand(DataSave);
        }

        public void DataSave()
        {
            ShuntInfos.Save();
        }
    }
}
