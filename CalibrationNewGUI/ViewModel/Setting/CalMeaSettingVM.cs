using CalibrationNewGUI.Model;
using J_Project.ViewModel.CommandClass;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CalibrationNewGUI.ViewModel.Setting
{
    [ImplementPropertyChanged]
    public class CalMeaSettingVM
    {
        public CalMeasureInfo CalMeasureInfos { get; set; }

        public ICommand SaveClick { get; set; }

        public CalMeaSettingVM()
        {
            CalMeasureInfos = CalMeasureInfo.GetObj();

            SaveClick = new BaseCommand(DataSave);
        }

        public void DataSave()
        {
            CalMeasureInfos.Save();
        }
    }
}
