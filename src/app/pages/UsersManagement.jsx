import { useEffect, useState } from "react";
import { Search, Eye, Ban, CheckCircle, RotateCcw } from "lucide-react";
import {
  fetchUsers,
  fetchUserDetails,
  toggleUserStatus,
  resetUserPassword,
} from "../../services/usersManagementService";

export default function UsersManagement() {
  const [activeTab, setActiveTab] = useState("Patient");
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("All");
  const [users, setUsers] = useState([]);
  const [patientCount, setPatientCount] = useState(0);
  const [nurseCount, setNurseCount] = useState(0);
  const [selectedUser, setSelectedUser] = useState(null);
  const [showResetPasswordModal, setShowResetPasswordModal] = useState(false);
  const [newPassword, setNewPassword] = useState("");
  const [loading, setLoading] = useState(true);
  const [detailsLoading, setDetailsLoading] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState("");

  const loadUsers = async () => {
    try {
      setLoading(true);
      setError("");

      const data = await fetchUsers(activeTab, statusFilter, searchTerm);
      setUsers(Array.isArray(data) ? data : []);
    } catch (err) {
      setError(err.message || "Failed to load users");
    } finally {
      setLoading(false);
    }
  };

  const loadCounts = async () => {
    try {
      const [patients, nurses] = await Promise.all([
        fetchUsers("Patient", "All", ""),
        fetchUsers("Nurse", "All", ""),
      ]);

      setPatientCount(Array.isArray(patients) ? patients.length : 0);
      setNurseCount(Array.isArray(nurses) ? nurses.length : 0);
    } catch {
      setPatientCount(0);
      setNurseCount(0);
    }
  };

  useEffect(() => {
    const delay = setTimeout(() => {
      loadUsers();
    }, 300);

    return () => clearTimeout(delay);
  }, [activeTab, statusFilter, searchTerm]);

  useEffect(() => {
    loadCounts();
  }, []);

  const getStatusBadge = (status) => {
    const styles = {
      Active: "bg-green-100 text-green-800",
      Suspended: "bg-red-100 text-red-800",
    };

    return (
      <span
        className={`px-3 py-1 rounded-full text-xs ${
          styles[status] || "bg-gray-100 text-gray-800"
        }`}
      >
        {status}
      </span>
    );
  };

  const handleViewProfile = async (user) => {
    try {
      setDetailsLoading(true);
      const details = await fetchUserDetails(user.id);
      setSelectedUser(details);
      setShowResetPasswordModal(false);
    } catch (err) {
      alert(err.message || "Failed to load user details");
    } finally {
      setDetailsLoading(false);
    }
  };

  const handleToggleStatus = async (user) => {
    try {
      setActionLoading(true);
      await toggleUserStatus(user.id);
      await loadUsers();
      await loadCounts();

      if (selectedUser && selectedUser.id === user.id) {
        const updatedDetails = await fetchUserDetails(user.id);
        setSelectedUser(updatedDetails);
      }
    } catch (err) {
      alert(err.message || "Failed to update user status");
    } finally {
      setActionLoading(false);
    }
  };

  const handleOpenResetPassword = (user) => {
    setSelectedUser(user);
    setNewPassword("");
    setShowResetPasswordModal(true);
  };

  const handleResetPassword = async () => {
    if (!selectedUser) return;

    if (!newPassword.trim()) {
      alert("Please enter a new password.");
      return;
    }

    try {
      setActionLoading(true);
      await resetUserPassword(selectedUser.id, newPassword);
      setShowResetPasswordModal(false);
      setNewPassword("");
      alert("Password reset successfully.");
    } catch (err) {
      alert(err.message || "Failed to reset password");
    } finally {
      setActionLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex flex-col md:flex-row gap-4 items-start md:items-center justify-between">
          <div>
            <h2 className="text-gray-800 mb-1">Users Management</h2>
            <p className="text-sm text-gray-500">
              Manage patients and nurses accounts
            </p>
          </div>

          <div className="flex gap-3 w-full md:w-auto">
            <div className="relative flex-1 md:flex-initial">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input
                type="text"
                placeholder="Search users..."
                className="pl-10 pr-4 py-2 border border-gray-300 rounded-lg w-full md:w-64 bg-white"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>

            <select
              className="px-4 py-2 border border-gray-300 rounded-lg bg-white"
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
            >
              <option value="All">All Status</option>
              <option value="Active">Active</option>
              <option value="Suspended">Suspended</option>
            </select>
          </div>
        </div>
      </div>

      {/* Error */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-600 rounded-lg p-4">
          {error}
        </div>
      )}

      {/* Tabs + Table */}
      <div className="bg-white rounded-lg shadow">
        <div className="border-b border-gray-200">
          <div className="flex">
            <button
              onClick={() => setActiveTab("Patient")}
              className={`px-6 py-3 text-sm transition-colors ${
                activeTab === "Patient"
                  ? "border-b-2 border-[#1F7A8C] text-[#1F7A8C]"
                  : "text-gray-600 hover:text-gray-800"
              }`}
            >
              Patients ({patientCount})
            </button>

            <button
              onClick={() => setActiveTab("Nurse")}
              className={`px-6 py-3 text-sm transition-colors ${
                activeTab === "Nurse"
                  ? "border-b-2 border-[#1F7A8C] text-[#1F7A8C]"
                  : "text-gray-600 hover:text-gray-800"
              }`}
            >
              Nurses ({nurseCount})
            </button>
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs text-gray-600">ID</th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">Name</th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">Email</th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">Phone</th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">Join Date</th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">Status</th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">Actions</th>
              </tr>
            </thead>

            <tbody className="divide-y divide-gray-200">
              {loading ? (
                <tr>
                  <td colSpan="7" className="px-6 py-6 text-sm text-gray-500 text-center">
                    Loading...
                  </td>
                </tr>
              ) : users.length === 0 ? (
                <tr>
                  <td colSpan="7" className="px-6 py-6 text-sm text-gray-500 text-center">
                    No users found.
                  </td>
                </tr>
              ) : (
                users.map((user) => (
                  <tr key={user.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-6 py-4 text-sm text-gray-800">{user.userCode}</td>
                    <td className="px-6 py-4 text-sm text-gray-800">{user.fullName}</td>
                    <td className="px-6 py-4 text-sm text-gray-600">{user.email}</td>
                    <td className="px-6 py-4 text-sm text-gray-600">{user.phone || "-"}</td>
                    <td className="px-6 py-4 text-sm text-gray-600">
                      {user.joinDate ? new Date(user.joinDate).toLocaleDateString() : "-"}
                    </td>
                    <td className="px-6 py-4">{getStatusBadge(user.status)}</td>
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleViewProfile(user)}
                          disabled={detailsLoading}
                          className="p-1 text-[#1F7A8C] hover:bg-[#1F7A8C]/10 rounded transition-colors disabled:opacity-70"
                          title="View Profile"
                        >
                          <Eye className="w-4 h-4" />
                        </button>

                        {user.status === "Active" ? (
                          <button
                            onClick={() => handleToggleStatus(user)}
                            disabled={actionLoading}
                            className="p-1 text-red-600 hover:bg-red-50 rounded transition-colors disabled:opacity-70"
                            title="Suspend"
                          >
                            <Ban className="w-4 h-4" />
                          </button>
                        ) : (
                          <button
                            onClick={() => handleToggleStatus(user)}
                            disabled={actionLoading}
                            className="p-1 text-green-600 hover:bg-green-50 rounded transition-colors disabled:opacity-70"
                            title="Activate"
                          >
                            <CheckCircle className="w-4 h-4" />
                          </button>
                        )}

                        <button
                          onClick={() => handleOpenResetPassword(user)}
                          className="p-1 text-gray-600 hover:bg-gray-100 rounded transition-colors"
                          title="Reset Password"
                        >
                          <RotateCcw className="w-4 h-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* View Details Modal */}
      {selectedUser && !showResetPasswordModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-xl w-full">
            <div className="p-6 border-b border-gray-200">
              <h3 className="text-gray-800">User Details</h3>
            </div>

            <div className="p-6 grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-gray-500">Full Name</p>
                <p className="text-sm text-gray-800 mt-1">{selectedUser.fullName}</p>
              </div>

              <div>
                <p className="text-sm text-gray-500">User Code</p>
                <p className="text-sm text-gray-800 mt-1">{selectedUser.userCode}</p>
              </div>

              <div>
                <p className="text-sm text-gray-500">Email</p>
                <p className="text-sm text-gray-800 mt-1">{selectedUser.email}</p>
              </div>

              <div>
                <p className="text-sm text-gray-500">Phone</p>
                <p className="text-sm text-gray-800 mt-1">{selectedUser.phone || "-"}</p>
              </div>

              <div>
                <p className="text-sm text-gray-500">Join Date</p>
                <p className="text-sm text-gray-800 mt-1">
                  {selectedUser.joinDate
                    ? new Date(selectedUser.joinDate).toLocaleDateString()
                    : "-"}
                </p>
              </div>

              <div>
                <p className="text-sm text-gray-500">Status</p>
                <div className="mt-1">{getStatusBadge(selectedUser.status)}</div>
              </div>

              <div>
                <p className="text-sm text-gray-500">Role</p>
                <p className="text-sm text-gray-800 mt-1">{selectedUser.role}</p>
              </div>

              <div>
                <p className="text-sm text-gray-500">Address</p>
                <p className="text-sm text-gray-800 mt-1">{selectedUser.address || "-"}</p>
              </div>
            </div>

            <div className="p-6 border-t border-gray-200 flex justify-end">
              <button
                onClick={() => setSelectedUser(null)}
                className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Reset Password Modal */}
      {showResetPasswordModal && selectedUser && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-[60] p-4">
          <div className="bg-white rounded-lg max-w-md w-full">
            <div className="p-6 border-b border-gray-200">
              <h3 className="text-gray-800">Reset Password</h3>
            </div>

            <div className="p-6 space-y-4">
              <p className="text-sm text-gray-600">
                Reset password for <span className="font-medium">{selectedUser.fullName}</span>
              </p>

              <div>
                <label className="block text-sm text-gray-700 mb-2">
                  New Password
                </label>
                <input
                  type="password"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  placeholder="Enter new password"
                />
              </div>
            </div>

            <div className="p-6 border-t border-gray-200 flex gap-3 justify-end">
              <button
                onClick={() => {
                  setShowResetPasswordModal(false);
                  setNewPassword("");
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
              >
                Cancel
              </button>

              <button
                onClick={handleResetPassword}
                disabled={actionLoading}
                className="px-4 py-2 bg-[#1F7A8C] text-white rounded-lg hover:bg-[#18626F] transition-colors disabled:opacity-70"
              >
                {actionLoading ? "Processing..." : "Reset Password"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}