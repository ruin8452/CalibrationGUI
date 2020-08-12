using CalibrationNewGUI.FileSystem;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.ViewModel.Func
{
    [ImplementPropertyChanged]
    public class AutoSaveCheck
    {
        public bool CalCh1VoltFlag { get; set; }
        public bool CalCh1CurrFlag { get; set; }
        public bool CalCh2VoltFlag { get; set; }
        public bool CalCh2CurrFlag { get; set; }

        public bool MeaCh1VoltFlag { get; set; }
        public bool MeaCh1CurrFlag { get; set; }
        public bool MeaCh2VoltFlag { get; set; }
        public bool MeaCh2CurrFlag { get; set; }

        public void SaveCheck(string filePath, string serialNum)
        {
            CalCh1VoltFlag = false;
            CalCh1CurrFlag = false;
            CalCh2VoltFlag = false;
            CalCh2CurrFlag = false;

            MeaCh1VoltFlag = false;
            MeaCh1CurrFlag = false;
            MeaCh2VoltFlag = false;
            MeaCh2CurrFlag = false;

            string[] dataSet = CsvFile.Read(filePath);

            var filterData = from data in dataSet
                             where Array.Exists(data.Split(','), exists => exists == serialNum) && data.Contains("PASS")
                             select data.Split(',');

            // 아무것도 없으면 패스
            if (!filterData.Any())
                return;

            foreach(var a in filterData)
            {
                if (Array.Exists(a, exists => exists == "CAL"))
                {
                    if (Array.Exists(a, exists => exists == "1"))
                    {
                        if (Array.Exists(a, exists => exists == "V"))
                            CalCh1VoltFlag = true;
                        else if (Array.Exists(a, exists => exists == "I"))
                            CalCh1CurrFlag = true;
                    }
                    else if (Array.Exists(a, exists => exists == "2"))
                    {
                        if (Array.Exists(a, exists => exists == "V"))
                            CalCh2VoltFlag = true;
                        else if (Array.Exists(a, exists => exists == "I"))
                            CalCh2CurrFlag = true;
                    }
                }
                else if (Array.Exists(a, exists => exists == "MEA"))
                {
                    if (Array.Exists(a, exists => exists == "1"))
                    {
                        if (Array.Exists(a, exists => exists == "V"))
                            MeaCh1VoltFlag = true;
                        else if (Array.Exists(a, exists => exists == "I"))
                            MeaCh1CurrFlag = true;
                    }
                    else if (Array.Exists(a, exists => exists == "2"))
                    {
                        if (Array.Exists(a, exists => exists == "V"))
                            MeaCh2VoltFlag = true;
                        else if (Array.Exists(a, exists => exists == "I"))
                            MeaCh2CurrFlag = true;
                    }
                }
            }
        }
    }
}
