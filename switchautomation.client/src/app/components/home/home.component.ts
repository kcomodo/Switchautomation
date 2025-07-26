import { Component, OnInit } from '@angular/core';
import { DeviceService } from '../../services/device.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  portData: string = '';
  devices: any[] = []; // You can type it better once you know what data structure your devices use
  selectedDevice: any = null;
  inventory: any[] = [];
  ports: { number: string, state: string, description: string }[] = [];
  configInfo: string = '';
  portsTop: any[] = [];
  portsBottom: any[] = [];


  constructor(private snackBar: MatSnackBar, private deviceService: DeviceService) { }


  ngOnInit(): void {
    this.fetchAllDevices();  // Fetch devices on component initialization
  }

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
        this.fetchAllDevices(); 
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
        this.fetchAllDevices();
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
    this.inventory = [];

    this.deviceService.getDeviceIdByIp(device.deviceIp).subscribe({
      next: (deviceId) => {
        this.deviceService.getInventory(deviceId).subscribe({
          next: (data) => {
            this.inventory = data;
          },
          error: (err) => console.error('Error fetching inventory:', err)
        });

        this.deviceService.getConfiguration(deviceId).subscribe({
          next: (data) => {
            this.configInfo = data.config_info;
          },
          error: (err) => console.error('Error fetching config:', err)
        });
      },
      error: (err) => console.error('Error getting device ID by IP:', err)
    });

    // Build port array
    const numbers = device.portNumber?.split(',') || [];
    const states = device.portState?.split(',') || [];
    const descriptions = device.port_description?.split(',') || [];

    const allPorts = numbers.map((num: string, idx: number) => ({
      number: num.trim(),
      state: states[idx]?.trim() || 'unknown',
      description: descriptions[idx]?.trim() || ''
    }));

    this.portsTop = allPorts.filter((p: { number: string; state: string; description: string }, idx: number) => idx % 2 === 0);
    this.portsBottom = allPorts.filter((p: { number: string; state: string; description: string }, idx: number) => idx % 2 !== 0);


  }

  getPortClass(port: any): string {
    if (port.state === 'up') return 'port-up';
    if (port.state === 'down') return 'port-down';
    return 'port-unknown';
  }

  formatPortNumber(full: string): string {
    return full.replace('GigabitEthernet', '');
  }
  getShortPort(portNumber: string): string {
    return portNumber.replace(/(GigabitEthernet|TenGigabitEthernet|FortyGigabitEthernet|TwentyFiveGigE|FastEthernet)/, '');
  }

  getDisplayIndex(portNumber: string): string {
    const short = this.getShortPort(portNumber);
    const parts = short.split('/');
    return parts[parts.length - 1];  
  }



}
