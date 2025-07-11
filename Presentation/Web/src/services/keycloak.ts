// Keycloak configuration
export const keycloakConfig = {
  url: 'http://localhost:8082',
  realm: 'systeminstaller',
  clientId: 'systeminstaller-client',
};

// Keycloak instance - will be initialized in App.tsx
export let keycloak: any = null;

// Initialize Keycloak
export const initKeycloak = async () => {
  // Note: In a real application, you would import Keycloak like this:
  // import Keycloak from 'keycloak-js';
  // 
  // For now, this is a placeholder that shows the structure.
  // You need to run: npm install keycloak-js
  
  try {
    // keycloak = new Keycloak(keycloakConfig);
    // const authenticated = await keycloak.init({
    //   onLoad: 'check-sso',
    //   silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html',
    //   checkLoginIframe: false,
    // });
    
    // For development without Keycloak running, return a mock
    const mockAuthenticated = localStorage.getItem('mock-auth') === 'true';
    
    if (!mockAuthenticated) {
      // Mock login for development
      localStorage.setItem('mock-auth', 'true');
      localStorage.setItem('mock-user', JSON.stringify({
        username: 'admin',
        email: 'admin@systeminstaller.com',
        firstName: 'System',
        lastName: 'Administrator',
        roles: ['admin', 'user']
      }));
    }
    
    // Create mock keycloak object
    keycloak = {
      authenticated: true,
      token: 'mock-token',
      tokenParsed: {
        preferred_username: 'admin',
        email: 'admin@systeminstaller.com',
        given_name: 'System',
        family_name: 'Administrator',
        realm_access: {
          roles: ['admin', 'user']
        }
      },
      login: () => {
        localStorage.setItem('mock-auth', 'true');
        window.location.reload();
      },
      logout: () => {
        localStorage.removeItem('mock-auth');
        localStorage.removeItem('mock-user');
        window.location.reload();
      },
      updateToken: () => Promise.resolve(true),
      hasRealmRole: (role: string) => {
        const user = JSON.parse(localStorage.getItem('mock-user') || '{}');
        return user.roles?.includes(role) || false;
      }
    };
    
    return true;
  } catch (error) {
    console.error('Failed to initialize Keycloak:', error);
    return false;
  }
};

// Get current user info
export const getCurrentUser = () => {
  if (!keycloak) return null;
  
  return {
    username: keycloak.tokenParsed?.preferred_username,
    email: keycloak.tokenParsed?.email,
    firstName: keycloak.tokenParsed?.given_name,
    lastName: keycloak.tokenParsed?.family_name,
    roles: keycloak.tokenParsed?.realm_access?.roles || [],
  };
};

// Check if user has specific role
export const hasRole = (role: string): boolean => {
  if (!keycloak) return false;
  return keycloak.hasRealmRole(role);
};

// Get auth token for API calls
export const getToken = (): string | undefined => {
  return keycloak?.token;
};

// Login function
export const login = () => {
  if (keycloak) {
    keycloak.login();
  }
};

// Logout function
export const logout = () => {
  if (keycloak) {
    keycloak.logout();
  }
};

// Update token before expiry
export const updateToken = async (): Promise<boolean> => {
  if (!keycloak) return false;
  
  try {
    return await keycloak.updateToken(30); // Refresh if expires in 30 seconds
  } catch (error) {
    console.error('Failed to refresh token:', error);
    return false;
  }
};
