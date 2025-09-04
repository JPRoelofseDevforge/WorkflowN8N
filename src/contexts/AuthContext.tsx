import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import n8nApi from '../services/n8nApi';

export interface User {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  permissions: string[];
  preferences: {
    theme: 'light' | 'dark';
    notifications: boolean;
    language: string;
  };
}

export interface AuthContextType {
  user: User | null;
  login: (username: string, password: string) => Promise<void>;
  register: (username: string, email: string, firstName: string, lastName: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  updateProfile: (updates: Partial<Pick<User, 'firstName' | 'lastName' | 'email'>>) => Promise<void>;
  changePassword: (currentPassword: string, newPassword: string) => Promise<void>;
  updatePreferences: (preferences: Partial<User['preferences']>) => Promise<void>;
  loading: boolean;
  error: string | null;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);


export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Check for existing tokens on mount
  useEffect(() => {
    console.log('🔍 AuthContext: Checking for existing tokens on mount');

    const accessToken = localStorage.getItem('accessToken');
    const userData = localStorage.getItem('user');
    const refreshToken = localStorage.getItem('refreshToken');

    console.log('🔍 AuthContext: localStorage values:', {
      accessToken: accessToken ? `${accessToken.substring(0, 20)}...` : null,
      userData: userData ? `${userData.substring(0, 50)}...` : null,
      refreshToken: refreshToken ? `${refreshToken.substring(0, 20)}...` : null,
      userDataType: typeof userData,
      userDataLength: userData?.length,
      userDataIsUndefined: userData === 'undefined',
      userDataIsNull: userData === 'null'
    });

    if (accessToken && userData && userData !== 'undefined' && userData !== 'null') {
      console.log('🔍 AuthContext: Attempting to parse user data');
      try {
        const parsedUser = JSON.parse(userData);
        console.log('✅ AuthContext: Successfully parsed user data:', {
          id: parsedUser.id,
          username: parsedUser.username,
          hasPermissions: Array.isArray(parsedUser.permissions),
          permissionsCount: parsedUser.permissions?.length || 0,
          hasPreferences: !!parsedUser.preferences,
          preferencesKeys: parsedUser.preferences ? Object.keys(parsedUser.preferences) : []
        });

        // Ensure user has permissions and preferences
        const userWithDefaults = {
          ...parsedUser,
          permissions: parsedUser.permissions || [],
          preferences: parsedUser.preferences || { theme: 'light', notifications: true, language: 'en' }
        };

        console.log('✅ AuthContext: Setting user with defaults:', {
          permissionsCount: userWithDefaults.permissions.length,
          preferencesKeys: Object.keys(userWithDefaults.preferences)
        });

        setUser(userWithDefaults);
        // Set n8nApi auth header
        n8nApi.setAuthToken(accessToken);
        console.log('✅ AuthContext: User authentication setup complete');
      } catch (err) {
        console.error('❌ AuthContext: Failed to parse user data:', err);
        console.error('❌ AuthContext: Raw userData:', userData);
        console.error('❌ AuthContext: userData type:', typeof userData);
        console.error('❌ AuthContext: userData length:', userData?.length);

        // Clear corrupted data
        console.log('🧹 AuthContext: Clearing corrupted localStorage data');
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
      }
    } else {
      console.log('🔍 AuthContext: Skipping user data parsing - missing or invalid data');
      if (!accessToken) console.log('  - Missing accessToken');
      if (!userData) console.log('  - Missing userData');
      if (userData === 'undefined') console.log('  - userData is "undefined" string');
      if (userData === 'null') console.log('  - userData is "null" string');
    }
  }, []);

  const login = async (username: string, password: string): Promise<void> => {
    console.log('🔐 AuthContext: Starting login process for:', username);
    setLoading(true);
    setError(null);

    try {
      console.log('🔐 AuthContext: Calling n8nApi.login');
      const response = await n8nApi.login({
        Username: username,
        Password: password,
      });

      console.log('🔐 AuthContext: Login response received:', {
        hasAccessToken: !!response.accessToken,
        hasRefreshToken: !!response.refreshToken,
        hasUser: !!response.user,
        userType: typeof response.user,
        userKeys: response.user ? Object.keys(response.user) : [],
        responseKeys: Object.keys(response)
      });

      // Validate response structure
      if (!response || !response.accessToken || !response.user) {
        console.error('🔐 AuthContext: Invalid login response structure');
        throw new Error('Invalid login response from server');
      }

      console.log('🔐 AuthContext: Storing tokens and user data');
      localStorage.setItem('accessToken', response.accessToken);
      localStorage.setItem('refreshToken', response.refreshToken);

      const userJson = JSON.stringify(response.user);
      console.log('🔐 AuthContext: User JSON to store:', userJson.substring(0, 200) + '...');
      localStorage.setItem('user', userJson);

      // Verify storage
      const storedUser = localStorage.getItem('user');
      console.log('🔐 AuthContext: Verification - stored user:', storedUser ? storedUser.substring(0, 100) + '...' : 'null');

      // Set n8nApi auth header
      n8nApi.setAuthToken(response.accessToken);

      // Ensure user has permissions and preferences with safe access
      const userWithDefaults = {
        ...response.user,
        permissions: response.user.permissions || [],
        preferences: response.user.preferences || { theme: 'light', notifications: true, language: 'en' }
      };

      console.log('🔐 AuthContext: Setting user with defaults:', {
        permissionsCount: userWithDefaults.permissions?.length || 0,
        preferencesKeys: userWithDefaults.preferences ? Object.keys(userWithDefaults.preferences) : []
      });

      setUser(userWithDefaults);
      console.log('✅ AuthContext: Login process completed successfully');
    } catch (err: any) {
      console.error('❌ AuthContext: Login failed:', err);
      const message = err.message || 'Login failed';
      setError(message);
      throw new Error(message);
    } finally {
      setLoading(false);
    }
  };

