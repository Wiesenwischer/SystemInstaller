import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { initKeycloak, getCurrentUser, hasRole, login, logout, updateToken } from '../services/keycloak';

interface User {
  username?: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  roles: string[];
}

interface AuthContextType {
  isAuthenticated: boolean;
  user: User | null;
  loading: boolean;
  login: () => void;
  logout: () => void;
  hasRole: (role: string) => boolean;
  refreshToken: () => Promise<boolean>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const initAuth = async () => {
      try {
        const authenticated = await initKeycloak();
        setIsAuthenticated(authenticated);
        
        if (authenticated) {
          const userInfo = getCurrentUser();
          setUser(userInfo);
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

  // Auto-refresh token
  useEffect(() => {
    if (!isAuthenticated) return;

    const interval = setInterval(async () => {
      try {
        await updateToken();
      } catch (error) {
        console.error('Token refresh failed:', error);
        // Token refresh failed, user needs to login again
        setIsAuthenticated(false);
        setUser(null);
      }
    }, 60000); // Check every minute

    return () => clearInterval(interval);
  }, [isAuthenticated]);

  const handleLogin = () => {
    login();
  };

  const handleLogout = () => {
    logout();
    setIsAuthenticated(false);
    setUser(null);
  };

  const checkRole = (role: string): boolean => {
    return hasRole(role);
  };

  const refreshToken = async (): Promise<boolean> => {
    return await updateToken();
  };

  const contextValue: AuthContextType = {
    isAuthenticated,
    user,
    loading,
    login: handleLogin,
    logout: handleLogout,
    hasRole: checkRole,
    refreshToken,
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
