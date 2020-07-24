using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.Model
{
    public class McuInfo
    {
        public string McuSerialNum { get; set; }
        public string McuVersion { get; set; }
        public string PortName { get; set; }
        public int Borate { get; set; }
        public int DataBit { get; set; }
        public string Parity { get; set; }
        public int StopBit { get; set; }
        public string FlowCtrl { get; set; }

        ConfigFileSave ConfigFile = new ConfigFileSave();

        #region 싱글톤 패턴 구현
        private static McuInfo SingleTonObj = null;

        private McuInfo()
        {
        }

        public static McuInfo GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new McuInfo();
            return SingleTonObj;
        }
        #endregion 싱글톤 패턴 구현

        public void Save()
        {
            ConfigFile.Write("McuComm", "PortName", PortName);
            ConfigFile.Write("McuComm", "Borate", Borate.ToString());
            ConfigFile.Write("McuComm", "DataBit", DataBit.ToString());
            ConfigFile.Write("McuComm", "Parity", Parity);
            ConfigFile.Write("McuComm", "StopBit", StopBit.ToString());
            ConfigFile.Write("McuComm", "FlowCtrl", FlowCtrl);
        }

        public void Load()
        {
            PortName = ConfigFile.Read("McuComm", "PortName", "");
            Borate = Convert.ToInt32(ConfigFile.Read("McuComm", "Borate", "57600"));
            DataBit = Convert.ToInt32(ConfigFile.Read("McuComm", "DataBit", "8"));
            Parity = ConfigFile.Read("McuComm", "Parity", "NONE");
            StopBit = Convert.ToInt32(ConfigFile.Read("McuComm", "StopBit", "1"));
            FlowCtrl = ConfigFile.Read("McuComm", "FlowCtrl", "NONE");
        }
    }
}
