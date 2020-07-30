using CalibrationNewGUI.Model;
using GalaSoft.MvvmLight.Command;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows;

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

        public RelayCommand SaveClick { get; set; }
        public RelayCommand ComboDrop { get; set; }

        public CommSettingVM()
        {
            McuInfos = McuInfo.GetObj();
            DmmInfos = DmmInfo.GetObj();

            PortList = new ObservableCollection<string>(SerialPort.GetPortNames());
            BorateList = new ObservableCollection<int>(new int[] { 115200, 57600, 38400, 19200, 9600});
            DataBitList = new ObservableCollection<int>(new int[] { 8 });
            StopBitList = new ObservableCollection<int>(new int[] { 1 });
            FlowCtrlList = new ObservableCollection<string>(new string[] { "NONE" });

            DmmModelList = new ObservableCollection<string>(new string[] { "34401A", "34450A", "Keithley2000" });

            SaveClick = new RelayCommand(DataSave);
            ComboDrop = new RelayCommand(IdRenuwal);
        }

        private void DataSave()
        {
            McuInfos.Save();
            DmmInfos.Save();
            MessageBox.Show("저장 완료");
        }

        private void IdRenuwal()
        {
            PortList.Clear();

            foreach(var item in SerialPort.GetPortNames())
                PortList.Add(item);
        }
    }
}