  const register = async (username: string, email: string, firstName: string, lastName: string, password: string): Promise<void> => {
    setLoading(true);
    setError(null);

    try {
      const response = await n8nApi.register({
        Username: username,
        Email: email,
        FirstName: firstName,
        LastName: lastName,
        Password: password,
      });

      // Validate response structure
      if (!response || !response.accessToken || !response.user) {
        console.error('👤 AuthContext: Invalid registration response structure');
        throw new Error('Invalid registration response from server');
      }

      console.log('👤 AuthContext: Storing registration tokens and user data');
      localStorage.setItem('accessToken', response.accessToken);
      localStorage.setItem('refreshToken', response.refreshToken);

      const userJson = JSON.stringify(response.user);
      console.log('👤 AuthContext: Registration user JSON to store:', userJson.substring(0, 200) + '...');
      localStorage.setItem('user', userJson);

      // Verify storage
      const storedUser = localStorage.getItem('user');
      console.log('👤 AuthContext: Registration verification - stored user:', storedUser ? storedUser.substring(0, 100) + '...' : 'null');

      // Set n8nApi auth header
      n8nApi.setAuthToken(response.accessToken);

      // Ensure user has permissions and preferences with safe access
      const userWithDefaults = {
        ...response.user,
        permissions: response.user.permissions || [],
        preferences: response.user.preferences || { theme: 'light', notifications: true, language: 'en' }
      };

      setUser(userWithDefaults);
    } catch (err: any) {
      const message = err.message || 'Registration failed';
      setError(message);
      throw new Error(message);
    } finally {
      setLoading(false);
    }
  };

  const logout = async (): Promise<void> => {
    console.log('🚪 AuthContext: Starting logout process');
    setLoading(true);
    setError(null);

    try {
      console.log('🚪 AuthContext: Calling n8nApi.logout');
      await n8nApi.logout();
      console.log('✅ AuthContext: n8nApi.logout completed');
    } catch (err: any) {
      console.error('❌ AuthContext: Logout API call failed:', err);
      // Continue with local logout even if API fails
    } finally {
      console.log('🧹 AuthContext: Clearing localStorage');
      // Clear local storage
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');

      // Verify cleanup
      const accessTokenAfter = localStorage.getItem('accessToken');
      const userDataAfter = localStorage.getItem('user');
      console.log('🧹 AuthContext: Cleanup verification:', {
        accessTokenCleared: accessTokenAfter === null,
        userDataCleared: userDataAfter === null,
        userDataValue: userDataAfter
      });

      // Remove n8nApi auth header
      n8nApi.setAuthToken(null);

      setUser(null);
      setLoading(false);
      console.log('✅ AuthContext: Logout process completed');
    }
  };

  const updateProfile = async (updates: Partial<Pick<User, 'firstName' | 'lastName' | 'email'>>): Promise<void> => {
    if (!user) throw new Error('No user logged in');

    setLoading(true);
    setError(null);

    try {
      // TODO: Implement API call to update profile
      // For now, update locally
      const updatedUser = { ...user, ...updates };
      setUser(updatedUser);
      if (updatedUser) {
        localStorage.setItem('user', JSON.stringify(updatedUser));
      }
    } catch (err: any) {
      const message = err.message || 'Profile update failed';
      setError(message);
      throw new Error(message);
    } finally {
      setLoading(false);
    }
  };

  const changePassword = async (currentPassword: string, newPassword: string): Promise<void> => {
    setLoading(true);
    setError(null);

    try {
      // TODO: Implement API call to change password
      // For now, just simulate success
      console.log('Password change requested');
    } catch (err: any) {
      const message = err.message || 'Password change failed';
      setError(message);
      throw new Error(message);
    } finally {
      setLoading(false);
    }
  };

  const updatePreferences = async (preferences: Partial<User['preferences']>): Promise<void> => {
    if (!user) throw new Error('No user logged in');

    setLoading(true);
    setError(null);

    try {
      const currentPrefs = user.preferences || { theme: 'light', notifications: true, language: 'en' };
      const updatedUser = {
        ...user,
        preferences: { ...currentPrefs, ...preferences } as User['preferences']
      };
      setUser(updatedUser);
      if (updatedUser) {
        localStorage.setItem('user', JSON.stringify(updatedUser));
      }
    } catch (err: any) {
      const message = err.message || 'Preferences update failed';
      setError(message);
      throw new Error(message);
    } finally {
      setLoading(false);
    }
  };

  const value: AuthContextType = {
    user,
    login,
    register,
    logout,
    updateProfile,
    changePassword,
    updatePreferences,
    loading,
    error,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}