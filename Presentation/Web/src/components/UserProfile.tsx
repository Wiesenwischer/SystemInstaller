import React from 'react';
import { useAuth } from '../contexts/AuthContextOIDC';

const UserProfile: React.FC = () => {
  const { user, logout, hasRole } = useAuth();

  if (!user) {
    return null;
  }

  return (
    <div className="bg-white shadow rounded-lg p-6">
      <div className="flex items-center space-x-4 mb-6">
        <div className="w-16 h-16 bg-blue-500 rounded-full flex items-center justify-center text-white text-xl font-semibold">
          {user.firstName?.[0]}{user.lastName?.[0]}
        </div>
        <div>
          <h2 className="text-xl font-semibold text-gray-900">
            {user.firstName} {user.lastName}
          </h2>
          <p className="text-gray-600">{user.email}</p>
          <p className="text-sm text-gray-500">@{user.username}</p>
        </div>
      </div>

      <div className="border-t border-gray-200 pt-6">
        <h3 className="text-lg font-medium text-gray-900 mb-4">Roles & Permissions</h3>
        <div className="flex flex-wrap gap-2 mb-6">
          {user.roles.map((role) => (
            <span
              key={role}
              className={`px-3 py-1 rounded-full text-sm font-medium ${
                role === 'admin'
                  ? 'bg-red-100 text-red-800'
                  : 'bg-blue-100 text-blue-800'
              }`}
            >
              {role.charAt(0).toUpperCase() + role.slice(1)}
            </span>
          ))}
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
          <div className="flex items-center space-x-3">
            <div className={`w-3 h-3 rounded-full ${hasRole('admin') ? 'bg-green-500' : 'bg-gray-300'}`}></div>
            <span className="text-sm text-gray-700">Administrator Access</span>
          </div>
          <div className="flex items-center space-x-3">
            <div className={`w-3 h-3 rounded-full ${hasRole('user') ? 'bg-green-500' : 'bg-gray-300'}`}></div>
            <span className="text-sm text-gray-700">User Access</span>
          </div>
        </div>

        <div className="flex space-x-4">
          <button
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
            onClick={() => {
              // In a real app, this would open an edit profile modal
              alert('Edit profile functionality would be implemented here');
            }}
          >
            Edit Profile
          </button>
          <button
            className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500"
            onClick={logout}
          >
            Logout
          </button>
        </div>
      </div>

      <div className="border-t border-gray-200 pt-6 mt-6">
        <h3 className="text-lg font-medium text-gray-900 mb-4">Session Information</h3>
        <div className="text-sm text-gray-600 space-y-2">
          <div>
            <span className="font-medium">Status:</span> Active
          </div>
          <div>
            <span className="font-medium">Login Time:</span> {new Date().toLocaleString()}
          </div>
          <div>
            <span className="font-medium">Session:</span> Protected by Keycloak
          </div>
        </div>
      </div>
    </div>
  );
};

export default UserProfile;
