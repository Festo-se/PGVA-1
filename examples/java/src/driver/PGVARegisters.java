public class PVGARegisters {
	//input registers
    public int VACUUM = 256;
    public int PRESSURE_ACTUAL = 257;
    public int OUTPUT_PRESSURE_ACTUAL = 258;
    public int FIRMWARE_VER = 259;
    public int FIRMWARE_SUB_VER = 260;
    public int FIRMWARE_BUILD = 261;
    public int STATUS_WORD = 262;
    public int DISPENSE_VALUE_OPENTIME_H = 263;
    public int DISPENSE_VALUE_OPENTIME_L = 264;
    public int PUMP_LIFE_CNTR_H = 265;
    public int PUMP_LIFE_CNTR_L = 266;
    public int LIFE_CNTR_H = 267;
    public int LIFE_CNTR_L = 268;
    public int VACUUM_ACTUAL_mBAR = 269;
    public int PRESSURE_ACTUAL_mBAR = 270;
    public int OUTPUT_PRESSURE_ACTUAL_mBAR = 271;
    public int MIN_DAC_INCR = 272;
    public int ZERO_DAC_INCR = 273;
    public int MAX_DAC_INCR = 274;
    public int LAST_MODBUS_ERR = 275;
    public int SEED = 276;
    public int MAN_MODE = 277;
    
    //holding registers
    public int VALVE_ACTUATION_TIME = 4096;
    public int VACUUM_THRESHOLD = 4097;
    public int PRESSURE_THRESHOLD = 4098;
    public int OUTPUT_PRESSURE = 4099;
    public int MIN_CALIBR_SETP = 4100;
    public int ZERO_CALIBR_SETP = 4101;
    public int MAX_CALIBR_SETP = 4102;
    public int TCP_PORT = 4108;
    public int UNIT_ID = 4109;
    public int VACUUM_THRESHOLD_mBAR = 4110;
    public int PRESSURE_THRESHOLD_mBAR = 4111;
    public int OUTPUT_PRESSURE_mBAR = 4112;
    public int MANUAL_TRIGGER = 4113;
    public int DHCP_SELECT = 4115;
    public int STORE_EEPROM = 4196;
    public int RESET_PUMP_LIFE_CNTR = 4197;
    public int START_PRECALIBRATION = 4198;
    public int ADJUST_CALIBR_SETP = 4199;
    public int AUTH_START = 4296;
    public int KEY_WRITE = 4297;
    public int CONFIG_ONGOING = 4298;
    
    //multiple holding registers
    public int IP_ADDRESS_H = 12288;
    public int IP_ADDRESS_L = 12289;
    public int GW_ADDRESS_H = 12290;
    public int GW_ADDRESS_L = 12291;
    public int NETMASK_H = 12292;
    public int NETMASK_L = 12293;
    public int MAC_ADDRESS_A = 12294;
    public int MAC_ADDRESS_B = 12295;
    public int MAC_ADDRESS_C = 12296;
    public int PRODUCTION_YEAR = 12297;
    public int PRODUCTION_DATE = 12298;
}
