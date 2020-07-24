using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.Model
{
    public class ShuntInfo
    {
        public int ShuntStandardCurr { get; set; } //기준 전류(mA)
        public int ShuntStandardReg { get; set; } //기준 저항(uOhm)
        public int ShuntNewCurr { get; set; } //새로 연결하는 션트 기준 전류(mA)
        public int ShuntNewReg { get; set; } //새로 연결하는 션트 기준 저항(uOhm)
        public int StandardVoltMeter { get; set; } //기준 전압미터
        public int NewVoltMeter { get; set; } //보정 전압미터

        ConfigFileSave ConfigFile = new ConfigFileSave();

        #region 싱글톤 패턴 구현
        private static ShuntInfo SingleTonObj = null;

        private ShuntInfo()
        {
        }

        public static ShuntInfo GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new ShuntInfo();
            return SingleTonObj;
        }
        #endregion 싱글톤 패턴 구현

        public void Save()
        {
            ConfigFile.Write("Shunt", "ShuntStandardCurr", ShuntStandardCurr.ToString());
            ConfigFile.Write("Shunt", "ShuntStandardReg", ShuntStandardReg.ToString());
            ConfigFile.Write("Shunt", "ShuntNewCurr", ShuntNewCurr.ToString());
            ConfigFile.Write("Shunt", "ShuntNewReg", ShuntNewReg.ToString());
            ConfigFile.Write("Shunt", "StandardVoltMeter", StandardVoltMeter.ToString());
            ConfigFile.Write("Shunt", "NewVoltMeter", NewVoltMeter.ToString());
        }

        public void Load()
        {
            ShuntStandardCurr = Convert.ToInt32(ConfigFile.Read("Shunt", "ShuntStandardCurr", "50000"));
            ShuntStandardReg = Convert.ToInt32(ConfigFile.Read("Shunt", "ShuntStandardReg", "0"));
            ShuntNewCurr = Convert.ToInt32(ConfigFile.Read("Shunt", "ShuntNewCurr", "50000"));
            ShuntNewReg = Convert.ToInt32(ConfigFile.Read("Shunt", "ShuntNewReg", "0"));
            StandardVoltMeter = Convert.ToInt32(ConfigFile.Read("Shunt", "StandardVoltMeter", "4200"));
            NewVoltMeter = Convert.ToInt32(ConfigFile.Read("Shunt", "NewVoltMeter", "4200"));
        }
    }
}
