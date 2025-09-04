import { useAuth } from '../contexts/AuthContext';

export default function AdminPage() {
  const { user } = useAuth();

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-gray-100">
      <div className="container mx-auto px-4 py-6 sm:py-8 max-w-7xl">
        <div className="bg-white rounded-lg shadow-sm p-6">
          <h1 className="text-2xl font-bold text-gray-900 mb-4">Admin Dashboard</h1>
          <p className="text-gray-600 mb-6">Welcome to the admin panel, {user?.firstName} {user?.lastName}</p>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div className="bg-blue-50 p-4 rounded-lg">
              <h3 className="font-semibold text-blue-900">User Management</h3>
              <p className="text-blue-700 text-sm">Manage users and their permissions</p>
            </div>
            <div className="bg-green-50 p-4 rounded-lg">
              <h3 className="font-semibold text-green-900">System Settings</h3>
              <p className="text-green-700 text-sm">Configure system-wide settings</p>
            </div>
            <div className="bg-purple-50 p-4 rounded-lg">
              <h3 className="font-semibold text-purple-900">Analytics</h3>
              <p className="text-purple-700 text-sm">View system analytics and reports</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}