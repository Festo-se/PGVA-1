# Java Driver
<img src="https://user-images.githubusercontent.com/71296226/134033531-ce6c1238-aa46-43da-8d6a-9cd36d30a62b.png" alt="alt text" width="350" height="200">

## Language
* Java 11.0.10

## Required Libraries
* <img src="https://avatars.githubusercontent.com/u/2309355?s=88&v=4" alt="alt text" width="30" height="30">[ modbus4j v2.0.7](https://github.com/MangoAutomation/modbus4j)
* <img src="http://rxtx.qbang.org/wiki/skins/common/images/wikii.png" alt="alt text" width="60" height="30">[ RXTXcomm v2.1.7](http://rxtx.qbang.org/wiki/index.php/Main_Page)

## DrvPGVA Arguments
* **int slaveID** - unit or slave Modbus identification number for the device (ex. 1)

## connectTCP_IP Arguments
* **String ip** - host IP for tcp/ip (ex. 192.168.0.XXX)
* **int port** - TCP port number for tcp/ip (ex. 502)

## connectSerial Arguments
* **String comPort** - serial port name (ex. "COM3")

## Example Code

* Creates a new PGVA driver object (slaveID)
* Connects to the PGVA (host, tcpPort)
```java
DrvPGVA pgva = new DrvPGVA(16);
pgva.connectTCP_IP("192.168.0.199", 8502);
```

* Sets the vacuum pump to -550 mBar and the pressure pump to 550 mBar
* Prints the pressuer chamber and vacuum chamber values to the screen
```java
pgva.calibrate();

System.out.println(pgva.getPressureChamberActual());
System.out.println(pgva.getVacuumChamberActual());
```

* Aspirate @ -40 mBar for 100 ms
* Dispense @ 40 mBar for 100 ms
* Close the connection
```java
pgva.aspirate(100, -40);
pgva.dispense(100, 40);

pgva.closeConnection();
```

## Author
|Name          | Email                      | GitHub         |
| ------------ | -------------------------  | -------------- |
| John Alessio | alessio.j@northeastern.edu | @jalesssio     |
