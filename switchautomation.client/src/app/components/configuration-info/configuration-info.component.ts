import { Component, OnInit } from '@angular/core';
import { DeviceService } from '../../services/device.service';
import { MatSnackBar } from '@angular/material/snack-bar';
@Component({
  selector: 'app-configuration-info',
  standalone: false,
  templateUrl: './configuration-info.component.html',
  styleUrl: './configuration-info.component.css'
})
export class ConfigurationInfoComponent implements OnInit {
  portData: string = '';
  devices: any[] = []; // You can type it better once you know what data structure your devices use
  selectedDevice: any = null;
  inventory: any[] = [];
  configInfo: string = '';


  constructor(private snackBar: MatSnackBar, private deviceService: DeviceService) { }
  ngOnInit(): void {
    this.fetchAllDevices();  // Fetch devices on component initialization
  }
  /*
  fetchPorts() {
    this.deviceService.getPorts('10.2.102.20').subscribe({
      next: data => this.portData = data,
      error: err => this.portData = 'Error: ' + err.message
    });
  }
  */
  deviceIp: string = '';
  fetchAllDevices(): void {
    console.log("Fetch all device called");
    this.deviceService.getAllDevices().subscribe(
      (data) => {
        this.devices = data;  // Store the fetched devices
      },
      (error) => {
        console.error('Error fetching devices:', error);
      }
    );
  }
  openAddDevice() {
    if (!this.deviceIp) {
      this.snackBar.open('Please enter a valid IP address.', 'Close', {
        duration: 3000,
        panelClass: ['snackbar-error']
      });
      return;
    }

    this.deviceService.addDevice(this.deviceIp).subscribe({
      next: () => {
        this.snackBar.open('Device added successfully!', 'Close', {
          duration: 3000,
          panelClass: ['snackbar-success']
        });
        this.deviceIp = '';
        this.fetchAllDevices(); // 🔄 Refresh
      },
      error: (err) => {
        this.snackBar.open(
          err.error?.message || 'Failed to add device.',
          'Close',
          {
            duration: 3000,
            panelClass: ['snackbar-error']
          }
        );
      }
    });
  }


  openRemoveDevice() {
    if (!this.deviceIp) {
      this.snackBar.open('Please enter a device IP to remove.', 'Close', {
        duration: 3000,
        panelClass: ['snackbar-error']
      });
      return;
    }

    this.deviceService.deleteDevice(this.deviceIp).subscribe({
      next: () => {
        this.snackBar.open('Device removed successfully!', 'Close', {
          duration: 3000,
          panelClass: ['snackbar-success']
        });
        this.deviceIp = '';
        this.fetchAllDevices(); // 🔄 Refresh
      },
      error: (err) => {
        this.snackBar.open(
          err.error?.message || 'Failed to remove device.',
          'Close',
          {
            duration: 3000,
            panelClass: ['snackbar-error']
          }
        );
      }
    });
  }
  selectDevice(device: any) {
    this.selectedDevice = device;
    this.inventory = [];  // Clear previous inventory view

    this.deviceService.getDeviceIdByIp(device.deviceIp).subscribe({
      next: (deviceId) => {
        console.log("deviceId: " + deviceId);

        // Fetch inventory
        this.deviceService.getInventory(deviceId).subscribe({
          next: (data) => {
            this.inventory = data;
          },
          error: (err) => {
            console.error('Error fetching inventory:', err);
          }
        });

        // ✅ Fetch configuration
        this.deviceService.getConfiguration(deviceId).subscribe({
          next: (data) => {
            this.configInfo = data.config_info; // assuming backend returns { config_info: "...full config..." }
            console.log("Config loaded");
          },
          error: (err) => {
            console.error('Error fetching config:', err);
            this.configInfo = 'Error fetching config';
          }
        });
      },
      error: (err) => {
        console.error('Error getting device ID by IP:', err);
      }
    });

  }
  downloadConfigAsTxt(): void {
    const blob = new Blob([this.configInfo], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = `${this.selectedDevice.deviceHostname || this.selectedDevice.deviceIp}_config.txt`;
    anchor.click();
    window.URL.revokeObjectURL(url);
  }

}
