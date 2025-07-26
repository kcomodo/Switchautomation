
using MySql.Data.MySqlClient;
using System.Linq;  // This enables LINQ extension methods

using SwitchAutomation.Server.Models;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Renci.SshNet;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Newtonsoft.Json;

namespace SwitchAutomation.Server.Repository
{
    public class DeviceRepository
    {
        private readonly MySqlConnection _connection;

        public DeviceRepository()
        {
            // string connectionString = "server=127.0.0.1;database=automation;userid=root;password=c9nbQ5yMX2E9WVW;port=3306";
            string connectionString = "server=database;database=automation;userid=root;password=c9nbQ5yMX2E9WVW;port=3306";
            _connection = new MySqlConnection(connectionString);
            _connection.Open();
        }

        ~DeviceRepository()
        {
            _connection.Close();
        }

        public void AddDevice(DeviceModel device)
        {
            string query = @"INSERT INTO device 
                    (device_ip, device_hostname, amount_port, port_number, port_state, device_model, device_license_level, OSVersion, device_serialnum, port_description, port_type, port_vlan) 
                    VALUES 
                    (@ip, @hostname, @amountPort, @portNumber, @portState, @model, @licenseLevel, @OSVersion, @device_serialnum, @portDescription, @portType, @portVlan)";

            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@ip", device.DeviceIp);
            cmd.Parameters.AddWithValue("@hostname", device.DeviceHostname ?? "");
            cmd.Parameters.AddWithValue("@amountPort", device.AmountPort);
            cmd.Parameters.AddWithValue("@portNumber", device.PortNumber);
            cmd.Parameters.AddWithValue("@portState", device.PortState ?? "");
            cmd.Parameters.AddWithValue("@model", device.device_model ?? "");
            cmd.Parameters.AddWithValue("@licenseLevel", device.device_license_level ?? "");
            cmd.Parameters.AddWithValue("@OSVersion", device.OSVersion ?? "");
            cmd.Parameters.AddWithValue("@device_serialnum", device.device_serialnum ?? "");
            cmd.Parameters.AddWithValue("@portDescription", device.port_description ?? "");
            cmd.Parameters.AddWithValue("@portType", device.port_type ?? "");
            cmd.Parameters.AddWithValue("@portVlan", device.port_vlan);
            cmd.ExecuteNonQuery();
        }

        public DeviceModel GetDeviceByIp(string deviceIp)
        {
            string query = @"SELECT * FROM device WHERE device_ip = @ip LIMIT 1";

            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@ip", deviceIp);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new DeviceModel
                {
                    DeviceIp = reader["device_ip"] != DBNull.Value ? reader.GetString("device_ip") : null,
                    DeviceHostname = reader["device_hostname"] != DBNull.Value ? reader.GetString("device_hostname") : null,
                    AmountPort = reader["amount_port"] != DBNull.Value ? reader.GetInt32("amount_port") : 0,
                    PortNumber = reader["port_number"] != DBNull.Value ? reader.GetString("port_number") : null,
                    PortState = reader["port_state"] != DBNull.Value ? reader.GetString("port_state") : null,
                    device_model = reader["device_model"] != DBNull.Value ? reader.GetString("device_model") : null,
                    device_license_level = reader["device_license_level"] != DBNull.Value ? reader.GetString("device_license_level") : null,
                    OSVersion = reader["OSVersion"] != DBNull.Value ? reader.GetString("OSVersion") : null,
                    device_serialnum = reader["device_serialnum"] != DBNull.Value ? reader.GetString("device_serialnum") : null,
                    port_description = reader["port_description"] != DBNull.Value ? reader.GetString("port_description") : null,
                    port_type = reader["port_type"] != DBNull.Value ? reader.GetString("port_type") : null,
                    port_vlan = reader["port_vlan"] != DBNull.Value ? reader.GetString("port_vlan") : null
                };
            }

