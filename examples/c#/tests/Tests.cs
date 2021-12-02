/* Author:     Raines, Jared
 * Copyright:  Copyright 2021, Festo Life Tech
 * Version:    1.0.0
 * Maintainer: Raines, Jared
 * Email:      jared.raines@festo.com
 * Status:     Development
 */
using System;
using System.Threading;
using NUnit.Framework;
using PgvaDriver.driver;

// unit testing
namespace Tests
{
    [TestFixture]
    public class Tests
    {
        private IPgvaDriver pgva;
        
        // initialize the PGVA
        private void Init(String face)
        {
            pgva = new PgvaDriver.driver.PgvaDriver(face, "COM3", 8502, "192.168.0.199", 115200, 16);
        }

        // test connecting over tcp/ip (ethernet)
        [Test]
        public void TestTcpConnection()
        {
            Init("tcp");
        }

        // test connecting over serial
        [Test]
        public void TestSerialConnection()
        {
            Init("serial");
        }

        // test dispense(ms, mBar)
        [Test]
        public void TestDispense()
        {  
            Init("serial");
            pgva.Dispense(100, 0);
            Assert.AreEqual(0, pgva.ReadSensorData()[2], 10);
            pgva.Dispense(100, 100);
            Thread.Sleep(200);
            Assert.AreEqual(100, pgva.ReadSensorData()[2], 10);
            pgva.Dispense(100, 400);
            Thread.Sleep(200);
            Assert.AreEqual(400, pgva.ReadSensorData()[2], 10);
            pgva.Disconnect();
        }
        
        // test dispense pressure value argument exceptions
        [Test]
        public void TestDispensePressureError()
        {
            Init("serial");
            Assert.Throws<ArgumentException>(delegate { pgva.Dispense(100, -1);});
            Assert.Throws<ArgumentException>(delegate { pgva.Dispense(100, 451);});
            pgva.Disconnect();
        }
        
        // test dispense actuation time argument exceptions
        [Test]
        public void TestDispenseActuateError()
        {
            Init("serial");
            Assert.Throws<ArgumentException>(delegate { pgva.Dispense(-1, 0);});
            Assert.Throws<ArgumentException>(delegate { pgva.Dispense(1001, 550);});
            pgva.Disconnect();
        }
        
        // test aspirate(ms, mBar)
        [Test]
        public void TestAspirate()
        {
            Init("serial");
            pgva.Aspirate(100, 0);
            Thread.Sleep(200);
            Assert.AreEqual(0, pgva.ReadSensorData()[2], 10);
            pgva.Aspirate(100, -100);
            Thread.Sleep(200);
            Assert.AreEqual(-100, pgva.ReadSensorData()[2], 10);
            pgva.Aspirate(100, -400);
            Thread.Sleep(200);
            Assert.AreEqual(-400, pgva.ReadSensorData()[2], 10);
            pgva.Disconnect();
        }

        // test aspirate pressure value argument exceptions
        [Test]
        public void TestAspiratePressureError()
        {
            Init("serial");
            Assert.Throws<ArgumentException>(delegate { pgva.Aspirate(100, 1);});
            Assert.Throws<ArgumentException>(delegate { pgva.Aspirate(100, -451);});
            pgva.Disconnect();
        }

        // test aspirate actuation time argument exceptions
        [Test]
        public void TestAspirateActuateError()
        {
            Init("serial");
            Assert.Throws<ArgumentException>(delegate { pgva.Aspirate(-1, 0);});
            Assert.Throws<ArgumentException>(delegate { pgva.Aspirate(1001, -550);});
            pgva.Disconnect();
        }
        
        // test ReadSensorData()
        [Test]
        public void TestReadSensorData()
        {
            Init("serial");
            Assert.AreEqual(-600, pgva.ReadSensorData()[0], 75);
            Assert.AreEqual(600, pgva.ReadSensorData()[1], 75);
            pgva.Dispense(100, 100);
            Assert.AreEqual(100, pgva.ReadSensorData()[2], 10);
            pgva.Aspirate(100, -100);
            Assert.AreEqual(-100, pgva.ReadSensorData()[2], 10);
            pgva.Disconnect();
        }

        // test SetOutputPressure()
        [Test]
        public void TestSetOutputPressure()
        {
            Init("serial");
            pgva.SetOutputPressure(100);
            Assert.AreEqual(100, pgva.ReadSensorData()[2], 10);
            pgva.SetOutputPressure(0);
            Assert.AreEqual(0, pgva.ReadSensorData()[2], 10);
            pgva.SetOutputPressure(-100);
            Assert.AreEqual(-100, pgva.ReadSensorData()[2], 10);
            pgva.Disconnect();
        }

        // test SetOutputPressure argument exceptions
        [Test]
        public void TestSetOutputPressureError()
        {
            Init("serial");
            Assert.Throws<ArgumentException>(delegate { pgva.SetOutputPressure(451);});
            Assert.Throws<ArgumentException>(delegate { pgva.SetOutputPressure(-451);});
            pgva.Disconnect();
        }
        
        // actuateValve(ms) tested quantitatively using example script

        // test ActuateValve argument exceptions
        [Test]
        public void TestActuateValveError()
        {
            Init("serial");
            Assert.Throws<ArgumentException>(delegate { pgva.ActuateValve(-1);});
            Assert.Throws<ArgumentException>(delegate { pgva.ActuateValve(1001);});
            pgva.Disconnect();
        }
    }
}