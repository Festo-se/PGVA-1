import com.serotonin.modbus4j.exception.ErrorResponseException;
import com.serotonin.modbus4j.exception.ModbusInitException;
import com.serotonin.modbus4j.exception.ModbusTransportException;
import driver.DrvPGVA;
import static java.lang.Thread.sleep;
import static org.junit.Assert.*;
import static org.junit.jupiter.api.Assertions.assertThrows;

// unit testing
public class Tests {
    DrvPGVA pgva;

    // initialize the PGVA over tcp
    private void InitTCP() throws ModbusInitException, ErrorResponseException, ModbusTransportException {
        pgva = new DrvPGVA(16);
        pgva.connectTCP_IP("192.168.0.199",8502);
    }

    // initialize the PGVA over serial
    private void InitSerial() throws ModbusInitException, ErrorResponseException, ModbusTransportException {
        pgva = new DrvPGVA(16);
        pgva.connectSerial("COM3");
    }

    // test dispense(ms, mBar)
    @org.junit.Test
    public void TestDispense() throws InterruptedException, ErrorResponseException, ModbusTransportException, ModbusInitException {
        InitSerial();
        pgva.dispense(100, 0);
        sleep(200);
        assertEquals(0, pgva.readSensorData()[2], 10);
        pgva.dispense(100, 100);
        sleep(200);
        assertEquals(100, pgva.readSensorData()[2], 10);
        pgva.dispense(100, 400);
        sleep(200);
        assertEquals(400, pgva.readSensorData()[2], 10);
        pgva.disconnect();
    }

    // test dispense pressure value argument exceptions
    @org.junit.Test
    public void TestDispensePressureError() throws ModbusInitException, ModbusTransportException, ErrorResponseException {
        InitSerial();
        assertThrows(IllegalArgumentException.class, () -> {
            pgva.dispense(100, -1);
        });
        assertThrows(IllegalArgumentException.class, () -> {
            pgva.dispense(100, 451);
        });
        pgva.disconnect();
    }

    // test dispense actuation time argument exceptions
    @org.junit.Test
    public void TestDispenseActuateError() throws ModbusInitException, ModbusTransportException, ErrorResponseException {
        InitSerial();
        assertThrows(IllegalArgumentException.class, () -> {
            pgva.dispense(-1, 0);
        });
        assertThrows(IllegalArgumentException.class, () -> {
            pgva.dispense(1001, 550);
        });
        pgva.disconnect();
    }

    // test aspirate(ms, mBar)
    @org.junit.Test
    public void TestAspirate() throws InterruptedException, ErrorResponseException, ModbusTransportException, ModbusInitException {
        InitSerial();
        pgva.aspirate(100, 0);
        sleep(200);
        assertEquals(0, pgva.readSensorData()[2], 10);
        pgva.aspirate(100, -100);
        sleep(200);
        assertEquals(-100, pgva.readSensorData()[2], 10);
        pgva.aspirate(100, -400);
        sleep(200);
        assertEquals(-400, pgva.readSensorData()[2], 10);
        pgva.disconnect();
    }

    // test aspirate pressure value argument exceptions
    @org.junit.Test
    public void TestAspiratePressureError() throws ErrorResponseException, ModbusTransportException, ModbusInitException {
        InitSerial();
        assertThrows(IllegalArgumentException.class, () -> {
            pgva.aspirate(100, 1);
        });
        assertThrows(IllegalArgumentException.class, () -> {
            pgva.aspirate(100, -451);
        });
        pgva.disconnect();
    }

    // test aspirate actuation time argument exceptions
    @org.junit.Test
    public void TestAspirateActuateError() throws ErrorResponseException, ModbusTransportException, ModbusInitException {
        InitSerial();
        assertThrows(IllegalArgumentException.class, () -> {
            pgva.aspirate(-1, 0);
        });
        assertThrows(IllegalArgumentException.class, () -> {
            pgva.aspirate(1001, -550);
        });
        pgva.disconnect();
    }

    // test readSensorData()
    @org.junit.Test
    public void TestReadSensorData() throws InterruptedException, ErrorResponseException, ModbusTransportException, ModbusInitException {
        InitSerial();
        assertEquals(-500, pgva.readSensorData()[1], 75);
        assertEquals(500, pgva.readSensorData()[0], 75);
        pgva.dispense(100, 100);
        assertEquals(100, pgva.readSensorData()[2], 10);
        pgva.aspirate(100, -100);
        assertEquals(-100, pgva.readSensorData()[2], 10);
        pgva.disconnect();
    }

    // test setOutputPressure()
    @org.junit.Test
    public void TestSetOutputPressure() throws InterruptedException, ErrorResponseException, ModbusTransportException, ModbusInitException {
        InitSerial();
        pgva.setOutputPressure(100);
        assertEquals(100, pgva.readSensorData()[2], 10);
        pgva.setOutputPressure(0);
        assertEquals(0, pgva.readSensorData()[2], 10);
        pgva.setOutputPressure(-100);
        assertEquals(-100, pgva.readSensorData()[2], 10);
        pgva.disconnect();
    }

    // test setOutputPressure argument exceptions
    @org.junit.Test
    public void TestSetOutputPressureError() throws ErrorResponseException, ModbusTransportException, ModbusInitException {
        InitSerial();
        assertThrows(IllegalArgumentException.class, () -> {
            pgva.setOutputPressure(451);
        });
        assertThrows(IllegalArgumentException.class, () -> {
            pgva.setOutputPressure( -451);
        });
        pgva.disconnect();
    }

    // actuateValve(ms) tested qualitatively using example script

    // test actuateValve argument exceptions
    @org.junit.Test
    public void TestActuateValveError() throws ErrorResponseException, ModbusTransportException, ModbusInitException {
        InitSerial();
        assertThrows(IllegalArgumentException.class, () -> {
            pgva.actuateValve(-1);
        });
        assertThrows(IllegalArgumentException.class, () -> {
            pgva.actuateValve( 1001);
        });
        pgva.disconnect();
    }
}
