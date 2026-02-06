import { Injectable } from '@angular/core';

export interface StoredAuthUser {
  userId: number;
  email: string;
  fullName: string;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class TokenStorageService {
  private readonly tokenKey = 'lpms_auth_token';
  private readonly userKey = 'lpms_auth_user';

  getToken(): string | null {
    try {
      return localStorage.getItem(this.tokenKey);
    } catch {
      return null;
    }
  }

  setToken(token: string): void {
    try {
      localStorage.setItem(this.tokenKey, token);
    } catch {
      // ignore storage errors
    }
  }

  clearToken(): void {
    try {
      localStorage.removeItem(this.tokenKey);
    } catch {
      // ignore storage errors
    }
  }

  getUser(): StoredAuthUser | null {
    try {
      const raw = localStorage.getItem(this.userKey);
      return raw ? (JSON.parse(raw) as StoredAuthUser) : null;
    } catch {
      return null;
    }
  }

  setUser(user: StoredAuthUser): void {
    try {
      localStorage.setItem(this.userKey, JSON.stringify(user));
    } catch {
      // ignore storage errors
    }
  }

  clearUser(): void {
    try {
      localStorage.removeItem(this.userKey);
    } catch {
      // ignore storage errors
    }
  }

  clear(): void {
    this.clearToken();
    this.clearUser();
  }
}
