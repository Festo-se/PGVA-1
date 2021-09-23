# Python Driver
<img src="https://www.python.org/static/community_logos/python-logo-master-v3-TM-flattened.png" alt="alt text" width="500" height="200">

## Language
* Python 3.7

## Required Libraries
* <img src="http://domoticx.com/wp-content/uploads/2017/09/modbus-logo-300x96.png" alt="alt text" width="60" height="30">[ PyModbus v2.5.2](http://riptideio.github.io/pymodbus/)
* <img src="https://pythonhosted.org/pyserial/_static/pyserial.png" alt="alt text" width="60" height="30">[ PySerial v3.5](https://pythonhosted.org/pyserial/)

## PGVA Arguments
* **string interface** - communication interface to be used ("tcp/ip" or "serial")
* **string comPort** - serial port name (ex. "COM3")
* **int tcpPort** - TCP port number for tcp/ip (ex. 502)
* **string host** - host IP for tcp/ip (ex. 192.168.0.XXX)
* **int baudrate** - baud rate speed for serial communication (ex. 115200)
* **int slaveID** - unit or slave Modbus identification number for the device (ex. 1)

## Example Code
### Basic TCP/IP example with port number and host IP passsed in
* Creates a new PGVA driver object (interface, tcpPort, host)
```python
pgva = PGVA("tcp/ip", tcpPort=8502, host="192.168.0.118")
```
* Aspirate at -40 mBar (vacuum) for 100 ms
```python
pgva.aspirate(100, -40)
```
* Dispense at 40 mBar (pressure) for 100 ms
```python
pgva.dispense(100, 40)
```


## Author
|Name          | Email                     | GitHub         |
| ------------ | ------------------------- | -------------- |
| Milen Kolev  | milen.kolev@festo.com     | @MKollev       |
