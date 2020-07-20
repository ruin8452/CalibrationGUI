using Microsoft.VisualStudio.TestTools.UnitTesting;
using CalibrationNewGUI.Equipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationNewGUI.Equipment.Tests
{
    [TestClass()]
    public class McuTests
    {
        [TestMethod()]
        public void ChSetDataTest()
        {
            Mcu mcu = Mcu.GetObj();

            mcu.ChSet('V', 1, 4200, -1000);

            Assert.Fail();
        }

        [TestMethod()]
        public void ChCalTest()
        {
            Mcu mcu = Mcu.GetObj();

            mcu.ChCal('I', 1, -10000);

            Assert.Fail();
        }
    }
}