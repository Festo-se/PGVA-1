using System;
using System.Net;
using System.Threading;
using EasyModbus;

namespace PvgaCSharpDriver_2._0
{
    enum InputRegisters
    {
        VacuumActual = 256,
        PressureActual = 257,
        OutputPressureActual = 258,
        FirmwareVer = 259,
        FirmwareSubVer = 260,
        FirmwareBuild = 261,
        StatusWord = 262,
        DispenseValveOpenTimeH = 263,
        DispenseValveOpenTimeL = 264,
        PumpLifeCntrH = 265,
        PumpLifeCntrL = 266,
        PgvaLifeCntrH = 267,
        PgvaLifeCntrL = 268,
        VacuumActualmBar = 269,
        PressureActualmBar = 270,
        OutputPressureActualmBar = 271,
        MinDacIncr = 272,
        ZeroDacIncr = 273,
        MaxDacIncr = 274,
        LastModbusErr = 275,
        Seed = 276,
        ManMode = 277
    }

    enum HoldingRegisters
    {
        ValveActuationTime = 4096,
        VacuumThreshold = 4097,
        PressureThreshold = 4098,
        OutputPressure = 4099,
        MinCalibrSetP = 4100,
        ZeroCalibrSetP = 4101,
        MaxCalibrSetP = 4102,
        TcpPort = 4108,
        UnitId = 4109,
        VacuumThresholdmBar = 4110,
        PressureThresholdmBar = 4111,
        OutputPressuremBar = 4112,
        ManualTrigger = 4113,
        DhcpSelect = 4115,
        StoreEeprom = 4196,
        ResetPumpLifeCntr = 4197,
        StartPreCalibration = 4198,
        AdjustCalibrSetPoints = 4199,
        AuthStart = 4296,
        KeyWrite = 4297,
        ConfigOngoing = 4298
    }

    enum MultipleHoldingRegisters
    {
        IpAddressMsh=12288,
        IpAddressLsh=12289,
        GwAddressMsh=12290,
        GwAddressLsh=12291,
        NetMaskMsh=12292,
        NetMaskLsh=12293,
        MacAddressA=12294,
        MacAddressB=12295,
        MacAddressC=12296,
        ProductionYear=12297,
        ProductionDate=12298
    }
        
interface IPgvaDriver {
    void Aspirate(int mL);
    void Dispense(int mL);
    void Mix();
}

    class PgvaDriver : IPgvaDriver
    {
        private ModbusClient modbusClient;

        // DEFAULT: TCP/IP
        public PgvaDriver(String config)
        {
            Console.WriteLine("Start");
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.SecurityProtocol = 
                SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            if (config.Equals("serial"))
            {
                modbusClient = new ModbusClient("Com3");
            }
            else
            {
                modbusClient = new ModbusClient("192.168.0.199", 8502);
            }
            modbusClient.Baudrate = 115200;
            modbusClient.ConnectionTimeout = 2000;
            modbusClient.UnitIdentifier = 16;
            
            try
            {
                modbusClient.Connect();
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not connect to PGVA: " + e + ", attempt {" + (1) + "}");
            }

            SetPumpPressure();
            Console.WriteLine("PGVA Initialized");
        }

        public int ReadData(int register)
        {
            int[] data = new int[1];

            try
            {
                data = modbusClient.ReadInputRegisters(register, 0x01);
            }
            catch (Exception e)
            {
                Console.WriteLine("Read Error: " + e);
            }

            return data[0];
        }

        public void WriteData(int register, int val)
        {
            try
            {
                if (val < 0)
                {
                    val += (int) Math.Pow(2, 16);
                }
                
                modbusClient.WriteSingleRegister(register, val);
                int[] status = modbusClient.ReadInputRegisters((int)InputRegisters.StatusWord, 1);
                while ((status[0] & 1) == 1)
                {
                    status = modbusClient.ReadInputRegisters((int)InputRegisters.StatusWord, 1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Write Error: " + e);
            }
        }

        public void Aspirate(int mL)
        {
            int actuationTime = 200;
            modbusClient.WriteSingleRegister((int)HoldingRegisters.OutputPressuremBar, -80);
            Thread.Sleep(500);
            modbusClient.WriteSingleRegister((int)HoldingRegisters.ValveActuationTime, 150);
            Thread.Sleep(actuationTime);
        }

        public void Dispense(int mL)
        {
            int actuationtime = 280;
            modbusClient.WriteSingleRegister((int)HoldingRegisters.OutputPressuremBar, 50);
            Thread.Sleep(500);
            modbusClient.WriteSingleRegister((int)HoldingRegisters.ValveActuationTime, 280);
            Thread.Sleep(actuationtime);
        }

        public void Mix()
        {
            int actuationTime = 150;
            WriteData((int)HoldingRegisters.OutputPressuremBar, -80);
            Thread.Sleep(500);
            WriteData((int)HoldingRegisters.ValveActuationTime, actuationTime);
            Thread.Sleep(actuationTime);

            Thread.Sleep(2000);

            actuationTime = 100;
            WriteData((int)HoldingRegisters.OutputPressuremBar, 35);
            Thread.Sleep(500);
            WriteData((int)HoldingRegisters.ValveActuationTime, actuationTime);
            Thread.Sleep(actuationTime);
            Console.WriteLine("Mix");
        }

        public void Calibration()
        {
            // start calibration
            WriteData((int)HoldingRegisters.StartPreCalibration, 1);
            // set max pressure
            WriteData((int)HoldingRegisters.AdjustCalibrSetPoints, 2);
            // enter actual max
            WriteData((int)HoldingRegisters.MaxCalibrSetP, 450);
            // set zero pressure
            WriteData((int)HoldingRegisters.AdjustCalibrSetPoints, 1);
            // enter actual 0
            WriteData((int)HoldingRegisters.ZeroCalibrSetP, 0);
            // set min pressure
            WriteData((int)HoldingRegisters.AdjustCalibrSetPoints, 0);
            // enter actual min
            WriteData((int)HoldingRegisters.MinCalibrSetP, -450);
        }

        public void SetPumpPressure()
        {
            WriteData((int)HoldingRegisters.PressureThresholdmBar, 550);
            WriteData((int)HoldingRegisters.VacuumThresholdmBar, -550);
        }

        public int[] ReadSensorData()
        {
            int[] data = new int[3];
            data[0] = ReadData((int)InputRegisters.VacuumActualmBar);
            data[1] = ReadData((int)InputRegisters.PressureActualmBar);
            data[2] = ReadData((int)InputRegisters.OutputPressureActualmBar);
            return data;
        }

    }

    class Example
    {
        static void Main(string[] args)
        {
            IPgvaDriver pg = new PgvaDriver("serial");

            while (true)
            {
                Console.WriteLine("Aspirate");
                pg.Aspirate(10);
                Thread.Sleep(3000);
            
                Console.WriteLine("Dispense");
                pg.Dispense(10);
                Thread.Sleep(3000);
            }
        }
    }
}