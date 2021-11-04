__author__ = "Kolev, Milen"
__copyright__ = "Copyright 2021, Festo Life Tech"
__credits__ = [""]
__license__ = "Apache"
__version__ = "0.0.1"
__maintainer__ = "Kolev, Milen"
__email__ = "milen.kolev@festo.com"
__status__ = "Development"


from pymodbus.client.sync import ModbusTcpClient as TcpClient
from pymodbus.client.sync import ModbusSerialClient as SerialClient
import time

class _ModbusCommands:    
#input registers
    VacuumActual=256
    PressureActual=257
    OutputPressureActual=258
    FirmwareVer=259
    FirmwareSubVer=260
    FirmwareBuild=261
    StatusWord=262
    DispenseValveOpenTimeH=263
    DispenseValveOpenTimeL=264
    PumpLifeCntrH=265
    PumpLifeCntrL=266
    PGVALifeCntrH=267
    PGVALifeCntrL=268
    VacuumActualmBar=269
    PressureActualmBar=270
    OutputPressureActualmBar=271
    MinDacIncr=272
    ZeroDacIncr=273
    MaxDacIncr=274
    LastModbusErr=275
    Seed=276
    ManMode=277
#holding registers
    ValveActuationTime=4096
    VacuumThreshold=4097
    PressureThreshold=4098
    OutputPressure=4099
    MinCalibrSetP=4100
    ZeroCalibrSetP=4101
    MaxCalibrSetP=4102
    TcpPort=4108
    UnitId=4109
    VacuumThresholdmBar=4110
    PressureThresholdmBar=4111
    OutputPressuremBar=4112
    ManualTrigger=4113
    DhcpSelect=4115
    StoreEeprom=4196
    ResetPumpLifeCntr=4197
    StartPreCalibration=4198
    AdjustCalibrSetPoints=4199
    AuthStart=4296
    KeyWrite=4297
    ConfigOngoing=4298
#multiple holding registers
    IpAddressMSH=12288
    IpAddressLSH=12289
    GWAdressMSH=12290    
    GWAdressLSH=12291    
    NetMaskMSH=12292
    NetMaskLSH=12293
    MacAddressA=12294
    MacAddressB=12295
    MacAddressC=12296
    ProductionYear=12297
    ProductionDate=12298

