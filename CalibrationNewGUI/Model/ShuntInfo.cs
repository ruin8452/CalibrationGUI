using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.Model
{
    public class ShuntInfo
    {
        public bool CorrectionMode { get; set; }    // 교정 모드(T:전류, F:저항)
        public int ShuntStandardCurr { get; set; }  // 기준 션트 전류(mA)
        public int ShuntNewCurr { get; set; }       // 교정 션트 전류(mA)
        public float ShuntNewReg { get; set; }        // 교정 션트 저항(uOhm)
        public int StandardVoltMeter { get; set; }  // 기준 전압미터
        public int NewVoltMeter { get; set; }       // 보정 전압미터
        public float shuntReg { get; set; }         // 션트 보정값과 표준값을 사용한 실제 보정치(보정치 = 표준/보정)

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
            ConfigFile.Write("Shunt", "CorrectionMode", CorrectionMode.ToString());
            ConfigFile.Write("Shunt", "ShuntStandardCurr", ShuntStandardCurr.ToString());
            ConfigFile.Write("Shunt", "ShuntNewCurr", ShuntNewCurr.ToString());
            ConfigFile.Write("Shunt", "ShuntNewReg", ShuntNewReg.ToString());
            ConfigFile.Write("Shunt", "StandardVoltMeter", StandardVoltMeter.ToString());
            ConfigFile.Write("Shunt", "NewVoltMeter", NewVoltMeter.ToString());
        }

        public void Load()
        {
            CorrectionMode = Convert.ToBoolean(ConfigFile.Read("Shunt", "CorrectionMode", "true"));
            ShuntStandardCurr = Convert.ToInt32(ConfigFile.Read("Shunt", "ShuntStandardCurr", "50000"));
            ShuntNewCurr = Convert.ToInt32(ConfigFile.Read("Shunt", "ShuntNewCurr", "50000"));
            ShuntNewReg = (float)Convert.ToDouble(ConfigFile.Read("Shunt", "ShuntNewReg", "0"));
            StandardVoltMeter = Convert.ToInt32(ConfigFile.Read("Shunt", "StandardVoltMeter", "4200"));
            NewVoltMeter = Convert.ToInt32(ConfigFile.Read("Shunt", "NewVoltMeter", "4200"));
        }
    }
}
