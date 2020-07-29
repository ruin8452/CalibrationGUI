using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.Model
{
    public class PointInfo
    {
        public int Id { get; set; }
        public int SetVolt { get; set; }
        public int SetCurr { get; set; }
        public int OutVolt { get; set; }
        public int OutCurr { get; set; }
        public double OutDmm { get; set; }
        public bool IsRangeIn { get; set; }

        public PointInfo()
        {

        }

        public PointInfo(int id, int setVolt, int setCurr)
        {
            Id = id;
            SetVolt = setVolt;
            SetCurr = setCurr;
        }

        public PointInfo(int id, int setVolt, int setCurr, int outVolt, int outCurr, double outDmm, bool isRangeIn)
        {
            Id = id;
            SetVolt = setVolt;
            SetCurr = setCurr;
            OutVolt = outVolt;
            OutCurr = outCurr;
            OutDmm = outDmm;
            IsRangeIn = isRangeIn;
        }
    }
}
