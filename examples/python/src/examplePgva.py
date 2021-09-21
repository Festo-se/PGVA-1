__author__ = "Kolev, Milen"
__copyright__ = "Copyright 2021, Festo Life Tech"
__credits__ = [""]
__license__ = "Apache"
__version__ = "0.0.1"
__maintainer__ = "Kolev, Milen"
__email__ = "milen.kolev@festo.com"
__status__ = "Development"

from driver.drvPgva import PGVA

if __name__ == "__main__":
    pgva = PGVA("tcp/ip", tcpPort=8502, host="192.168.0.118")

    #calibrate the pgva with sensor and pressure output
    pgva.calibration()
    #aspirate with some actuation time[ms] and pressure[mBar]
    pgva.aspirate(100, -40)
    #dispense with some actuation time[ms] and pressure[mBar]
    pgva.dispense(100, 40)