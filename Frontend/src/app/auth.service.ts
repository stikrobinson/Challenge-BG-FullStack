import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { tap } from 'rxjs';

export interface Credentials {
  email: string;
  password: string;
}

export interface Registration {
  nombre: string;
  apellido: string;
  email: string;
  password: string;
}

export interface AuthUser {
  usuarioId: number;
  email: string;
  nombre: string;
  apellido: string;
}

interface AuthResponse extends AuthUser {
  token: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'inventario-token';
  private readonly userKey = 'inventario-usuario';

  constructor(private readonly http: HttpClient) {}

  get token(): string | null {
    return sessionStorage.getItem(this.tokenKey);
  }

  get user(): AuthUser | null {
    const storedUser = sessionStorage.getItem(this.userKey);
    return storedUser ? (JSON.parse(storedUser) as AuthUser) : null;
  }

  login(credentials: Credentials) {
    return this.http.post<AuthResponse>('/api/auth/login', credentials).pipe(
      tap((response) => {
        sessionStorage.setItem(this.tokenKey, response.token);
        const { usuarioId, email, nombre, apellido } = response;
        sessionStorage.setItem(
          this.userKey,
          JSON.stringify({ usuarioId, email, nombre, apellido }),
        );
      }),
    );
  }

  register(registration: Registration) {
    return this.http.post<{ mensaje: string }>(
      '/api/auth/registro',
      registration,
    );
  }

  logout(): void {
    sessionStorage.removeItem(this.tokenKey);
    sessionStorage.removeItem(this.userKey);
  }

  get isAuthenticated(): boolean {
    return Boolean(this.token);
  }
}
