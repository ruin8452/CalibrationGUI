using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.Model
{
    public class CalMeasureInfo
    {
        public int CalErrRangeVolt { get; set; } //CAL 전압 에러 오차값(mV)
        public int CalErrRangeCurr { get; set; } //CAL 전류 에러 오차값(mA)
        public int CalErrRetryCnt { get; set; } //CAL 에러 재측정 횟수
        public int CalDelayTime { get; set; } //CAL 측정 딜레이 시간(ms)

        public int MeaErrRangeVolt { get; set; } //실측 전압 에러 오차값(mV)
        public int MeaErrRangeCurr { get; set; } //실측 전류 에러 오차값(mA)
        public int MeaErrRetryCnt { get; set; } //실측 에러 재측정 횟수
        public int MeaDelayTime { get; set; } //실측 측정 딜레이 시간(ms)

        ConfigFileSave ConfigFile = new ConfigFileSave();

        #region 싱글톤 패턴 구현
        private static CalMeasureInfo SingleTonObj = null;

        private CalMeasureInfo()
        {
        }

        public static CalMeasureInfo GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new CalMeasureInfo();
            return SingleTonObj;
        }
        #endregion 싱글톤 패턴 구현

        public void Save()
        {
            ConfigFile.Write("CalMeasure", "CalErrRangeVolt", CalErrRangeVolt.ToString());
            ConfigFile.Write("CalMeasure", "CalErrRangeCurr", CalErrRangeCurr.ToString());
            ConfigFile.Write("CalMeasure", "CalErrRetryCnt", CalErrRetryCnt.ToString());
            ConfigFile.Write("CalMeasure", "CalDelayTime", CalDelayTime.ToString());

            ConfigFile.Write("CalMeasure", "MeaErrRangeVolt", MeaErrRangeVolt.ToString());
            ConfigFile.Write("CalMeasure", "MeaErrRangeCurr", MeaErrRangeCurr.ToString());
            ConfigFile.Write("CalMeasure", "MeaErrRetryCnt", MeaErrRetryCnt.ToString());
            ConfigFile.Write("CalMeasure", "MeaDelayTime", MeaDelayTime.ToString());
        }

        public void Load()
        {
            CalErrRangeVolt = Convert.ToInt32(ConfigFile.Read("CalMeasure", "CalErrRangeVolt", "2"));
            CalErrRangeCurr = Convert.ToInt32(ConfigFile.Read("CalMeasure", "CalErrRangeCurr", "5"));
            CalErrRetryCnt = Convert.ToInt32(ConfigFile.Read("CalMeasure", "CalErrRetryCnt", "3"));
            CalDelayTime = Convert.ToInt32(ConfigFile.Read("CalMeasure", "CalDelayTime", "2000"));

            MeaErrRangeVolt = Convert.ToInt32(ConfigFile.Read("CalMeasure", "MeaErrRangeVolt", "2"));
            MeaErrRangeCurr = Convert.ToInt32(ConfigFile.Read("CalMeasure", "MeaErrRangeCurr", "5"));
            MeaErrRetryCnt = Convert.ToInt32(ConfigFile.Read("CalMeasure", "MeaErrRetryCnt", "3"));
            MeaDelayTime = Convert.ToInt32(ConfigFile.Read("CalMeasure", "MeaDelayTime", "2000"));
        }
    }
}
