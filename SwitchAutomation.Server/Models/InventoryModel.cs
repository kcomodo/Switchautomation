namespace SwitchAutomation.Server.Models
{
    public class InventoryModel
    {
        public int inventory_ID { get; set; }
        public string inventory_name { get; set; } 
        public string inventory_description { get; set; }
        public string inventory_PID { get; set; }
        public string inventory_VID { get; set; }

        public string inventory_SN { get; set; }
        public int device_id { get; set; }
    }
}