class PGVA:
    def __init__(self, interface, comPort="COM1", tcpPort=8502, host="192.168.0.1", baudrate=115200):
        self.sensorData = {'vaccumChamber' : 0, 'pressureChamber': 0, 'outputPressure': 0}
        self.pgvaConfig = {
            "interface": interface,
            "comPort" : comPort,
            "tcpPort": tcpPort,
            "ip" : host,
            "baudrate" : baudrate,
            "modbusSlave" : 16
        }
        try:
            if self.pgvaConfig["interface"] == 'serial':
                self.client = SerialClient(method="ascii", port=self.pgvaConfig['comPort'], baudrate=self.pgvaConfig['baudrate'])
            elif self.pgvaConfig['interface']  == "tcp/ip":
                self.client = TcpClient(host=self.pgvaConfig["ip"], port=self.pgvaConfig['tcpPort'])
            
            self.client.connect()
        except Exception as e:
            print("Incorrect Modbus configuration : ", str(e))


    def readData(self, register, sign=True):
        data = 0
        try:
            data = self.client.read_input_registers(register, 1, unit=self.pgvaConfig['modbusSlave'])
        except Exception as e:
            print("Error while reading : ", str(e))

        result = data.registers[0]

        if (sign):
            if (result & 0x8000):
                result = -(~result & 0xFFFF) - 1

        return result



    def writeData(self, register, val, sign=True):
        status = object

        if (sign):
            val &= 0xFFFF

        print(f"{register}, {val}")
        try:
            if val < 0:
                val = val + 2**16
            self.client.write_register(register, val, unit=self.pgvaConfig['modbusSlave'])
            status = self.client.read_input_registers(_ModbusCommands.StatusWord, count=1, unit=self.pgvaConfig['modbusSlave']) 
            while (status.registers[0] & 1) == 1:
                status = self.client.read_input_registers(_ModbusCommands.StatusWord, 1, unit=self.pgvaConfig['modbusSlave'])
        except Exception as e:
            print("error while writing : ", str(e)) 
 

    #calibration procedure for setting exact levels of P&V
    def calibration(self):
        #send start calibartion
        self.writeData(_ModbusCommands.StartPreCalibration, 1, sign=True)
        
        #Set max pressure
        self.writeData(_ModbusCommands.AdjustCalibrSetPoints, 2, sign=False)
        
        #enter actual max
        self.writeData(_ModbusCommands.MaxCalibrSetP, 450, sign=True)

        #Set zero pressure
        self.writeData(_ModbusCommands.AdjustCalibrSetPoints, 1, sign=False)

        #enter actual 0
        self.writeData(_ModbusCommands.ZeroCalibrSetP, 0, sign=True)
               
        #Set min pressure 
        self.writeData(_ModbusCommands.AdjustCalibrSetPoints, 0, sign=False)
        
        #enter actual min
        self.writeData(_ModbusCommands.MinCalibrSetP, -450, sign=True)
     
    
    def setPumpPressure(self, pressure, vacuum):
        
        #set pressure chamber treshold
        if pressure in range(0, 550):
            self.writeData(_ModbusCommands.PressureThresholdmBar, pressure, sign=True)
        #set vacuum chamber treshold
        if vacuum in range(0, -550):
            self.writeData(_ModbusCommands.VacuumThresholdmBar, vacuum, sign=True)
        

    def aspirate(self, actuationTime: int, pressure: int):

        #set output pressure
        if pressure in range(-450, 0):
            self.writeData(_ModbusCommands.OutputPressuremBar, pressure)
        else:
            raise ValueError("Pressure Data not in range")
        self._waitUntilPressureRegulated(pressure)
        #set actuation time
        if actuationTime in range(0, 1000):
            self.writeData(_ModbusCommands.ValveActuationTime, actuationTime, sign=False)
            time.sleep(actuationTime/1000)
        else:
            raise ValueError("Actuation time not in range")

    def dispense(self, actuationTime: int, pressure: int):
        #convert from uL to actuation time
        if pressure in range(0, 450):
            self.writeData(_ModbusCommands.OutputPressuremBar, pressure)
        else:
            raise ValueError("Pressure Data not in range")
        self._waitUntilPressureRegulated(pressure)
        #set actuation time
        if actuationTime in range(0, 1000):
            self.writeData(_ModbusCommands.ValveActuationTime, actuationTime, sign=False)
            time.sleep(actuationTime/1000)
        else:
            raise ValueError("Actuation time not in range")

    def readSensData(self):

        self.sensorData['vaccumChamber'] = self.readData(_ModbusCommands.VacuumActualmBar, True)
        self.sensorData['pressureChamber'] = self.readData(_ModbusCommands.PressureActualmBar, True)
        self.sensorData['outputPressure'] = self.readData(_ModbusCommands.OutputPressureActualmBar, True)
    
        return self.sensorData   

    def _waitUntilPressureRegulated(self, desiredPressure:int):
        STABLE_PRESSURE_THRESHOLD = 3
        TIMEOUT = 100 # 100 * 50 ms = 5000 ms

        timesPressureReached = 0
        timeoutCounter = 0

        # If the desired pressure less than +-50 mbar we will wait until the
        # pressure is +- 10%. Otherwise, wait until the pressure is within +- 5%
        if (abs(desiredPressure) <= 50):
            margin = 0.1
        else:
            margin = 0.05

        maxAllowedDeviation = abs(int(desiredPressure * margin))
        
    
        while True:
            actualPressure = self.readData(_ModbusCommands.OutputPressureActualmBar, True)
            if (abs(desiredPressure - actualPressure) < maxAllowedDeviation):
                timesPressureReached += 1
            else:
                timesPressureReached = 0
            
            if (timesPressureReached == STABLE_PRESSURE_THRESHOLD):
                break
            
            if (timeoutCounter > TIMEOUT):
                return False

            timeoutCounter += 1

        return True
    



