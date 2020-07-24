using CalibrationNewGUI.Model;
using J_Project.ViewModel.CommandClass;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows.Input;

namespace CalibrationNewGUI.ViewModel.Setting
{
    [ImplementPropertyChanged]
    public class CommSettingVM
    {
        public McuInfo McuInfos { get; set; }
        public DmmInfo DmmInfos { get; set; }

        public ObservableCollection<string> PortList { get; set; }
        public ObservableCollection<int> BorateList { get; set; }
        public ObservableCollection<int> DataBitList { get; set; }
        public ObservableCollection<int> StopBitList { get; set; }
        public ObservableCollection<string> FlowCtrlList { get; set; }
        public ObservableCollection<string> DmmModelList { get; set; }

        public ICommand SaveClick { get; set; }
        public ICommand ComboDrop { get; set; }

        public CommSettingVM()
        {
            McuInfos = McuInfo.GetObj();
            DmmInfos = DmmInfo.GetObj();

            PortList = new ObservableCollection<string>(SerialPort.GetPortNames());
            BorateList = new ObservableCollection<int>(new int[] { 115200, 57600, 38400, 9600});
            DataBitList = new ObservableCollection<int>(new int[] { 8 });
            StopBitList = new ObservableCollection<int>(new int[] { 1 });
            FlowCtrlList = new ObservableCollection<string>(new string[] { "NONE" });

            DmmModelList = new ObservableCollection<string>(new string[] { "34401A", "34450A", "Keithley2000" });

            SaveClick = new BaseCommand(DataSave);
            ComboDrop = new BaseCommand(IdRenuwal);
        }

        public void DataSave()
        {
            McuInfos.Save();
            DmmInfos.Save();
        }

        public void IdRenuwal()
        {
            PortList = new ObservableCollection<string>(SerialPort.GetPortNames());
        }
    }
}
