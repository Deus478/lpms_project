import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { TokenStorageService, StoredAuthUser } from './token-storage.service';

export interface LoginRequestDto {
  email: string;
}

export interface LoginResponseDto {
  token: string;
  userId: number;
  email: string;
  fullName: string;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly loginUrl = `${environment.apiBaseUrl}/api/auth/login`;

  constructor(
    private http: HttpClient,
    private tokenStorage: TokenStorageService
  ) {}

  login(email: string): Observable<LoginResponseDto> {
    const payload: LoginRequestDto = { email };
    return this.http.post<LoginResponseDto>(this.loginUrl, payload).pipe(
      tap(res => {
        if (res?.token) {
          this.tokenStorage.setToken(res.token);
          const user: StoredAuthUser = {
            userId: res.userId,
            email: res.email,
            fullName: res.fullName,
            roles: res.roles || []
          };
          this.tokenStorage.setUser(user);
        }
      })
    );
  }

  logout(): void {
    this.tokenStorage.clear();
  }

  isLoggedIn(): boolean {
    return !!this.tokenStorage.getToken();
  }
}
