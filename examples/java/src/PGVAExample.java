
public class PGVAExample {
	public static void main(String[] args)
	{
		try
		{
			// Create a pgva object with mobus unit id 16
			DrvPGVA pgva = new DrvPGVA(16);
			
			// Connect to pgva over serial
			//pgva.connectSerial("COM5");
			
			// Connect to pgva over TCP/IP
			pgva.connectTCP_IP("192.168.0.199", 8502);
			
			// Calibrate pgva
			pgva.calibrate();
			
			// Print chamber pressures
			System.out.println(pgva.getPressureChamberActual());
			System.out.println(pgva.getVacuumChamberActual());
			
			// Aspirate for 100 ms at -40 mbar
			pgva.aspirate(100, -40);
			
			// Dispense for 100 ms at 40 mbar
			pgva.dispense(100, 40);
			
			// Close connection
			pgva.closeConnection();
		}
		catch (Exception e)
		{
			e.printStackTrace();
		}	
	}
}
