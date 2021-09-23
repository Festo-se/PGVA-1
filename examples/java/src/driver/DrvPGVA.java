import java.io.InputStream;
import java.io.OutputStream;

import gnu.io.CommPort;
import gnu.io.CommPortIdentifier;
import gnu.io.SerialPort;

import com.serotonin.modbus4j.ModbusMaster;
import com.serotonin.modbus4j.code.DataType;
import com.serotonin.modbus4j.code.RegisterRange;
import com.serotonin.modbus4j.exception.ErrorResponseException;
import com.serotonin.modbus4j.exception.ModbusInitException;
import com.serotonin.modbus4j.exception.ModbusTransportException;
import com.serotonin.modbus4j.ip.IpParameters;
import com.serotonin.modbus4j.ip.tcp.TcpMaster;
import com.serotonin.modbus4j.locator.NumericLocator;
import com.serotonin.modbus4j.serial.SerialPortWrapper;
import com.serotonin.modbus4j.serial.ascii.AsciiMaster;

public class DrvPGVA {
	private PGVARegisters registers;
	private boolean tcpIpConnected;
	private boolean serialConnected;
	private int slaveId;
	public ModbusMaster client;
	
	public DrvPGVA(int slaveId) {
		this.slaveId = slaveId;
		this.tcpIpConnected = false;
		this.serialConnected = false;
		this.registers = new PGVARegisters();
	}
	
	public void connectTCP_IP(String ip, int port) throws ModbusInitException {
		if (tcpIpConnected || serialConnected) {
			throw new RuntimeException("A connection has already been initialized");
		}
		IpParameters parameters = new IpParameters();
		parameters.setHost(ip);
		parameters.setPort(port);
		parameters.setEncapsulated(false);
		this.client = new TcpMaster(parameters, true);
		this.client.setTimeout(2000);
		this.client.setRetries(2);
		this.client.setConnected(true);
		this.client.init();
		
		this.tcpIpConnected = true;
	}
	
	public void connectSerial(String comPort) throws ModbusInitException {
		if (tcpIpConnected || serialConnected) {
			throw new RuntimeException("A connection has already been initialized");
		}
		PGVASerialPortWrapper wrapper = new PGVASerialPortWrapper(comPort);
		this.client = new AsciiMaster(wrapper);
		this.client.init();
		
		this.serialConnected = true;
	}
	
	public void closeConnection() throws ModbusInitException {
		if (!tcpIpConnected && !serialConnected) {
			throw new RuntimeException("No connection has been initialized");
		}
		this.client.destroy();
		
		this.tcpIpConnected = false;
		this.serialConnected = false;
	}
	
	private void write(int register, int value) throws ModbusTransportException, ErrorResponseException {
		if (!(tcpIpConnected || serialConnected)) {
			throw new RuntimeException("No connection has been esablished");
		}
		
		if (value < 0) {
			value = value - 1;
		}
		
		this.client.setValue(new NumericLocator(
				this.slaveId, RegisterRange.HOLDING_REGISTER, register, DataType.TWO_BYTE_INT_UNSIGNED), value);
		int status = 1;
		do {
			status = this.read(this.registers.STATUS_WORD, false);
		} while ((status & 1) == 1);
	}
	
	private int read(int register, boolean signed) throws ModbusTransportException, ErrorResponseException {
		if (!(tcpIpConnected || serialConnected)) {
			throw new RuntimeException("No connection has been esablished");
		}
		int dataType;
		if (signed) {
			dataType = DataType.TWO_BYTE_INT_SIGNED;
		}
		else {
			dataType = DataType.TWO_BYTE_INT_UNSIGNED;
		}
		return this.client.getValue(new NumericLocator(
				this.slaveId, RegisterRange.INPUT_REGISTER, register, dataType)).intValue();
		
	}
	
	public void calibrate() throws ModbusTransportException, ErrorResponseException {
		// Send start calibration command
        this.write(this.registers.START_PRECALIBRATION, 1);
        
        // Set max pressure
        this.write(this.registers.ADJUST_CALIBR_SETP, 2);
        
        // Enter actual max
        this.write(this.registers.MAX_CALIBR_SETP, 450);

        // Set zero pressure
        this.write(this.registers.ADJUST_CALIBR_SETP, 1);

        // Enter actual 0
        this.write(this.registers.ZERO_CALIBR_SETP, 0);
               
        // Set min pressure 
        this.write(this.registers.ADJUST_CALIBR_SETP, 0);
        
        // Enter actual min
        this.write(this.registers.MIN_CALIBR_SETP, -450);
	}
	
