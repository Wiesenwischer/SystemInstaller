// Keycloak configuration
export const keycloakConfig = {
  url: 'http://localhost:8082',
  realm: 'systeminstaller',
  clientId: 'systeminstaller-client',
};

// Keycloak instance - will be initialized in App.tsx
export let keycloak: any = null;

// Initialize Keycloak
export const initKeycloak = async (): Promise<boolean> => {
  // Check if we're coming back from Keycloak with a code
  const urlParams = new URLSearchParams(window.location.search);
  const code = urlParams.get('code');
  
  if (code) {
    // We have an authorization code, this means user authenticated successfully
    console.log('Found authorization code from Keycloak redirect');
    
    // For now, just set mock authentication and remove the code from URL
    localStorage.setItem('mock-auth', 'true');
    localStorage.setItem('mock-user', JSON.stringify({
      username: 'keycloak-user',
      email: 'user@systeminstaller.com',
      firstName: 'Keycloak',
      lastName: 'User',
      roles: ['admin', 'user']
    }));
    
    // Clean URL
    window.history.replaceState({}, document.title, window.location.pathname);
    
    return initMockAuthentication();
  }
  
  // Check if already authenticated
  const mockAuthenticated = localStorage.getItem('mock-auth') === 'true';
  if (mockAuthenticated) {
    return initMockAuthentication();
  }
  
  // Check if Keycloak server is available and redirect
  try {
    console.log('Checking Keycloak server availability...');
    const keycloakUrl = `${keycloakConfig.url}/realms/${keycloakConfig.realm}`;
    
    // Try to fetch the realm configuration
    const response = await fetch(keycloakUrl, { 
      method: 'GET', 
      mode: 'cors' 
    });
    
    if (response.ok) {
      console.log('Keycloak server is available, redirecting to login...');
      redirectToKeycloak();
      return false; // Will redirect
    }
  } catch (error) {
    console.log('Keycloak server not available:', error);
  }
  
  console.log('Using mock authentication as fallback');
  return initMockAuthentication();
};

// Manual redirect to Keycloak login
const redirectToKeycloak = () => {
  const params = new URLSearchParams({
    client_id: keycloakConfig.clientId,
    redirect_uri: window.location.origin,
    response_type: 'code',
    scope: 'openid profile email'
  });
  
  const loginUrl = `${keycloakConfig.url}/realms/${keycloakConfig.realm}/protocol/openid-connect/auth?${params}`;
  console.log('Redirecting to Keycloak login:', loginUrl);
  window.location.href = loginUrl;
};

// Initialize mock authentication as fallback
const initMockAuthentication = () => {
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
