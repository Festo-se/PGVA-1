using driver;

namespace src
{
        static void Main(string[] args)
        {
            driver.IPgvaDriver pgva = new PgvaDriver("tcp/ip", "COM3", 8502, "192.168.0.199", 115200, 16);
            pgva.SetPumpPressure(550, -550);
            pgva.Calibration();
            pgva.Aspirate(100, -40);
            pgva.Dispense(100, 40);
        }
}