            return null;  // Return null if no record is found.
        }
        public List<DeviceModel> GetAllDevices()
        {
            string query = @"SELECT * FROM device";

            using var cmd = new MySqlCommand(query, _connection);

            using var reader = cmd.ExecuteReader();
            var devices = new List<DeviceModel>();

            while (reader.Read())
            {
                var device = new DeviceModel
                {
                    DeviceIp = reader["device_ip"] != DBNull.Value ? reader.GetString("device_ip") : null,
                    DeviceHostname = reader["device_hostname"] != DBNull.Value ? reader.GetString("device_hostname") : null,
                    AmountPort = reader["amount_port"] != DBNull.Value ? reader.GetInt32("amount_port") : 0,
                    PortNumber = reader["port_number"] != DBNull.Value ? reader.GetString("port_number") : null,
                    PortState = reader["port_state"] != DBNull.Value ? reader.GetString("port_state") : null,
                    device_model = reader["device_model"] != DBNull.Value ? reader.GetString("device_model") : null,
                    device_license_level = reader["device_license_level"] != DBNull.Value ? reader.GetString("device_license_level") : null,
                    OSVersion = reader["OSVersion"] != DBNull.Value ? reader.GetString("OSVersion") : null,
                    device_serialnum = reader["device_serialnum"] != DBNull.Value ? reader.GetString("device_serialnum") : null,
                    port_description = reader["port_description"] != DBNull.Value ? reader.GetString("port_description") : null,
                    port_type = reader["port_type"] != DBNull.Value ? reader.GetString("port_type") : null,
                    port_vlan = reader["port_vlan"] != DBNull.Value ? reader.GetString("port_vlan") : null
                };
                devices.Add(device);
            }

            return devices;  // Return the list of devices
        }
        public bool DeleteDeviceByIp(string deviceIp)
        {
            string query = @"DELETE FROM device WHERE device_ip = @DeviceIp";

            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@DeviceIp", deviceIp);

            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }
        public bool DeleteDeviceAndRelatedDataByIp(string deviceIp)
        {
  

            // Get device ID first
            var getIdQuery = "SELECT device_id FROM device WHERE device_ip = @DeviceIp";
            using var getIdCmd = new MySqlCommand(getIdQuery, _connection);
            getIdCmd.Parameters.AddWithValue("@DeviceIp", deviceIp);

            var deviceIdObj = getIdCmd.ExecuteScalar();
            if (deviceIdObj == null) return false;

            int deviceId = Convert.ToInt32(deviceIdObj);

            // Delete from child tables first
            var deleteConfigQuery = "DELETE FROM device_configurations WHERE device_id = @DeviceId";
            var deleteInventoryQuery = "DELETE FROM device_inventory WHERE device_id = @DeviceId";
            var deleteDeviceQuery = "DELETE FROM device WHERE device_id = @DeviceId";

            using var deleteConfigCmd = new MySqlCommand(deleteConfigQuery, _connection);
            using var deleteInventoryCmd = new MySqlCommand(deleteInventoryQuery, _connection);
            using var deleteDeviceCmd = new MySqlCommand(deleteDeviceQuery, _connection);

            deleteConfigCmd.Parameters.AddWithValue("@DeviceId", deviceId);
            deleteInventoryCmd.Parameters.AddWithValue("@DeviceId", deviceId);
            deleteDeviceCmd.Parameters.AddWithValue("@DeviceId", deviceId);

            deleteConfigCmd.ExecuteNonQuery();
            deleteInventoryCmd.ExecuteNonQuery();
            int deleted = deleteDeviceCmd.ExecuteNonQuery();

            return deleted > 0;
        }


        public bool DeviceExistsByIpOrHostname(string deviceIpOrHostname)
        {
            string query = @"SELECT 1 FROM device 
                     WHERE device_ip = @input OR device_hostname = @input
                     LIMIT 1";

            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@input", deviceIpOrHostname);

            using var reader = cmd.ExecuteReader();
            return reader.HasRows;
        }

        public DeviceModel GetDeviceByHostname(string hostname)
        {
            string query = @"SELECT * FROM device WHERE device_hostname = @hostname LIMIT 1";

            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@hostname", hostname);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new DeviceModel
                {
                    DeviceIp = reader["device_ip"] != DBNull.Value ? reader.GetString("device_ip") : null,
                    DeviceHostname = reader["device_hostname"] != DBNull.Value ? reader.GetString("device_hostname") : null,
                    // (other fields if needed)
                };
            }

