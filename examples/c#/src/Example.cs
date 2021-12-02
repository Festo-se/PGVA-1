/* Author:     Raines, Jared
 * Copyright:  Copyright 2021, Festo Life Tech
 * Version:    1.0.0
 * Maintainer: Raines, Jared
 * Email:      jared.raines@festo.com
 * Status:     Development
 */

using System.Threading;
using PgvaDriver.driver;
 

class Example
{
    static void Main(string[] args)
    { 
        IPgvaDriver pgva = new PgvaDriver.driver.PgvaDriver("serial", "COM3", 8502, "192.168.0.199", 115200, 16);
        // aspirate with some actuation time(ms) and vacuum(mBar)
        pgva.Aspirate(100, -40);
        Thread.Sleep(1);
        // dispense with some actuation time(ms) and pressure(mBar)
        pgva.Dispense(100, 40);
        Thread.Sleep(1);
    }
}