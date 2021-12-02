import time
from unittest import TestCase
from driver.drvPgva import PGVA


# unit testing
class Tests(TestCase):

    # initialize the PGVA
    def init(self, face):
        self.pgva = PGVA(interface=face, comPort="COM3", tcpPort=8502, host="192.168.0.199", baudrate=115200)

    # test connecting over tcp/ip (ethernet)
    def test_tcp_connection(self):
        self.init("tcp/ip")
        self.assertEqual(True, self.pgva.client.is_socket_open())
        self.pgva.disconnect()

    # test connecting over serial
    def test_serial_connection(self):
        self.init("serial")
        self.assertEqual(True, self.pgva.client.is_socket_open())
        self.pgva.disconnect()

    # test dispense(ms, mBar)
    def test_dispense(self):
        self.init("serial")
        self.pgva.dispense(100, 0)
        time.sleep(0.2)
        self.assertAlmostEqual(0, self.pgva.readSensData()['outputPressure'], delta=10)
        self.pgva.dispense(100, 100)
        time.sleep(0.2)
        self.assertAlmostEqual(100, self.pgva.readSensData()['outputPressure'], delta=10)
        self.pgva.dispense(100, 400)
        time.sleep(0.2)
        self.assertAlmostEqual(400, self.pgva.readSensData()['outputPressure'], delta=10)
        self.pgva.disconnect()

    # test dispense pressure value argument exceptions
    def test_dispense_pressure_error(self):
        self.init("serial")
        self.assertRaisesRegex(ValueError, "Pressure value not in range", self.pgva.dispense, 100, -1)
        self.assertRaisesRegex(ValueError, "Pressure value not in range", self.pgva.dispense, 100, 451)
        self.pgva.disconnect()

    # test dispense actuation time argument exceptions
    def test_dispense_actuate_error(self):
        self.init("serial")
        self.assertRaisesRegex(ValueError, "Actuation time not in range", self.pgva.dispense, -1, 0)
        self.assertRaisesRegex(ValueError, "Actuation time not in range", self.pgva.dispense, 1001, 0)
        self.pgva.disconnect()

    # test aspirate(ms, mBar)
    def test_aspirate(self):
        self.init("serial")
        self.pgva.aspirate(500, 0)
        time.sleep(0.2)
        self.assertAlmostEqual(0, self.pgva.readSensData()['outputPressure'], delta=10)
        self.pgva.aspirate(500, -100)
        time.sleep(0.2)
        self.assertAlmostEqual(-100, self.pgva.readSensData()['outputPressure'], delta=10)
        self.pgva.aspirate(500, -400)
        time.sleep(0.2)
        self.assertAlmostEqual(-400, self.pgva.readSensData()['outputPressure'], delta=10)
        self.pgva.disconnect()

    # test aspirate pressure value argument exceptions
    def test_aspirate_pressure_error(self):
        self.init("serial")
        self.assertRaisesRegex(ValueError, "Pressure value not in range", self.pgva.aspirate, 100, 1)
        self.assertRaisesRegex(ValueError, "Pressure value not in range", self.pgva.aspirate, 100, -451)
        self.pgva.disconnect()

    # test aspirate actuation time argument exceptions
    def test_aspirate_actuate_error(self):
        self.init("serial")
        self.assertRaisesRegex(ValueError, "Actuation time not in range", self.pgva.aspirate, -1, 0)
        self.assertRaisesRegex(ValueError, "Actuation time not in range", self.pgva.aspirate, 1001, 0)
        self.pgva.disconnect()

    # test readSensData()
    def test_readSensData(self):
        self.init("serial")
        self.assertAlmostEqual(-600, self.pgva.readSensData()['vacuumChamber'], delta=50)
        self.assertAlmostEqual(600, self.pgva.readSensData()['pressureChamber'], delta=50)
        self.pgva.dispense(100, 100)
        self.assertAlmostEqual(100, self.pgva.readSensData()['outputPressure'], delta=10)
        self.pgva.aspirate(100, -100)
        self.assertAlmostEqual(-100, self.pgva.readSensData()['outputPressure'], delta=10)
        self.pgva.disconnect()

    # test setOutputPressure()
    def test_setOutputPressure(self):
        self.init("serial")
        self.pgva.setOutputPressure(100)
        self.assertAlmostEqual(100, self.pgva.readSensData()['outputPressure'], delta=10)
        self.pgva.setOutputPressure(0)
        self.assertAlmostEqual(0, self.pgva.readSensData()['outputPressure'], delta=10)
        self.pgva.setOutputPressure(-100)
        self.assertAlmostEqual(-100, self.pgva.readSensData()['outputPressure'], delta=10)
        self.pgva.disconnect()

    # test setOutputPressure argument exceptions
    def test_setOutputPressure_error(self):
        self.init("serial")
        self.assertRaisesRegex(ValueError, "Pressure value not in range", self.pgva.setOutputPressure, 451)
        self.assertRaisesRegex(ValueError, "Pressure value not in range", self.pgva.setOutputPressure, -451)
        self.pgva.disconnect()

    # actuateValve(ms) tested qualitatively using example script

    # test actuateValve argument exceptions
    def test_actuateValve_error(self):
        self.init("serial")
        self.assertRaisesRegex(ValueError, "Actuation time not in range", self.pgva.actuateValve, -1)
        self.assertRaisesRegex(ValueError, "Actuation time not in range", self.pgva.actuateValve, 1001)
        self.pgva.disconnect()
