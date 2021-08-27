from driver.drvPgva import PGVA

if __name__ == "__main__":
    pgva = PGVA("tcp/ip", tcpPort=8502, host="192.168.0.118")

    #calibrate the pgva with sensor and pressure output
    pgva.calibration()
    #aspirate with some actuation time[ms] and pressure[mBar]
    pgva.aspirate(100, -40)
    #dispense with some actuation time[ms] and pressure[mBar]
    pgva.dispense(100, 40)