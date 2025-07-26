import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
@Injectable({
  providedIn: 'root'
})
export class DeviceService {

  private baseUrl = 'http://localhost:7105/api/Device'; // adjust port if needed


  constructor(private http: HttpClient) { }

  getPorts(ip: string) {
    // Prepare the username:password string for basic auth
    const username = 'admin';
    const password = 'admin';
    const authHeader = 'Basic ' + btoa(username + ':' + password); // base64 encode the credentials

    const headers = new HttpHeaders({
      'Authorization': authHeader
    });

    // Send the request with the Authorization header
    return this.http.get(`${this.baseUrl}/devices/${ip}/ports`, {
      headers: headers,
      responseType: 'text'
    });
  }
  addDevice(device: string): Observable<any> {
    console.log(device);
    return this.http.post<any>(`${this.baseUrl}`, JSON.stringify(device), {
      headers: { 'Content-Type': 'application/json' }
    });
  }
  getAllDevices(): Observable<any> {
    console.log("get all device called");
    const username = 'admin';
    const password = 'admin';
    const authHeader = 'Basic ' + btoa(username + ':' + password); // base64 encode the credentials

    const headers = new HttpHeaders({
      'Authorization': authHeader
    });

    return this.http.get(`${this.baseUrl}/all`, {
      headers: headers
    });
  }
  deleteDevice(ip: string) {
    const username = 'admin';
    const password = 'admin';
    const authHeader = 'Basic ' + btoa(username + ':' + password);

    const headers = new HttpHeaders({
      'Authorization': authHeader
    });

    return this.http.delete(`${this.baseUrl}/${ip}`, { headers });
  }
  validateLogin(username: string, password: string) {
    return this.http.post<boolean>(`${this.baseUrl}/login`, {
      username,
      password
    });
  }
  getDeviceIdByIp(deviceIp: string): Observable<number> {
    const username = 'admin';
    const password = 'admin';
    const authHeader = 'Basic ' + btoa(username + ':' + password);

    const headers = new HttpHeaders({
      'Authorization': authHeader
    });

    return this.http.get<number>(`${this.baseUrl}/getDeviceIdByIp/${deviceIp}`, { headers });
  }

  getInventory(deviceId: number): Observable<any[]> {
    const username = 'admin';
    const password = 'admin';
    const authHeader = 'Basic ' + btoa(username + ':' + password);

    const headers = new HttpHeaders({
      'Authorization': authHeader
    });

    return this.http.get<any[]>(`${this.baseUrl}/${deviceId}/inventory`, { headers });
  }
  getConfiguration(deviceId: number): Observable<any> {
    const username = 'admin';
    const password = 'admin';
    const authHeader = 'Basic ' + btoa(username + ':' + password);

    const headers = new HttpHeaders({
      'Authorization': authHeader
    });

    return this.http.get<any>(`${this.baseUrl}/${deviceId}/configuration`, { headers });
  }


}
