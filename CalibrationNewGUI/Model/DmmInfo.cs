using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.Model
{
    public class DmmInfo
    {
        public string ModelName { get; set; }
        public string PortName { get; set; }
        public int Borate { get; set; }
        public int DataBit { get; set; }
        public string Parity { get; set; }
        public int StopBit { get; set; }
        public string FlowCtrl { get; set; }
        public bool OffsetUseFlag { get; set; }

        ConfigFileSave ConfigFile = new ConfigFileSave();

        #region 싱글톤 패턴 구현
        private static DmmInfo SingleTonObj = null;

        private DmmInfo()
        {
        }

        public static DmmInfo GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new DmmInfo();
            return SingleTonObj;
        }
        #endregion 싱글톤 패턴 구현

        public void Save()
        {
            ConfigFile.Write("DmmComm", "ModelName", ModelName);
            ConfigFile.Write("DmmComm", "PortName", PortName);
            ConfigFile.Write("DmmComm", "Borate", Borate.ToString());
            ConfigFile.Write("DmmComm", "DataBit", DataBit.ToString());
            ConfigFile.Write("DmmComm", "Parity", Parity);
            ConfigFile.Write("DmmComm", "StopBit", StopBit.ToString());
            ConfigFile.Write("DmmComm", "FlowCtrl", FlowCtrl);
            ConfigFile.Write("DmmComm", "OffsetUseFlag", OffsetUseFlag.ToString());
        }

        public void Load()
        {
            ModelName = ConfigFile.Read("DmmComm", "ModelName", "34450A");
            PortName = ConfigFile.Read("DmmComm", "PortName", "");
            Borate = Convert.ToInt32(ConfigFile.Read("DmmComm", "Borate", "9600"));
            DataBit = Convert.ToInt32(ConfigFile.Read("DmmComm", "DataBit", "8"));
            Parity = ConfigFile.Read("DmmComm", "Parity", "NONE");
            StopBit = Convert.ToInt32(ConfigFile.Read("DmmComm", "StopBit", "1"));
            FlowCtrl = ConfigFile.Read("DmmComm", "FlowCtrl", "NONE");
            OffsetUseFlag = Convert.ToBoolean(ConfigFile.Read("DmmComm", "OffsetUseFlag", "False"));
        }
    }
}
