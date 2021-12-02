__author__ = "Kolev, Milen"
__copyright__ = "Copyright 2021, Festo Life Tech"
__credits__ = [""]
__license__ = "Apache"
__version__ = "1.0.0"
__maintainer__ = "Kolev, Milen"
__email__ = "milen.kolev@festo.com"
__status__ = "Development"

from pymodbus.client.sync import ModbusTcpClient as TcpClient
from pymodbus.client.sync import ModbusSerialClient as SerialClient
import time


class _ModbusCommands:
    # input registers
    VacuumActual = 256
    PressureActual = 257
    OutputPressureActual = 258
    FirmwareVer = 259
    FirmwareSubVer = 260
    FirmwareBuild = 261
    StatusWord = 262
    DispenseValveOpenTimeH = 263
    DispenseValveOpenTimeL = 264
    PumpLifeCntrH = 265
    PumpLifeCntrL = 266
    PGVALifeCntrH = 267
    PGVALifeCntrL = 268
    VacuumActualmBar = 269
    PressureActualmBar = 270
    OutputPressureActualmBar = 271
    MinDacIncr = 272
    ZeroDacIncr = 273
    MaxDacIncr = 274
    LastModbusErr = 275
    Seed = 276
    ManMode = 277
    # holding registers
    ValveActuationTime = 4096
    VacuumThreshold = 4097
    PressureThreshold = 4098
    OutputPressure = 4099
    MinCalibrSetP = 4100
    ZeroCalibrSetP = 4101
    MaxCalibrSetP = 4102
    TcpPort = 4108
    UnitId = 4109
    VacuumThresholdmBar = 4110
    PressureThresholdmBar = 4111
    OutputPressuremBar = 4112
    ManualTrigger = 4113
    DhcpSelect = 4115
    StoreEeprom = 4196
    ResetPumpLifeCntr = 4197
    StartPreCalibration = 4198
    AdjustCalibrSetPoints = 4199
    AuthStart = 4296
    KeyWrite = 4297
    ConfigOngoing = 4298
    # multiple holding registers
    IpAddressMSH = 12288
    IpAddressLSH = 12289
    GWAdressMSH = 12290
    GWAdressLSH = 12291
    NetMaskMSH = 12292
    NetMaskLSH = 12293
    MacAddressA = 12294
    MacAddressB = 12295
    MacAddressC = 12296
    ProductionYear = 12297
    ProductionDate = 12298


# PGVA driver
class PGVA:
    # constructor
    def __init__(self, interface, comPort, tcpPort, host, baudrate):
        self.sensorData = {'vaccumChamber': 0, 'pressureChamber': 0, 'outputPressure': 0}
        self.pgvaConfig = {
            "interface": interface,
            "comPort": comPort,
            "tcpPort": tcpPort,
            "ip": host,
            "baudrate": baudrate,
            "modbusSlave": 16
        }
        try:
            if self.pgvaConfig["interface"] == 'serial':
                self.client = SerialClient(method="ascii", port=self.pgvaConfig['comPort'],
                                           baudrate=self.pgvaConfig['baudrate'])
            elif self.pgvaConfig['interface'] == "tcp/ip":
                self.client = TcpClient(host=self.pgvaConfig["ip"], port=self.pgvaConfig['tcpPort'])

            self.client.connect()
        except Exception as e:
            print("Incorrect Modbus configuration : ", str(e))

    # disconnect from the PGVA
    def disconnect(self):
        self.client.close();

    # read a frame over modbus
    def readData(self, register, sign=True):
        data = 0
        try:
            data = self.client.read_input_registers(register, 1, unit=self.pgvaConfig['modbusSlave'])
            if data.registers[0] > ((2 ** 15) - 1):
                data.registers[0] = data.registers[0] - 2 ** 16
            return data.registers[0]
        except Exception as e:
            print("Error while reading : ", str(e))

    # write a frame over modbus
    def writeData(self, register, val, sign=True):
        status = object
        print(f"{register}, {val}")
        try:
            if val < 0:
                val = val + 2 ** 16
            self.client.write_register(register, val, unit=self.pgvaConfig['modbusSlave'])
            status = self.client.read_input_registers(_ModbusCommands.StatusWord, count=1,
                                                      unit=self.pgvaConfig['modbusSlave'])
            while (status.registers[0] & 1) == 1:
                status = self.client.read_input_registers(_ModbusCommands.StatusWord, 1,
                                                          unit=self.pgvaConfig['modbusSlave'])
        except Exception as e:
            print("error while writing : ", str(e))

    # aspirate at the given vacuum for the given amount of time
    def aspirate(self, actuationTime: int, pressure: int):
        # set output pressure
        if pressure in range(-450, 1):
            self.setOutputPressure(pressure)
        else:
            raise ValueError("Pressure value not in range")
        time.sleep(0.5)
        # set actuation time
        if actuationTime in range(0, 1001):
            self.actuateValve(actuationTime)
            time.sleep(actuationTime / 1000)
        else:
            raise ValueError("Actuation time not in range")

    # dispense at the given pressure for the given amount of time
    def dispense(self, actuationTime: int, pressure: int):
        # convert from uL to actuation time
        if pressure in range(0, 451):
            self.setOutputPressure(pressure)
        else:
            raise ValueError("Pressure value not in range")
        time.sleep(0.5)
        # set actuation time
        if actuationTime in range(0, 1001):
            self.actuateValve(actuationTime)
            time.sleep(actuationTime / 1000)
        else:
            raise ValueError("Actuation time not in range")

    # read the vacuum chamber, pressure chamber, and output pressure values from the device
    def readSensData(self):
        self.sensorData['vacuumChamber'] = self.readData(_ModbusCommands.VacuumActualmBar, True)
        self.sensorData['pressureChamber'] = self.readData(_ModbusCommands.PressureActualmBar, True)
        self.sensorData['outputPressure'] = self.readData(_ModbusCommands.OutputPressureActualmBar, True)
        return self.sensorData

    # set the output pressure in mBar
    def setOutputPressure(self, pressure: int):
        if pressure in range(-450, 451):
            self.writeData(_ModbusCommands.OutputPressuremBar, pressure)
            time.sleep(0.5)
        else:
            raise ValueError("Pressure value not in range")

    # set the actuation time in milliseconds
    def actuateValve(self, actuationTime: int):
        if actuationTime in range(0, 1001):
            self.writeData(_ModbusCommands.ValveActuationTime, actuationTime, sign=False)
            time.sleep(actuationTime / 1000)
        else:
            raise ValueError("Actuation time not in range")
