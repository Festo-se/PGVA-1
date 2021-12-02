/* Author:     Raines, Jared
 * Copyright:  Copyright 2021, Festo Life Tech
 * Version:    1.0.0
 * Maintainer: Raines, Jared
 * Email:      jared.raines@festo.com
 * Status:     Development
 */
using System;
using System.Net;
using System.Threading;
using EasyModbus;
using System.IO.Ports;
using Modbus.Device;
using Modbus.Serial;

namespace PgvaDriver.driver
{
    // Modbus Addresses
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
    
    // PGVA interface
    public interface IPgvaDriver
    {
        void Aspirate(int actuationTime, int pressure);
        void Dispense(int actuationTime, int pressure);
        int[] ReadSensorData();
        void SetOutputPressure(int pressure);
        void ActuateValve(int actuationTime);
        void Disconnect();
    }

    // PGVA Driver
    public class PgvaDriver : IPgvaDriver
    {
        private ModbusClient modbusClient;
        private IModbusSerialMaster master;
        private SerialPort port;
        private string intrface;
        private string comPort;
        private int tcpPort;
        private string host;
        private int baudrate;
        private byte slaveID;

        // Constructor
        public PgvaDriver(string intrface, string comPort, int tcpPort, string host, int baudrate, int slaveID)
        {
            this.intrface = intrface;
            this.comPort = comPort;
            this.tcpPort = tcpPort;
            this.host = host;
            this.baudrate = baudrate;
            this.slaveID = (byte)slaveID;

            Console.WriteLine("Start");
            System.Net.ServicePointManager.SecurityProtocol = 
                SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            try 
            {
                if (intrface.Equals("serial"))
                {
                    port = new SerialPort(comPort);
                    port.BaudRate = baudrate;
                    port.DataBits = 8;
                    port.Parity = Parity.None;
                    port.StopBits = StopBits.One;

                    port.Open();
                    var adapter = new SerialPortAdapter(port);
                    master = ModbusSerialMaster.CreateAscii(adapter);
                }
                else
                {
                    modbusClient = new ModbusClient(host, tcpPort);
                    modbusClient.ConnectionTimeout = 2000;
                    modbusClient.UnitIdentifier = this.slaveID;

                    modbusClient.Connect();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not connect to PGVA: " + e + ", attempt {" + (1) + "}");
            }

            Console.WriteLine("PGVA Initialized");
        }

        // disconnect from the PGVA
        public void Disconnect()
        {
            if (intrface == "serial")
            {
                port.Close();
            }
            else
            {
                modbusClient.Disconnect();
            }
        }

        // read a frame over modbus
        public int ReadData(int register)
        {
            try
            {
                if (intrface.Equals("serial"))
                {
                    ushort[] data = new ushort[1];
                    data = (master.ReadInputRegisters(slaveID, (ushort) register, 1));
                    return (data[0] - ((data[0] > 32767) ? 65536 : 0));
                }
                else
                {
                    int[] data = new int[1];
                    data = modbusClient.ReadInputRegisters(register, 0x01);
                    return data[0];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Read Error: " + e);
                return -1;
            }
        }

        // write a frame over modbus
        public void WriteData(int register, int val)
        {
            if (val < 0)
            {
                val += (int) Math.Pow(2, 16);
            }
            
            try
            {
                if (intrface.Equals("serial"))
                {
                    master.WriteSingleRegister(slaveID, (ushort)register, (ushort)val);
                    ushort[] status = master.ReadInputRegisters(slaveID, (int)InputRegisters.StatusWord, 1);
                    while ((status[0] & 1) == 1)
                    {
                        status = master.ReadInputRegisters(slaveID, (int)InputRegisters.StatusWord, 1);
                    }
                }
                else
                {
                    modbusClient.WriteSingleRegister(register, val);
                    int[] status = modbusClient.ReadInputRegisters((int)InputRegisters.StatusWord, 1);
                    while ((status[0] & 1) == 1)
                    {
                        status = modbusClient.ReadInputRegisters((int)InputRegisters.StatusWord, 1);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Write Error: " + e);
            }
        }

        // aspirate at the given vacuum for the given amount of time
        public void Aspirate(int actuationTime, int pressure)
        {
            if (pressure >= -450 && pressure <= 0)
            {
                WriteData((int)HoldingRegisters.OutputPressuremBar, pressure);
            }
            else
            {
                throw new ArgumentException("Pressure is outside of the range -450 to 0 mBar");
            }
            Thread.Sleep(500);
            if (actuationTime >= 0 && actuationTime <= 1000)
            {
                WriteData((int)HoldingRegisters.ValveActuationTime, actuationTime);
            }
            else
            {
                throw new ArgumentException("Actuation time is outside of the range 0 to 1000 ms");
            }
            Thread.Sleep(actuationTime);
        }

        // dispense at the given pressure for the given amount of time
        public void Dispense(int actuationTime, int pressure)
        {
            if (pressure >= 0 && pressure <= 450)
            {
                WriteData((int)HoldingRegisters.OutputPressuremBar, pressure);
            }
            else
            {
                throw new ArgumentException("Pressure is outside of the range 0 to 450 mBar");
            }
            Thread.Sleep(500);
            if (actuationTime >= 0 && actuationTime <= 1000)
            {
                WriteData((int)HoldingRegisters.ValveActuationTime, actuationTime);
            }
            else
            {
                throw new ArgumentException("Actuation time is outside of the range 0 to 1000 ms");
            }
            Thread.Sleep(actuationTime);
        }

        // read the vacuum chamber, pressure chamber, and output pressure values from the device
        public int[] ReadSensorData()
        {
            int[] data = new int[3];
            data[0] = ReadData((int)InputRegisters.VacuumActualmBar);
            data[1] = ReadData((int)InputRegisters.PressureActualmBar);
            data[2] = ReadData((int)InputRegisters.OutputPressureActualmBar);
            return data;
        }

        // set the output pressure in mBar
        public void SetOutputPressure(int pressure)
        {
            if (pressure >= -450 && pressure <= 450)
            {
                WriteData((int) HoldingRegisters.OutputPressuremBar, pressure);
                Thread.Sleep(500);
            }
            else
            {
                throw new ArgumentException("Pressure value not in range");
            }
        }

        // set the actuation time in milliseconds
        public void ActuateValve(int actuationTime)
        {
            if (actuationTime >= 0 && actuationTime <= 1000)
            {
                WriteData((int) HoldingRegisters.ValveActuationTime, actuationTime);
                Thread.Sleep(actuationTime / 1000);
            }
            else
            {
                throw new ArgumentException("Actuation time not in range");
            }
        }
    }
}