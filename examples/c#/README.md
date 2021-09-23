# C#/.NET Driver
<img src="https://miro.medium.com/max/2000/1*MfOHvI5b1XZKYTXIAKY7PQ.png" alt="alt text" width="500" height="200">

## Language and Framework
* C# 7.3
* .NETFramework v4.7.2

## Required Libraries
* <img src="https://a.fsdn.com/allura/p/easymodbustcp/icon?1609423069?&w=90" alt="alt text" width="30" height="30">[ EasyModbusTCP v5.6.0](https://sourceforge.net/projects/easymodbustcp/#focus)
* <img src="https://avatars.githubusercontent.com/u/27690942?s=280&v=4" alt="alt text" width="30" height="30">[ NModbus4 v3.0.0-alpha2](https://github.com/NModbus4/NModbus4)
* <img src="https://avatars.githubusercontent.com/u/27690942?s=280&v=4" alt="alt text" width="30" height="30">[ NModbus4.Serial v1.0.0-alpha1](https://github.com/NModbus4/NModbus4/tree/portable-3.0/NModbus4.Serial)

## Installing Dependencies
* Download [NuGet](https://www.nuget.org/) if you do not already have it 
* Use NuGet to add the needed libraries
* Add imports to the top of the file
```csharp 
using EasyModbus;
using Modbus.Device;
using Modbus.Serial;
```

## PGVA Arguments
* **string intrface** - communication interface to be used ("tcp/ip" or "serial")
* **string comPort** - serial port name (ex. "COM3")
* **int tcpPort** - TCP port number for tcp/ip (ex. 502)
* **string host** - host IP for tcp/ip (ex. 192.168.0.XXX)
* **int baudrate** - baud rate speed for serial communication (ex. 115200)
* **int slaveID** - unit or slave Modbus identification number for the device (ex. 1)

## Example Code

* Creates a new PGVA driver object (interface, comPort, tcpPort, host, baudrate, and slaveID)
```csharp
driver.IPgvaDriver pgva = 
new PgvaDriver("tcp/ip", "COM3", 8502, "192.168.0.199", 115200, 16);
```

* Sets the vacuum pump to -550 mBar and the pressure pump to 550 mBar
* Adjusts the max, min, and zero calibration set points
```csharp
pgva.SetPumpPressure(550, -550);
pgva.Calibration();
```

* Aspirate @ -40 mBar for 100 ms
* Dispense @ 40 mBar for 100 ms
```csharp
pgva.Aspirate(100, -40);
pgva.Dispense(100, 40);
```

## Author
|Name          | Email                     | GitHub         |
| ------------ | ------------------------- | -------------- |
| Jared Raines | raines.j@northeastern.edu | @rainesjared   |
=======
