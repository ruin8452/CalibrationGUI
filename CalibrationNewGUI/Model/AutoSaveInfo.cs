using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.Model
{
    public class AutoSaveInfo
    {
        public string AutoSavePrifix { get; set; }
        public bool AutoSaveFlag { get; set; }

        ConfigFileSave ConfigFile = new ConfigFileSave();

        #region 싱글톤 패턴 구현
        private static AutoSaveInfo SingleTonObj = null;

        private AutoSaveInfo()
        {
        }

        public static AutoSaveInfo GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new AutoSaveInfo();
            return SingleTonObj;
        }
        #endregion 싱글톤 패턴 구현

        public void Save()
        {
            ConfigFile.Write("AutoSave", "AutoSavePrifix", AutoSavePrifix);
            ConfigFile.Write("AutoSave", "AutoSaveFlag", AutoSaveFlag.ToString());
        }

        public void Load()
        {
            AutoSavePrifix = ConfigFile.Read("AutoSave", "AutoSavePrifix", "AUTO_SAVE");
            AutoSaveFlag = Convert.ToBoolean(ConfigFile.Read("AutoSave", "AutoSaveFlag", "false"));
        }
    }
}
