import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';

interface UserInfo {
  sub: string;
  username: string;
  email: string;
  firstName?: string;
  lastName?: string;
  roles: string[];
}

interface AuthContextType {
  isAuthenticated: boolean;
  user: UserInfo | null;
  loading: boolean;
  login: () => void;
  logout: () => Promise<void>;
  hasRole: (role: string) => boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

// Gateway Authentication Service for OIDC flow
class GatewayOIDCService {
  private GATEWAY_BASE_URL = 'http://localhost:5000';

  async getCurrentUser(): Promise<UserInfo | null> {
    try {
      const response = await fetch(`${this.GATEWAY_BASE_URL}/auth/user`, {
        method: 'GET',
        credentials: 'include', // Include cookies
      });

      if (!response.ok) {
        if (response.status === 401) {
          return null; // Not authenticated
        }
        throw new Error(`Failed to get user info: ${response.status}`);
      }

      const userInfo: UserInfo = await response.json();
      return userInfo;
    } catch (error) {
      console.error('Get user error:', error);
      return null;
    }
  }

  redirectToLogin(): void {
    // Redirect to Gateway's OIDC login endpoint
    window.location.href = `${this.GATEWAY_BASE_URL}/auth/login`;
  }

  async logout(): Promise<void> {
    try {
      await fetch(`${this.GATEWAY_BASE_URL}/auth/logout`, {
        method: 'POST',
        credentials: 'include',
      });
      
      // Redirect to home page after logout
      window.location.href = '/';
    } catch (error) {
      console.error('Logout error:', error);
      // Even if logout fails, redirect to home
      window.location.href = '/';
    }
  }
}

const gatewayOIDC = new GatewayOIDCService();

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [user, setUser] = useState<UserInfo | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const initAuth = async () => {
      try {
        // Check if user is already authenticated via existing cookies
        const userInfo = await gatewayOIDC.getCurrentUser();
        
        if (userInfo) {
          setIsAuthenticated(true);
          setUser(userInfo);
          console.log('User already authenticated:', userInfo.username);
        } else {
          setIsAuthenticated(false);
          setUser(null);
        }
      } catch (error) {
        console.error('Authentication initialization failed:', error);
        setIsAuthenticated(false);
        setUser(null);
      } finally {
        setLoading(false);
      }
    };

    initAuth();
  }, []);

  const handleLogin = () => {
    // Redirect to Gateway's OIDC login (Keycloak)
    gatewayOIDC.redirectToLogin();
  };

  const handleLogout = async () => {
    try {
      await gatewayOIDC.logout();
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      setIsAuthenticated(false);
      setUser(null);
    }
  };

  const checkRole = (role: string): boolean => {
    return user?.roles?.includes(role) ?? false;
  };

  const contextValue: AuthContextType = {
    isAuthenticated,
    user,
    loading,
    login: handleLogin,
    logout: handleLogout,
    hasRole: checkRole,
  };

  return (
    <AuthContext.Provider value={contextValue}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
