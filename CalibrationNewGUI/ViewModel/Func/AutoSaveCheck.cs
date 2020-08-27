using CalibrationNewGUI.FileSystem;
using PropertyChanged;
using System;
using System.IO;
using System.Linq;

namespace CalibrationNewGUI.ViewModel.Func
{
    [ImplementPropertyChanged]
    public class AutoSaveCheck
    {
        public bool? CalCh1VoltFlag { get; set; }
        public bool? CalCh1CurrFlag { get; set; }
        public bool? CalCh2VoltFlag { get; set; }
        public bool? CalCh2CurrFlag { get; set; }

        public bool? MeaCh1VoltFlag { get; set; }
        public bool? MeaCh1CurrFlag { get; set; }
        public bool? MeaCh2VoltFlag { get; set; }
        public bool? MeaCh2CurrFlag { get; set; }

        public void SaveCheck(string filePath, string serialNum)
        {
            CalCh1VoltFlag = null;
            CalCh1CurrFlag = null;
            CalCh2VoltFlag = null;
            CalCh2CurrFlag = null;

            MeaCh1VoltFlag = null;
            MeaCh1CurrFlag = null;
            MeaCh2VoltFlag = null;
            MeaCh2CurrFlag = null;

            if (!File.Exists(filePath))
                return;

            string[] dataSet = CsvFile.Read(filePath);

            var filterData = from data in dataSet
                             where Array.Exists(data.Split(','), exists => exists == serialNum) && data.Contains("PASS")
                             select data.Split(',');

            // 아무것도 없으면 패스
            if (!filterData.Any())
                return;

            foreach(var data in filterData)
            {
                if (Array.Exists(data, exists => exists == "CAL"))
                {
                    if (Array.Exists(data, exists => exists == "1"))
                    {
                        if (Array.Exists(data, exists => exists == "V"))
                            CalCh1VoltFlag = true;
                        else
                            CalCh1VoltFlag = false;

                        if (Array.Exists(data, exists => exists == "I"))
                            CalCh1CurrFlag = true;
                        else
                            CalCh1CurrFlag = false;
                    }
                    else if (Array.Exists(data, exists => exists == "2"))
                    {
                        if (Array.Exists(data, exists => exists == "V"))
                            CalCh2VoltFlag = true;
                        else
                            CalCh2VoltFlag = false;

                        if (Array.Exists(data, exists => exists == "I"))
                            CalCh2CurrFlag = true;
                        else
                            CalCh2CurrFlag = false;
                    }
                }
                else if (Array.Exists(data, exists => exists == "MEA"))
                {
                    if (Array.Exists(data, exists => exists == "1"))
                    {
                        if (Array.Exists(data, exists => exists == "V"))
                            MeaCh1VoltFlag = true;
                        else
                            MeaCh1VoltFlag = false;

                        if (Array.Exists(data, exists => exists == "I"))
                            MeaCh1CurrFlag = true;
                        else
                            MeaCh1CurrFlag = false;
                    }
                    else if (Array.Exists(data, exists => exists == "2"))
                    {
                        if (Array.Exists(data, exists => exists == "V"))
                            MeaCh2VoltFlag = true;
                        else
                            MeaCh2VoltFlag = false;

                        if (Array.Exists(data, exists => exists == "I"))
                            MeaCh2CurrFlag = true;
                        else
                            MeaCh2CurrFlag = false;
                    }
                }
            }
        }
    }
}
