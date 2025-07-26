using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using SwitchAutomation.Server.Models;
using SwitchAutomation.Server.Repository;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using MySqlX.XDevAPI;
using Microsoft.AspNetCore.Identity.Data;

namespace SwitchAutomation.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : Controller
    {
        private readonly string _connString = "server=127.0.0.1;database=automation;userid=root;password=c9nbQ5yMX2E9WVW;port=3306";
        private readonly DeviceRepository _deviceRepository;
        public DeviceController(DeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        [HttpGet("devices/{ip}/ports")]
        public async Task<IActionResult> GetPorts(string ip)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            var client = new HttpClient(handler);
            var byteArray = Encoding.ASCII.GetBytes("admin:admin");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var url = $"https://{ip}/restconf/data/ietf-interfaces:interfaces";

            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

                var content = await response.Content.ReadAsStringAsync();
                return Ok(content);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddDevice([FromBody] string deviceInput)
        {
            try
            {
                string deviceIp = deviceInput;

                if (!IPAddress.TryParse(deviceInput, out _))
                    deviceIp = await ResolveHostnameToIp(deviceInput);

                var normalizedInput = NormalizeHostname(deviceInput);

                if (_deviceRepository.DeviceExistsByIpOrHostname(deviceIp) ||
                    _deviceRepository.DeviceExistsByIpOrHostname(normalizedInput))
                {
                    return Conflict(new { message = $"Device with IP or Hostname '{deviceInput}' already exists." });
                }

                using var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
                using var client = new HttpClient(handler);

                var byteArray = Encoding.ASCII.GetBytes("admin:admin");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/yang-data+json"));

                var (hostnameSuccess, hostname) = await TryFetchHostname(client, deviceIp);
                if (!hostnameSuccess || string.IsNullOrEmpty(hostname)) hostname = "Unknown";

                var (modelSuccess, deviceModel) = await TryFetchModel(client, deviceIp);
                if (!modelSuccess || string.IsNullOrEmpty(deviceModel)) deviceModel = "Unknown";

                var portNumber = await _deviceRepository.GetPhysicalPortNames(client, deviceIp);
                var portState = await _deviceRepository.GetPhysicalPortStates(client, deviceIp);

                var licenseInfo = _deviceRepository.GetDeviceLicenseInfo(deviceIp, "admin", "admin");
                var OSVersion = await _deviceRepository.GetDeviceOsVersion(deviceIp, "admin", "admin");
                var serialNumber = _deviceRepository.GetDeviceSerialNumber(deviceIp, "admin", "admin");

                var (port_description, port_type, portVlans) = await _deviceRepository.GetPhysicalPortDetails(client, deviceIp);
                var (port_type1, portVlans1) = await _deviceRepository.GetPortTypeAndVlan(client, deviceIp);

                // Save the device first
                var device = new DeviceModel
                {
                    DeviceIp = deviceIp,
                    DeviceHostname = hostname,
                    AmountPort = portNumber.Count,
                    PortNumber = string.Join(",", portNumber),
                    PortState = string.Join(",", portState),
                    device_model = deviceModel,
                    device_license_level = licenseInfo,
                    OSVersion = OSVersion,
                    device_serialnum = serialNumber,
                    port_description = string.Join(",", port_description),
                    port_type = string.Join(",", port_type1),
                    port_vlan = string.Join(",", portVlans1),
                };

                _deviceRepository.AddDevice(device);

                int newDeviceId = _deviceRepository.GetDeviceIdByIp(device.DeviceIp); // Retrieve saved ID

                // Save inventory items
                var inventoryList = await _deviceRepository.GetInventoryOverSsh(device.DeviceIp, "admin", "admin");
                foreach (var item in inventoryList)
                {
                    item.device_id = newDeviceId;
                    _deviceRepository.AddInventoryItem(item);
                }

                // Save running-config
                var config = await _deviceRepository.GetRunningConfigOverSsh(device.DeviceIp, "admin", "admin");
                if (config != null)
                {
                    config.device_id = newDeviceId;
                    _deviceRepository.AddDeviceConfiguration(config);
                }

                return Ok(new { message = "Device, inventory, and configuration saved successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{deviceId}/configuration")]
        public IActionResult GetConfiguration(int deviceId)
        {
            try
            {
                var config = _deviceRepository.GetConfigurationByDeviceId(deviceId);

                if (config == null)
                {
                    return NotFound(new { message = "No configuration found for this device." });
                }

                return Ok(config);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving configuration.", error = ex.Message });
            }
        }


        [HttpGet("{deviceId}/inventory")]
        public IActionResult GetInventory(int deviceId)
        {
            try
            {
                var inventoryItems = _deviceRepository.GetInventoryByDeviceId(deviceId);
                return Ok(inventoryItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving inventory", error = ex.Message });
            }
        }



        // Get a specific device by IP
        [HttpGet("{deviceIp}")]
        public IActionResult GetDeviceByIp(string deviceIp)
        {
            try
            {
                var device = _deviceRepository.GetDeviceByIp(deviceIp);
                if (device == null)
                {
                    return NotFound($"Device with IP {deviceIp} not found.");
                }

                return Ok(device);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error occurred: {ex.Message}");
            }
        }

        [HttpGet("getDeviceIdByIp/{ip}")]
        public IActionResult GetDeviceIdByIp(string ip)
        {
            int deviceId = _deviceRepository.GetDeviceIdByIp(ip);
            if (deviceId == -1)
                return NotFound(new { message = "Device not found." });

            return Ok(deviceId);
        }


        [HttpGet("all")]
        public IActionResult GetAllDevices()
        {
            try
            {
                var devices = _deviceRepository.GetAllDevices();
                if (devices == null || devices.Count == 0)
                {
                    return NotFound("No devices found.");
                }

                return Ok(devices);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error occurred: {ex.Message}");
            }
        }
        [HttpDelete("{deviceInput}")]
        public async Task<IActionResult> DeleteDevice(string deviceInput)
        {
            if (string.IsNullOrWhiteSpace(deviceInput))
            {
                return BadRequest(new { message = "IP address or hostname is required." });
            }

            string deviceIp = deviceInput;

            // Try resolving hostname to IP
            try
            {
                var addresses = await Dns.GetHostAddressesAsync(deviceInput);
                if (addresses.Length > 0)
                {
                    deviceIp = addresses[0].ToString();
                }
            }
            catch (Exception)
            {
                // Ignore: assume deviceInput is already an IP
            }

            try
            {
                var deleted = _deviceRepository.DeleteDeviceAndRelatedDataByIp(deviceIp);
                if (!deleted)
                {
                    return NotFound(new { message = $"Device with IP {deviceIp} not found." });
                }

                return Ok(new { message = "Device deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private async Task<(bool Success, string Hostname)> TryFetchHostname(HttpClient client, string ip)
        {
            try
            {
                var url = $"https://{ip}/restconf/data/Cisco-IOS-XE-native:native/hostname";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return (false, null);
                }

                var json = await response.Content.ReadAsStringAsync();

                // Let's look at the JSON returned
                var parsedJson = JsonConvert.DeserializeObject<JObject>(json);

                // Now extract the hostname
                var hostname = parsedJson?["Cisco-IOS-XE-native:hostname"]?.ToString();

                return (true, hostname);
            }
            catch (Exception)
            {
                return (false, null);
            }
        }



        private async Task<(bool success, string? model)> TryFetchModel(HttpClient client, string ip)
        {
            try
            {
                var url = $"https://{ip}/restconf/data/Cisco-IOS-XE-device-hardware-oper:device-hardware-data";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to fetch model, status code: {response.StatusCode}");
                    return (false, null);
                }

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Device-Hardware Response: {content}"); // Debugging output

                var json = JObject.Parse(content);

                // Extract model description (fixing array handling with [0])
                var description = json
                    .SelectToken("Cisco-IOS-XE-device-hardware-oper:device-hardware-data.entity-information[0].description")
                    ?.ToString();

                Console.WriteLine($"Extracted description: {description}"); // Debugging output

                return (description != null, description);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in TryFetchModel: {ex.Message}");
                return (false, null);
            }
        }




     
        private string NormalizeHostname(string hostname)
        {
            if (string.IsNullOrEmpty(hostname))
                return "";

            hostname = hostname.ToLower();

            var parts = hostname.Split('.');
            return parts[0]; // Only the base hostname (no domain)
        }

        private async Task<string> ResolveHostnameToIp(string hostname)
        {
            try
            {
                var addresses = await Dns.GetHostAddressesAsync(hostname);
                return addresses.FirstOrDefault()?.ToString() ?? hostname; // fallback to hostname if no IP found
            }
            catch
            {
                return hostname; // fallback to hostname if resolution fails
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserModel user)
        {
            bool isValid = _deviceRepository.ValidateLogin(user.username, user.password);
            return Ok(isValid);  // ✅ return true or false instead of a message string
        }


    }
}
