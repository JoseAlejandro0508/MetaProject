import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiUrlService {
  private readonly baseUrl = environment.apiUrl;

  /**
   * Construye una URL completa para la API.
   * Uso: this.apiUrl.url('User/Login') → '/api/User/Login' (producción)
   *                                     → 'https://meta-api-production-3abd.up.railway.app/api/User/Login' (dev)
   */
  url(endpoint: string): string {
    // Eliminar slash inicial del endpoint si existe para evitar doble slash
    const cleanEndpoint = endpoint.startsWith('/') ? endpoint.slice(1) : endpoint;
    return `${this.baseUrl}/${cleanEndpoint}`;
  }
}