	public void setPressureChamber(int pressure) throws ModbusTransportException, ErrorResponseException {
		if (pressure < 0 || pressure > 550) {
			throw new RuntimeException("The given pressure is outside the valid range [0, 550]mbar");
		}
		
		this.write(this.registers.PRESSURE_THRESHOLD_mBAR, pressure);
	}
	
	public void setVacuumChamber(int pressure) throws ModbusTransportException, ErrorResponseException {
		if (pressure < -550 || pressure > 0) {
			throw new RuntimeException("The given pressure is outside the valid range [-550, 0]mbar");
		}
		
		this.write(this.registers.VACUUM_THRESHOLD_mBAR, pressure);
	}
	
	public int getPressureChamberActual() throws ModbusTransportException, ErrorResponseException {
		return this.read(this.registers.PRESSURE_ACTUAL_mBAR, true);
	}
	
    public int getVacuumChamberActual() throws ModbusTransportException, ErrorResponseException {
    	return this.read(this.registers.VACUUM_ACTUAL_mBAR, true);
	}
    
    public int getOutputActual() throws ModbusTransportException, ErrorResponseException {
		return this.read(this.registers.OUTPUT_PRESSURE_ACTUAL_mBAR, true);
	}
	
	public void aspirate(int actuationTime, int pressure) throws InterruptedException, ModbusTransportException, ErrorResponseException {
		if (pressure < -450 || pressure > 0) {
			throw new RuntimeException("The given pressure is outside the valid range [-450, 0]mbar");
		}
		setTimedOutputPressure(actuationTime, pressure);
	}
	
	public void dispense(int actuationTime, int pressure) throws InterruptedException, ModbusTransportException, ErrorResponseException {
		if (pressure < 0 || pressure > 450) {
			throw new RuntimeException("The given pressure is outside the valid range [0, 450]mbar");
		}
		setTimedOutputPressure(actuationTime, pressure);
	}
	
	private void setTimedOutputPressure(int actuationTime, int pressure) throws InterruptedException, ModbusTransportException, ErrorResponseException {
		if (pressure < -450 || pressure > 450) {
			throw new RuntimeException("The given pressure is outside the valid range [-450, 450]mbar");
		}
		if (actuationTime < 0 || actuationTime > 1000) {
			throw new RuntimeException("The given actuation time is outside the valid range [0, 1000]msec");
		}
		
		this.write(registers.OUTPUT_PRESSURE_mBAR, pressure);
		Thread.sleep(500);
		this.write(this.registers.VALVE_ACTUATION_TIME, actuationTime);
		Thread.sleep(actuationTime);
	}
	
	class PGVASerialPortWrapper implements SerialPortWrapper {
		private String portName;
		private InputStream inStream;
		private OutputStream outStream;
		private CommPort commPort;
		
		public PGVASerialPortWrapper (String comPort) {
			this.portName = comPort;
		}
		
		@Override
		public void close() throws Exception {
			this.commPort.close();
		}

		@Override
		public int getBaudRate() {
			return 115200;
		}

		@Override
		public int getDataBits() {
			return 8;
		}

		@Override
		public InputStream getInputStream() {
			return this.inStream;
		}

		@Override
		public OutputStream getOutputStream() {
			return this.outStream;
		}

		@Override
		public int getParity() {
			return 0;
		}

		@Override
		public int getStopBits() {
			return 1;
		}

		@Override
		public void open() throws Exception {
			CommPortIdentifier portIdentifier = CommPortIdentifier.getPortIdentifier(this.portName);
	        if ( portIdentifier.isCurrentlyOwned() )
	        {
	            System.out.println("Error: Port is currently in use");
	        }
	        else
	        {
	            this.commPort = portIdentifier.open(this.getClass().getName(),1000);
	            
	            if ( commPort instanceof SerialPort )
	            {
	                SerialPort serialPort = (SerialPort) commPort;
	                serialPort.setSerialPortParams(this.getBaudRate(),this.getDataBits(), this.getStopBits(), this.getParity());
	                
	                this.inStream = serialPort.getInputStream();
	                this.outStream = serialPort.getOutputStream();
	            }
	            else
	            {
	                System.out.println("Error: Port is not a serial port");
	            }
	        }
		}
	}
}
