# C#/.NET Driver

![image](https://miro.medium.com/max/2000/1*MfOHvI5b1XZKYTXIAKY7PQ.png)

## Required Libraries

* <img src="https://a.fsdn.com/allura/p/easymodbustcp/icon?1609423069?&w=90" alt="alt text" width="30" height="30">[ EasyModbusTCP 5.6.0](https://sourceforge.net/projects/easymodbustcp/#focus)

## Installing Dependencies
* Download [NuGet](https://www.nuget.org/) if you do not already have it 
* Use NuGet to add the needed libraries
* Add imports to the top of the file
```csharp 
using EasyModbus
```
## Example Code

* Creates a new PGVA driver object (interface, comPort, tcpPort, host, baudrate, and slaveID)
```csharp
driver.IPgvaDriver pgva = 
new PgvaDriver("tcp/ip", "COM3", 8502, "192.168.0.199", 115200, 16);
```

* Sets the vacuum pump to -550 mBar and the pressure pump to 550 mBar and calibrate the set points
```csharp
pgva.SetPumpPressure(550, -550);
pgva.Calibration();
```

* Aspirate @ -40 mBar for 100 ms and dispense @ 40 mBar for 100 ms
```csharp
pgva.Aspirate(100, -40);
pgva.Dispense(100, 40);
```

## Author
|Name          | Email                     | GitHub         |
| ------------ | ------------------------- | -------------- |
| Jared Raines | raines.j@northeastern.edu | @rainesjared   |
