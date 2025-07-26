namespace SwitchAutomation.Server.Models
{
    public class DeviceModel
    {
        public int DeviceId { get; set; }
        public string DeviceIp { get; set; }
        public string DeviceHostname { get; set; }
        public int AmountPort { get; set; }
        public string PortNumber { get; set; }
        public string PortState { get; set; }
        public string device_model { get; set; }

        public string device_license_level { get; set; }
        public string OSVersion { get; set; }
        public string device_serialnum { get; set; }
        public string port_description { get; set; }
        public string port_type { get; set; }
        public string port_vlan { get; set; }
 

    }
}
