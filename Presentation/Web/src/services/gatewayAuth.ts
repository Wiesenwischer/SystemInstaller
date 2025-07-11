// Gateway Authentication Service
// This service communicates with our Gateway authentication endpoints

export interface LoginRequest {
  username: string;
  password: string;
}

export interface UserInfo {
  sub: string;
  username: string;
  email: string;
  firstName?: string;
  lastName?: string;
  roles: string[];
}

export interface LoginResponse {
  success: boolean;
  user?: UserInfo;
  message?: string;
}

// Base URL for Gateway API
const GATEWAY_BASE_URL = 'http://localhost:5000';

class GatewayAuthService {
  private user: UserInfo | null = null;

  /**
   * Login with username and password
   */
  async login(username: string, password: string): Promise<LoginResponse> {
    try {
      console.log('Attempting login via Gateway...');
      
      const response = await fetch(`${GATEWAY_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include', // Important: Include cookies
        body: JSON.stringify({ username, password }),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `Login failed: ${response.status}`);
      }

      const result: LoginResponse = await response.json();
      
      if (result.success && result.user) {
        this.user = result.user;
        console.log('Login successful:', result.user.username);
      }

      return result;
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    }
  }

  /**
   * Logout
   */
  async logout(): Promise<void> {
    try {
      await fetch(`${GATEWAY_BASE_URL}/auth/logout`, {
        method: 'POST',
        credentials: 'include',
      });
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      this.user = null;
    }
  }

  /**
   * Get current user info from Gateway
   */
  async getCurrentUser(): Promise<UserInfo | null> {
    try {
      const response = await fetch(`${GATEWAY_BASE_URL}/auth/user`, {
        method: 'GET',
        credentials: 'include',
      });

      if (!response.ok) {
        if (response.status === 401) {
          // Not authenticated
          this.user = null;
          return null;
        }
        throw new Error(`Failed to get user info: ${response.status}`);
      }

      const userInfo: UserInfo = await response.json();
      this.user = userInfo;
      return userInfo;
    } catch (error) {
      console.error('Get user error:', error);
      this.user = null;
      return null;
    }
  }

  /**
   * Refresh authentication token
   */
  async refreshToken(): Promise<boolean> {
    try {
      const response = await fetch(`${GATEWAY_BASE_URL}/auth/refresh`, {
        method: 'POST',
        credentials: 'include',
      });

      if (!response.ok) {
        return false;
      }

      // Token refreshed successfully, get updated user info
      const userInfo = await this.getCurrentUser();
      return userInfo !== null;
    } catch (error) {
      console.error('Token refresh error:', error);
      return false;
    }
  }

  /**
   * Check if user has a specific role
   */
  hasRole(role: string): boolean {
    return this.user?.roles?.includes(role) ?? false;
  }

  /**
   * Get current user (cached)
   */
  getUser(): UserInfo | null {
    return this.user;
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return this.user !== null;
  }
}

// Export singleton instance
export const gatewayAuth = new GatewayAuthService();

// Export individual functions for compatibility
export const login = (username: string, password: string) => gatewayAuth.login(username, password);
export const logout = () => gatewayAuth.logout();
export const getCurrentUser = () => gatewayAuth.getCurrentUser();
export const refreshToken = () => gatewayAuth.refreshToken();
export const hasRole = (role: string) => gatewayAuth.hasRole(role);
export const isAuthenticated = () => gatewayAuth.isAuthenticated();
