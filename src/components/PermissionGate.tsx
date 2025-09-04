import { ReactNode } from 'react';
import { useAuth } from '../contexts/AuthContext';

interface PermissionGateProps {
  children: ReactNode;
  permissions?: string[];
  requireAll?: boolean; // If true, user must have ALL permissions; if false, user must have ANY permission
  fallback?: ReactNode;
}

export default function PermissionGate({
  children,
  permissions = [],
  requireAll = false,
  fallback = null
}: PermissionGateProps) {
  const { user } = useAuth();

  console.log('🔐 PermissionGate: Checking permissions:', permissions, {
    hasUser: !!user,
    userType: typeof user,
    hasPermissions: user ? Array.isArray(user.permissions) : false,
    permissionsCount: user?.permissions?.length || 0,
    hasRoles: user ? Array.isArray(user.roles) : false,
    rolesCount: user?.roles?.length || 0,
    userPermissions: user?.permissions,
    userRoles: user?.roles
  });

  // Comprehensive null checks
  if (!user || !user.permissions || !user.roles || !Array.isArray(user.permissions) || !Array.isArray(user.roles)) {
    console.log('🔐 PermissionGate: User data validation failed, showing fallback');
    return <>{fallback}</>;
  }

  if (permissions.length === 0) {
    console.log('🔐 PermissionGate: No permissions required, showing content');
    // No permissions required, show content
    return <>{children}</>;
  }

  const userPermissions = user.permissions || [];
  const userRoles = user.roles || [];

  console.log('🔐 PermissionGate: Checking permission requirements:', {
    requiredPermissions: permissions,
    userPermissions,
    requireAll,
    hasRequiredPermissions: requireAll
      ? permissions.every(p => userPermissions.includes(p))
      : permissions.some(p => userPermissions.includes(p))
  });

  if (requireAll) {
    // User must have ALL specified permissions
    const hasAllPermissions = permissions.every(permission =>
      userPermissions.includes(permission)
    );
    console.log('🔐 PermissionGate: Require ALL result:', hasAllPermissions ? '✅ SHOWING CONTENT' : '❌ SHOWING FALLBACK');
    return hasAllPermissions ? <>{children}</> : <>{fallback}</>;
  } else {
    // User must have ANY of the specified permissions
    const hasAnyPermission = permissions.some(permission =>
      userPermissions.includes(permission)
    );
    console.log('🔐 PermissionGate: Require ANY result:', hasAnyPermission ? '✅ SHOWING CONTENT' : '❌ SHOWING FALLBACK');
    return hasAnyPermission ? <>{children}</> : <>{fallback}</>;
  }
}

// Helper component for role-based rendering
interface RoleGateProps {
  children: ReactNode;
  roles?: string[];
  requireAll?: boolean;
  fallback?: ReactNode;
}

export function RoleGate({
  children,
  roles = [],
  requireAll = false,
  fallback = null
}: RoleGateProps) {
  const { user } = useAuth();

  if (!user || !user.roles || !Array.isArray(user.roles)) {
    return <>{fallback}</>;
  }

  if (roles.length === 0) {
    return <>{children}</>;
  }

  if (requireAll) {
    const hasAllRoles = roles.every(role => user.roles.includes(role));
    return hasAllRoles ? <>{children}</> : <>{fallback}</>;
  } else {
    const hasAnyRole = roles.some(role => user.roles.includes(role));
    return hasAnyRole ? <>{children}</> : <>{fallback}</>;
  }
}