using Microsoft.VisualStudio.TestTools.UnitTesting;
using CalibrationNewGUI.Equipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J_Project.Manager;

namespace CalibrationNewGUI.Equipment.Tests
{
    [TestClass()]
    public class McuTests
    {
        [TestMethod()]
        public void ChSetTest()
        {
            Mcu mcu = Mcu.GetObj();
            mcu.Connect("COM120", 57600);

            mcu.ChSet(1, 2700, 1000);

            Util.Delay(10);

            Assert.Fail();
        }

        [TestMethod()]
        public void ChCalTest()
        {
            Mcu mcu = Mcu.GetObj();

            mcu.ChCal('I', 1, -10000);

            Assert.Fail();
        }

        [TestMethod()]
        public void ChStopTest()
        {
            Mcu mcu = Mcu.GetObj();
            mcu.Connect("COM120", 57600);

            mcu.ChStop();

            Util.Delay(10);

            Assert.Fail();
        }

        [TestMethod()]
        public void CalSeqTest()
        {
            Mcu mcu = Mcu.GetObj();
            mcu.Connect("COM120", 57600);

            mcu.ChSet(1, 4200, 1000);

            Util.Delay(4);

            mcu.ChCal('V', 1, 42087);

            Util.Delay(4);

            mcu.ChStop();

            Util.Delay(4);

            Assert.Fail();
        }
    }
}