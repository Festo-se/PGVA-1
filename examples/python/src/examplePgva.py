__author__ = "Kolev, Milen"
__copyright__ = "Copyright 2021, Festo Life Tech"
__credits__ = [""]
__license__ = "Apache"
__version__ = "1.0.0"
__maintainer__ = "Kolev, Milen"
__email__ = "milen.kolev@festo.com"
__status__ = "Development"

import time

from driver.drvPgva import PGVA

if __name__ == "__main__":
    pgva = PGVA("serial", comPort="COM3", tcpPort=8502, host="192.168.0.199", baudrate=115200)
    # aspirate with some actuation time[ms] and vacuum[mBar]
    pgva.aspirate(100, -40)
    time.sleep(1)
    # dispense with some actuation time[ms] and pressure[mBar]
    pgva.dispense(100, 40)
    time.sleep(1)
