using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.Model
{
    [ImplementPropertyChanged]
    public class OthersInfo
    {
        public int InputVoltMax { get; set; } //입력 전압 범위(mV) 최대값
        public int InputVoltMin { get; set; } //입력 전압 범위(mV) 최소값
        public int InputCurrMax { get; set; } //입력 전류 범위(mA) 최대값
        public int InputCurrMin { get; set; } //입력 전류 범위(mA) 최소값
        public string Language  { get; set; } //현재 언어 설정값

        ConfigFileSave ConfigFile = new ConfigFileSave();

        #region 싱글톤 패턴 구현
        private static OthersInfo SingleTonObj = null;

        private OthersInfo()
        {
        }

        public static OthersInfo GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new OthersInfo();
            return SingleTonObj;
        }
        #endregion 싱글톤 패턴 구현

        public void Save()
        {
            ConfigFile.Write("Others", "InputVoltMax", InputVoltMax.ToString());
            ConfigFile.Write("Others", "InputVoltMin", InputVoltMin.ToString());
            ConfigFile.Write("Others", "InputCurrMax", InputCurrMax.ToString());
            ConfigFile.Write("Others", "InputCurrMin", InputCurrMin.ToString());
            ConfigFile.Write("Others", "Language", Language);
        }

        public void Load()
        {
            InputVoltMax = Convert.ToInt32(ConfigFile.Read("Others", "InputVoltMax", "4200"));
            InputVoltMin = Convert.ToInt32(ConfigFile.Read("Others", "InputVoltMin", "2700"));
            InputCurrMax = Convert.ToInt32(ConfigFile.Read("Others", "InputCurrMax", "40000"));
            InputCurrMin = Convert.ToInt32(ConfigFile.Read("Others", "InputCurrMin", "-40000"));
            Language = ConfigFile.Read("Others", "Language", "한국어");
        }
    }
}