            return null;
        }
        public class IetfInterfaces
        {
            public InterfaceList Interfaces { get; set; }
        }

        public class InterfaceList
        {
            public List<Interface> Interface { get; set; } // A list of Interface objects
        }

        public class Interface
        {
            public string Name { get; set; }
            public string Type { get; set; }  // This corresponds to the interface type, e.g., "iana-if-type:ethernetCsmacd"
        }



        public async Task<int> GetTotalPorts(HttpClient client, string deviceIp)
        {
            try
            {
                var url = $"https://{deviceIp}/restconf/data/ietf-interfaces:interfaces";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"HTTP error: {response.StatusCode}");
                    return 0;
                }

                var content = await response.Content.ReadAsStringAsync();

                // Print the raw JSON response to console or logs
                Console.WriteLine("===== Raw JSON Response =====");
                Console.WriteLine(content);
                Console.WriteLine("===== End of JSON =====");

                // Parse the JSON response
                var jsonResponse = JObject.Parse(content);

                // Find all "name" elements and count them
                var interfaces = jsonResponse["ietf-interfaces:interfaces"]?["interface"];
                int portCount = interfaces?.Count() ?? 0;

                Console.WriteLine($"Total Ports: {portCount}");

                return portCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return 0;
            }
        }


        public async Task<int> GetPhysicalPorts(HttpClient client, string deviceIp)
        {
            try
            {
                var url = $"https://{deviceIp}/restconf/data/ietf-interfaces:interfaces";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"HTTP error: {response.StatusCode}");
                    return 0;
                }

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine("===== Raw JSON Response =====");
                Console.WriteLine(content);
                Console.WriteLine("===== End of JSON =====");

                var jsonResponse = JObject.Parse(content);
                var interfaces = jsonResponse["ietf-interfaces:interfaces"]?["interface"];

                if (interfaces == null)
                {
                    Console.WriteLine("No interfaces found.");
                    return 0;
                }

                // Print all interface names for debugging
                Console.WriteLine("===== Interface Names =====");
                foreach (var iface in interfaces)
                {
                    Console.WriteLine(iface["name"].ToString());
                }

                // Filter for physical ports (excluding management port, VLAN ports, and interfaces like 'Vlan1')
                var physicalInterfaces = interfaces
                    .Where(iface => (iface["name"].ToString().Contains("GigabitEthernet") ||
                                     iface["name"].ToString().Contains("TenGigabitEthernet") ||
                                     iface["name"].ToString().Contains("TwentyFiveGigE") ||
                                     iface["name"].ToString().Contains("FortyGigabitEthernet")) &&
                                    iface["name"].ToString() != "GigabitEthernet0/0" &&  // Exclude management port
                                    !iface["name"].ToString().Contains("Vlan"))  // Exclude VLAN interfaces
                    .ToList();

                int physicalPortCount = physicalInterfaces.Count();
                Console.WriteLine($"Total Physical Ports (Excluding Management and VLAN): {physicalPortCount}");

                return physicalPortCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return 0;
            }
        }

        private (int slot, int port) ExtractPortSortKey(string portName)
        {
            var match = Regex.Match(portName, @"\d+/\d+/\d+");
            if (match.Success)
            {
                var parts = match.Value.Split('/');
                if (parts.Length == 3 &&
                    int.TryParse(parts[0], out int mod) &&
                    int.TryParse(parts[1], out int slot) &&
                    int.TryParse(parts[2], out int port))
                {
                    return (slot * 1000 + mod, port); // sorts by slot/mod before port
                }
            }
            return (int.MaxValue, int.MaxValue); // put invalid names at end
        }


        public async Task<List<string>> GetPhysicalPortNames(HttpClient client, string deviceIp)
        {
            try
            {
                var url = $"https://{deviceIp}/restconf/data/ietf-interfaces:interfaces";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"HTTP error: {response.StatusCode}");
                    return new List<string>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(content);
                var interfaces = jsonResponse["ietf-interfaces:interfaces"]?["interface"];

                if (interfaces == null)
                {
                    Console.WriteLine("No interfaces found.");
                    return new List<string>();
                }

                var portNames = interfaces
                    .Where(iface => (iface["name"].ToString().Contains("GigabitEthernet") ||
                                     iface["name"].ToString().Contains("TenGigabitEthernet") ||
                                     iface["name"].ToString().Contains("TwentyFiveGigE") ||
                                     iface["name"].ToString().Contains("FortyGigabitEthernet")) &&
                                    !iface["name"].ToString().Contains("MgmtEthernet") &&
                                    iface["name"].ToString() != "GigabitEthernet0/0" &&  // exclude known management
                                    !iface["name"].ToString().Contains("Vlan"))
                    .Select(iface => iface["name"].ToString())
                .OrderBy(name => ExtractPortSortKey(name).Item1) // slot
                .ThenBy(name => ExtractPortSortKey(name).Item2)  // port
                .ToList();


                // Optionally log the port names to the console
                Console.WriteLine("===== Port Names =====");
                portNames.ForEach(Console.WriteLine);

                return portNames;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return new List<string>();
            }
        }
        public async Task<string> GetPhysicalPortStates(HttpClient client, string deviceIp)
        {
            try
            {
                var url = $"https://{deviceIp}/restconf/data/Cisco-IOS-XE-interfaces-oper:interfaces";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"HTTP error while fetching port states: {response.StatusCode}");
                    return "";
                }

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);
                var interfaces = json["Cisco-IOS-XE-interfaces-oper:interfaces"]?["interface"];

                if (interfaces == null)
                {
                    Console.WriteLine("No interface state data found.");
                    return "";
                }

                var sortedInterfaces = interfaces
                    .Where(iface =>
                        iface["name"] != null &&
                        (iface["name"].ToString().Contains("GigabitEthernet") ||
                         iface["name"].ToString().Contains("TenGigabitEthernet") ||
                         iface["name"].ToString().Contains("TwentyFiveGigE") ||
                         iface["name"].ToString().Contains("FortyGigabitEthernet")) &&
                        !iface["name"].ToString().Contains("MgmtEthernet") &&
                        iface["name"].ToString() != "GigabitEthernet0/0" &&
                        !iface["name"].ToString().Contains("Vlan"))
                    .OrderBy(iface => ExtractPortSortKey(iface["name"]?.ToString()))
                    .ToList();

                var states = sortedInterfaces
                    .Select(iface =>
                    {
                        var status = iface["oper-status"]?.ToString() ?? "unknown";
                        return status switch
                        {
                            "if-oper-state-lower-layer-down" => "down",
                            "if-oper-state-ready" => "up",
                            "if-oper-state-up" => "up",
                            "if-oper-state-down" => "down",
                            _ => "unknown"
                        };
                    })
                    .ToList();

                return string.Join(",", states);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while fetching port states: {ex.Message}");
                return "";
            }
        }


        public async Task<int> GetVirtualPorts(HttpClient client, string deviceIp)
        {
            try
            {
                var url = $"https://{deviceIp}/restconf/data/ietf-interfaces:interfaces";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"HTTP error: {response.StatusCode}");
                    return 0;
                }

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine("===== Raw JSON Response =====");
                Console.WriteLine(content);
                Console.WriteLine("===== End of JSON =====");

                var jsonResponse = JObject.Parse(content);
                var interfaces = jsonResponse["ietf-interfaces:interfaces"]?["interface"];

                if (interfaces == null)
                {
                    Console.WriteLine("No interfaces found.");
                    return 0;
                }

                // Filter for virtual ports (e.g., VLAN, Loopback, etc.)
                var virtualInterfaces = interfaces
                    .Where(iface => iface["name"].ToString().Contains("Vlan") ||
                                    iface["name"].ToString().Contains("Loopback"))
                    .ToList();

                int virtualPortCount = virtualInterfaces.Count();
                Console.WriteLine($"Total Virtual Ports: {virtualPortCount}");

                return virtualPortCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return 0;
            }
        }

        public string GetDeviceLicenseInfo(string ip, string username, string password)
        {
            using var sshClient = new SshClient(ip, username, password);
            sshClient.Connect();

            var cmd = sshClient.RunCommand("show version");
            string output = cmd.Result;

            Console.WriteLine("===== Raw Show Version Output =====");
            Console.WriteLine(output);

            sshClient.Disconnect();

            var match = Regex.Match(output, @"Technology-package\s+Current\s+Type\s+Next reboot\s+-+\s+(.*?)(?=\n\n|\r\n\r\n)", RegexOptions.Singleline);

            if (match.Success)
            {
                string licenseBlock = match.Groups[1].Value.Trim();
                Console.WriteLine("===== Extracted License Info =====");
                Console.WriteLine(licenseBlock);


                var firstLine = licenseBlock.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!string.IsNullOrEmpty(firstLine))
                {
                    var parts = Regex.Split(firstLine.Trim(), @"\s{2,}");
                    if (parts.Length > 0)
                    {
                        return parts[0].Trim();
                    }
                }
            }

            Console.WriteLine("No license info found.");
            return "Unknown";
        }

        public async Task<string> GetDeviceOsVersion(string ip, string username, string password)
        {
            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            using var client = new HttpClient(handler);
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/yang-data+json"));

            var url = $"https://{ip}/restconf/data/Cisco-IOS-XE-native:native/version";

            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("Cisco-IOS-XE-native:version", out var versionElement))
                {
                    return versionElement.GetString() ?? "Unknown";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting OS version: {ex.Message}");
            }

            return "Unknown";
        }

        public string GetDeviceSerialNumber(string ip, string username, string password)
        {
            using var sshClient = new SshClient(ip, username, password);
            sshClient.Connect();

            var cmd = sshClient.RunCommand("show version");
            string output = cmd.Result;

            sshClient.Disconnect();

            // Extract the Serial Number using regex
            var match = Regex.Match(output, @"System Serial Number\s+:\s+(\S+)", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string serialNumber = match.Groups[1].Value.Trim();
                return serialNumber;
            }

            return "Unknown";
        }
        public bool ValidateLogin(string username, string password)
        {
            string query = "SELECT COUNT(*) FROM user WHERE username = @username AND password = @password";

            using (MySqlCommand cmd = new MySqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password); 

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        public async Task<(List<string> descriptions, List<string> types, List<string> vlans)> GetPhysicalPortDetails(HttpClient client, string deviceIp)
        {
            string url = $"https://{deviceIp}/restconf/data/Cisco-IOS-XE-native:native/interface";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            dynamic parsed = JsonConvert.DeserializeObject(json);

            var gigabitInterfaces = parsed?["Cisco-IOS-XE-native:interface"]?.GigabitEthernet;
            var descriptions = new List<string>();
            var types = new List<string>();
            var vlans = new List<string>();

            if (gigabitInterfaces == null)
                return (descriptions, types, vlans);

            // Sort interfaces by numeric port ID (e.g., "1/0/24")
            var sortedInterfaces = ((IEnumerable<dynamic>)gigabitInterfaces)
                .OrderBy(iface => ExtractPortSortKey($"GigabitEthernet{iface.name}"))
                .ToList();

            foreach (var iface in sortedInterfaces)
            {
                string desc = iface?.description ?? "";
                string type = "GigabitEthernet"; // or dynamically derive from RESTCONF path if needed
                string vlan = iface?.encapsulation?.dot1Q?.vlanId?.ToString() ?? "";

                descriptions.Add(desc);
                types.Add(type);
                vlans.Add(vlan);
            }

            return (descriptions, types, vlans);
        }
        public void AddInventoryItem(InventoryModel inventoryItem)
        {
      

            var query = @"INSERT INTO device_inventory (inventory_name, inventory_description, inventory_PID, inventory_VID, inventory_SN, device_id)
                  VALUES (@Name, @Description, @PID, @VID, @SN, @device_id);";

            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@device_id", inventoryItem.device_id);
            cmd.Parameters.AddWithValue("@Name", inventoryItem.inventory_name);
            cmd.Parameters.AddWithValue("@Description", inventoryItem.inventory_description);
            cmd.Parameters.AddWithValue("@PID", inventoryItem.inventory_PID);
            cmd.Parameters.AddWithValue("@VID", inventoryItem.inventory_VID);
            cmd.Parameters.AddWithValue("@SN", inventoryItem.inventory_SN);

            cmd.ExecuteNonQuery();
        }

        public List<InventoryModel> GetInventoryByDeviceId(int deviceId)
        {
            var inventoryList = new List<InventoryModel>();

     

            var query = "SELECT * FROM device_inventory WHERE device_id = @DeviceId";
            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@DeviceId", deviceId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var item = new InventoryModel
                {
                    device_id = reader.GetInt32("device_id"),
                    inventory_name = reader.GetString("inventory_name"),
                    inventory_description = reader.GetString("inventory_description"),
                    inventory_PID = reader.GetString("inventory_PID"),
                    inventory_VID = reader.GetString("inventory_VID"),
                    inventory_SN = reader.GetString("inventory_SN")
                };
                inventoryList.Add(item);
            }

            return inventoryList;
        }


        public int GetDeviceIdByIp(string deviceIp)
        {


            var query = "SELECT device_id FROM device WHERE device_ip = @ip LIMIT 1;";
            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@ip", deviceIp);

            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : -1;  // return -1 if not found
        }

        public async Task<(List<string> portTypes, List<string> portVlans)> GetPortTypeAndVlan(HttpClient client, string deviceIp)
        {
            var portTypes = new List<string>();
            var portVlans = new List<string>();

            var response = await client.GetAsync($"https://{deviceIp}/restconf/data/Cisco-IOS-XE-native:native/interface");
            if (!response.IsSuccessStatusCode)
                return (portTypes, portVlans);

            var json = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(json);

            var interfaces = jObject["Cisco-IOS-XE-native:interface"]?["GigabitEthernet"] as JArray;
            if (interfaces == null) return (portTypes, portVlans);

            // Sort interfaces by GigabitEthernet name
            var sortedInterfaces = interfaces
                .OrderBy(iface => ExtractPortSortKey($"GigabitEthernet{iface["name"]}"))
                .ToList();

            foreach (var port in sortedInterfaces)
            {
                string type = "";
                string vlan = "";

                var switchport = port["switchport-config"]?["switchport"];

                if (switchport != null)
                {
                    var accessMode = switchport["Cisco-IOS-XE-switch:mode"]?["access"];
                    var trunkMode = switchport["Cisco-IOS-XE-switch:mode"]?["trunk"];

                    if (accessMode != null)
                    {
                        type = "access";
                        vlan = switchport["Cisco-IOS-XE-switch:access"]?["vlan"]?["vlan"]?.ToString()
                            ?? switchport["Cisco-IOS-XE-switch:access"]?["vlan"]?.ToString()
                            ?? "";
                    }
                    else if (trunkMode != null)
                    {
                        type = "trunk";
                        // Optionally handle trunk allowed VLANs here
                    }
                    else
                    {
                        type = "switchport";
                    }
                }

                portTypes.Add(type);
                portVlans.Add(vlan);

                // Optional logs
                Console.WriteLine($"Parsed Type: {type}, VLAN: {vlan}");
            }

            return (portTypes, portVlans);
        }

    

    
    public async Task<List<InventoryModel>> GetInventoryOverSsh(string host, string username, string password)
        {
            var inventory = new List<InventoryModel>();

            using (var sshClient = new SshClient(host, username, password))
            {
                sshClient.Connect();
                if (!sshClient.IsConnected)
                {
                    Console.WriteLine("SSH connection failed.");
                    return inventory;
                }

                var cmd = sshClient.RunCommand("show inventory");
                var output = cmd.Result;
                sshClient.Disconnect();

                var blocks = Regex.Split(output, @"(?=^NAME:)", RegexOptions.Multiline);

                foreach (var block in blocks)
                {
                    var nameMatch = Regex.Match(block, @"NAME:\s*""(?<name>[^""]+)"",\s*DESCR:\s*""(?<desc>[^""]+)""", RegexOptions.Multiline);
                    var pidMatch = Regex.Match(block, @"PID:\s*(?<pid>\S+)\s*,\s*VID:\s*(?<vid>\S+)\s*,\s*SN:\s*(?<sn>\S+)", RegexOptions.Multiline);

                    if (nameMatch.Success && pidMatch.Success)
                    {
                        inventory.Add(new InventoryModel
                        {
                            inventory_name = nameMatch.Groups["name"].Value,
                            inventory_description = nameMatch.Groups["desc"].Value,
                            inventory_PID = pidMatch.Groups["pid"].Value,
                            inventory_VID = pidMatch.Groups["vid"].Value,
                            inventory_SN = pidMatch.Groups["sn"].Value
                        });
                    }
                }
            }

            return inventory;
        }
        public async Task<configModel> GetRunningConfigOverSsh(string host, string username, string password)
        {
            using var sshClient = new SshClient(host, username, password);
            sshClient.Connect();

            if (!sshClient.IsConnected)
            {
                Console.WriteLine("SSH connection failed.");
                return null;
            }

            var cmd = sshClient.RunCommand("show running-config");
            var output = cmd.Result;
            Console.WriteLine("=== Running Config Start ===");
            Console.WriteLine(output);
            Console.WriteLine("=== Running Config End ===");
            sshClient.Disconnect();

            return new configModel
            {
                config_info = output
            };
        }
        public void AddDeviceConfiguration(configModel config)
        {
   

            var query = @"INSERT INTO device_configurations (config_info, device_id)
                  VALUES (@ConfigInfo, @DeviceId);";

            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@ConfigInfo", config.config_info);
            cmd.Parameters.AddWithValue("@DeviceId", config.device_id);

            cmd.ExecuteNonQuery();
        }
        public configModel GetConfigurationByDeviceId(int deviceId)
        {
        
            var query = "SELECT * FROM device_configurations WHERE device_id = @DeviceId ORDER BY config_id DESC LIMIT 1";
            using var cmd = new MySqlCommand(query, _connection);
            cmd.Parameters.AddWithValue("@DeviceId", deviceId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new configModel
                {
                    config_id = reader.GetInt32("config_id"),
                    config_info = reader.GetString("config_info"),
                    device_id = reader.GetInt32("device_id")
                };
            }

            return null;
        }



    }


}



