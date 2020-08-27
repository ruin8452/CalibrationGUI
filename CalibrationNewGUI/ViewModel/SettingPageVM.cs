using CalibrationNewGUI.Message;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.ViewModel
{
    [ImplementPropertyChanged]
    public class SettingPageVM : ViewModelBase
    {
        public bool CommRadio { get; set; }
        public bool CalRadio { get; set; }
        public bool ShuntRadio { get; set; }
        public bool SaveRadio { get; set; }
        public bool OtherRadio { get; set; }

        public SettingPageVM()
        {
            Messenger.Default.Register<SettingMoveMessage>(this, SettingMode);
        }

        private void SettingMode(SettingMoveMessage obj)
        {
            if (obj.LangMove)
                OtherRadio = true;
            else if (obj.ShuntMode)
                ShuntRadio = true;
        }
    }
}
