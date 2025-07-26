import { Component, OnInit } from '@angular/core';
import { DeviceService } from '../../services/device.service';
import { MatSnackBar } from '@angular/material/snack-bar';
@Component({
  selector: 'app-portdetails',
  standalone: false,
  templateUrl: './portdetails.component.html',
  styleUrl: './portdetails.component.css'
})
export class PortdetailsComponent implements OnInit {
  deviceIp: string = '';
  devices: any[] = [];
  selectedDevice: any = null;

  constructor(private snackBar: MatSnackBar, private deviceService: DeviceService) { }

  ngOnInit(): void {
    this.fetchAllDevices();
  }

  fetchAllDevices(): void {
    this.deviceService.getAllDevices().subscribe({
      next: (data) => {
        this.devices = data;
      },
      error: (err) => {
        console.error('Error fetching devices:', err);
      }
    });
  }

  openAddDevice(): void {
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
        this.snackBar.open(err.error?.message || 'Failed to add device.', 'Close', {
          duration: 3000,
          panelClass: ['snackbar-error']
        });
      }
    });
  }

  openRemoveDevice(): void {
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
        this.snackBar.open(err.error?.message || 'Failed to remove device.', 'Close', {
          duration: 3000,
          panelClass: ['snackbar-error']
        });
      }
    });
  }

  selectDevice(device: any): void {
    this.selectedDevice = device;
  }

  getPortList(): { number: string, state: string, description: string, type: string, vlan: string }[] {
    if (!this.selectedDevice?.portNumber || !this.selectedDevice?.portState || !this.selectedDevice?.port_description ||
      !this.selectedDevice?.port_type ||
      !this.selectedDevice?.port_vlan) {
      return [];
    }

    const numbers = this.selectedDevice.portNumber.split(',');
    const states = this.selectedDevice.portState.split(',');
    const descriptions = this.selectedDevice.port_description.split(',');
    const types = this.selectedDevice.port_type.split(',');
    const vlans = this.selectedDevice.port_vlan.split(',');
    return numbers.map((num: string, idx: number) => ({
      number: num.trim(),
      state: states[idx]?.trim() || 'unknown',
      description: descriptions[idx]?.trim() || '',
      type: types[idx]?.trim() || '',
      vlan: vlans[idx]?.trim() || ''
    }));
  }

}
