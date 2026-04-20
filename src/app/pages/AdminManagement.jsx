import { useEffect, useState } from "react";
import { Plus, Edit, Trash2, Eye } from "lucide-react";
import {
  fetchAdmins,
  fetchAdminDetails,
  createAdmin,
  updateAdmin,
  deleteAdmin,
  fetchRecentAdminActions,
} from "../../services/adminManagementService";

const initialFormState = {
  fullName: "",
  email: "",
  password: "",
  location: "",
  phoneNumber: "",
};

export default function AdminManagement() {
  const [admins, setAdmins] = useState([]);
  const [recentActions, setRecentActions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState("");

  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showViewModal, setShowViewModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);

  const [selectedAdmin, setSelectedAdmin] = useState(null);
  const [formData, setFormData] = useState(initialFormState);

  const loadPageData = async () => {
    try {
      setLoading(true);
      setError("");

      const [adminsData, actionsData] = await Promise.all([
        fetchAdmins(),
        fetchRecentAdminActions(),
      ]);

      setAdmins(Array.isArray(adminsData) ? adminsData : []);
      setRecentActions(Array.isArray(actionsData) ? actionsData : []);
    } catch (err) {
      setError(err.message || "Failed to load admin management data");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPageData();
  }, []);

  const formatDate = (value) => {
    if (!value) return "-";

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return value;

    return date.toLocaleDateString();
  };

  const formatDateTime = (value) => {
    if (!value) return "-";

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return value;

    return date.toLocaleString();
  };

  const getRelativeTime = (value) => {
    if (!value) return "-";

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return value;

    const diffMs = Date.now() - date.getTime();
    const diffMinutes = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMinutes < 1) return "Just now";
    if (diffMinutes < 60) return `${diffMinutes} mins ago`;
    if (diffHours < 24) return `${diffHours} hours ago`;
    if (diffDays === 1) return "Yesterday";
    return `${diffDays} days ago`;
  };

  const handleOpenCreate = () => {
    setFormData(initialFormState);
    setShowCreateModal(true);
  };

  const handleOpenView = async (admin) => {
    try {
      setActionLoading(true);
      const details = await fetchAdminDetails(admin.id);
      setSelectedAdmin(details);
      setShowViewModal(true);
    } catch (err) {
      alert(err.message || "Failed to load admin details");
    } finally {
      setActionLoading(false);
    }
  };

  const handleOpenEdit = async (admin) => {
    try {
      setActionLoading(true);
      const details = await fetchAdminDetails(admin.id);
      setSelectedAdmin(details);
      setFormData({
        fullName: details.fullName || "",
        email: details.email || "",
        password: "",
        location: details.location || "",
        phoneNumber: details.phoneNumber || "",
      });
      setShowEditModal(true);
    } catch (err) {
      alert(err.message || "Failed to load admin details");
    } finally {
      setActionLoading(false);
    }
  };

  const handleCreateAdmin = async () => {
    if (
      !formData.fullName.trim() ||
      !formData.email.trim() ||
      !formData.password.trim()
    ) {
      return;
    }

    try {
      setActionLoading(true);

      await createAdmin({
        fullName: formData.fullName,
        email: formData.email,
        password: formData.password,
        location: formData.location,
        phoneNumber: formData.phoneNumber,
      });

      setShowCreateModal(false);
      setFormData(initialFormState);
      await loadPageData();
    } catch (err) {
    } finally {
      setActionLoading(false);
    }
  };

  const handleUpdateAdmin = async () => {
    if (!selectedAdmin) return;

    if (!formData.fullName.trim()) {
      alert("Full Name is required.");
      return;
    }

    try {
      setActionLoading(true);

      await updateAdmin(selectedAdmin.id, {
        fullName: formData.fullName,
        location: formData.location,
        phoneNumber: formData.phoneNumber,
      });

      setShowEditModal(false);
      setFormData(initialFormState);
      setSelectedAdmin(null);
      await loadPageData();
      alert("Admin updated successfully.");
    } catch (err) {
      alert(err.message || "Failed to update admin");
    } finally {
      setActionLoading(false);
    }
  };

  const handleDeleteAdmin = async (admin) => {
    const confirmed = window.confirm(
      `Are you sure you want to delete ${admin.fullName || admin.name || "this admin"}?`
    );

    if (!confirmed) return;

    try {
      setActionLoading(true);
      await deleteAdmin(admin.id);
      await loadPageData();
      alert("Admin deleted successfully.");
    } catch (err) {
      alert(err.message || "Failed to delete admin");
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
            <h2 className="text-gray-800 mb-1">Admin Management</h2>
            <p className="text-sm text-gray-500">
              Manage administrator accounts
            </p>
          </div>

          <button
            onClick={handleOpenCreate}
            className="px-4 py-2 bg-[#1F7A8C] text-white rounded-lg hover:bg-[#18626F] transition-colors flex items-center gap-2"
          >
            <Plus className="w-4 h-4" />
            Add New Admin
          </button>
        </div>
      </div>

      {/* Error */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-600 rounded-lg p-4">
          {error}
        </div>
      )}

      {/* Admin List */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Admin ID
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Name
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Email
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Created Date
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Last Active
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Actions
                </th>
              </tr>
            </thead>

            <tbody className="divide-y divide-gray-200">
              {loading ? (
                <tr>
                  <td
                    colSpan="6"
                    className="px-6 py-6 text-sm text-gray-500 text-center"
                  >
                    Loading...
                  </td>
                </tr>
              ) : admins.length === 0 ? (
                <tr>
                  <td
                    colSpan="6"
                    className="px-6 py-6 text-sm text-gray-500 text-center"
                  >
                    No admins found.
                  </td>
                </tr>
              ) : (
                admins.map((admin, index) => (
                  <tr
                    key={admin.id || index}
                    className="hover:bg-gray-50 transition-colors"
                  >
                    <td className="px-6 py-4 text-sm text-gray-800">
                      {`A${String(index + 1).padStart(3, "0")}`}
                    </td>

                    <td className="px-6 py-4 text-sm text-gray-800">
                      {admin.fullName}
                    </td>

                    <td className="px-6 py-4 text-sm text-gray-600">
                      {admin.email}
                    </td>

                    <td className="px-6 py-4 text-sm text-gray-600">
                      {formatDate(admin.createdAt)}
                    </td>

                    <td className="px-6 py-4 text-sm text-gray-600">
                      {formatDateTime(admin.lastLoginAt)}
                    </td>

                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleOpenView(admin)}
                          disabled={actionLoading}
                          className="p-1 text-[#1F7A8C] hover:bg-[#1F7A8C]/10 rounded transition-colors disabled:opacity-70"
                          title="View Details"
                        >
                          <Eye className="w-4 h-4" />
                        </button>

                        <button
                          onClick={() => handleOpenEdit(admin)}
                          disabled={actionLoading}
                          className="p-1 text-blue-600 hover:bg-blue-50 rounded transition-colors disabled:opacity-70"
                          title="Edit"
                        >
                          <Edit className="w-4 h-4" />
                        </button>

                        <button
                          onClick={() => handleDeleteAdmin(admin)}
                          disabled={actionLoading}
                          className="p-1 text-red-600 hover:bg-red-50 rounded transition-colors disabled:opacity-70"
                          title="Delete"
                        >
                          <Trash2 className="w-4 h-4" />
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

      {/* Admin Action Log */}
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-gray-800 mb-4">Recent Admin Actions</h3>

        {loading ? (
          <p className="text-sm text-gray-500">Loading...</p>
        ) : recentActions.length === 0 ? (
          <p className="text-sm text-gray-500">No recent actions found.</p>
        ) : (
          <div className="space-y-3">
            {recentActions.map((action, index) => (
              <div
                key={index}
                className="flex items-start gap-4 p-3 hover:bg-gray-50 rounded-lg transition-colors"
              >
                <div className="w-2 h-2 bg-[#1F7A8C] rounded-full mt-2"></div>

                <div className="flex-1">
                  <p className="text-sm text-gray-800">
                    <span className="text-[#1F7A8C]">
                      {action.adminName || "Admin"}
                    </span>{" "}
                    {action.description ||
                      action.actionDescription ||
                      action.title ||
                      "performed an action"}
                  </p>
                  <p className="text-xs text-gray-500 mt-1">
                    {getRelativeTime(action.createdAt)}
                  </p>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* View Admin Modal */}
      {showViewModal && selectedAdmin && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full">
            <div className="p-6 border-b border-gray-200">
              <h3 className="text-gray-800">Admin Details</h3>
            </div>

            <div className="p-6 space-y-4">
              <div>
                <p className="text-sm text-gray-500">Full Name</p>
                <p className="text-sm text-gray-800 mt-1">
                  {selectedAdmin.fullName || "-"}
                </p>
              </div>

              <div>
                <p className="text-sm text-gray-500">Email</p>
                <p className="text-sm text-gray-800 mt-1">
                  {selectedAdmin.email || "-"}
                </p>
              </div>

              <div>
                <p className="text-sm text-gray-500">Location</p>
                <p className="text-sm text-gray-800 mt-1">
                  {selectedAdmin.location || "-"}
                </p>
              </div>

              <div>
                <p className="text-sm text-gray-500">Phone Number</p>
                <p className="text-sm text-gray-800 mt-1">
                  {selectedAdmin.phoneNumber || "-"}
                </p>
              </div>

              <div>
                <p className="text-sm text-gray-500">Created Date</p>
                <p className="text-sm text-gray-800 mt-1">
                  {formatDate(selectedAdmin.createdAt)}
                </p>
              </div>

              <div>
                <p className="text-sm text-gray-500">Last Active</p>
                <p className="text-sm text-gray-800 mt-1">
                  {formatDateTime(selectedAdmin.lastLoginAt)}
                </p>
              </div>
            </div>

            <div className="p-6 border-t border-gray-200 flex justify-end">
              <button
                onClick={() => {
                  setShowViewModal(false);
                  setSelectedAdmin(null);
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Add Admin Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full">
            <div className="p-6 border-b border-gray-200">
              <h3 className="text-gray-800">Add New Administrator</h3>
            </div>

            <div className="p-6 space-y-4">
              <div>
                <label className="block text-sm text-gray-700 mb-2">
                  Full Name
                </label>
                <input
                  type="text"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white"
                  placeholder="Enter full name..."
                  value={formData.fullName}
                  onChange={(e) =>
                    setFormData((prev) => ({
                      ...prev,
                      fullName: e.target.value,
                    }))
                  }
                />
              </div>

              <div>
                <label className="block text-sm text-gray-700 mb-2">
                  Email
                </label>
                <input
                  type="email"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white"
                  placeholder="admin@nursehome.com"
                  value={formData.email}
                  onChange={(e) =>
                    setFormData((prev) => ({
                      ...prev,
                      email: e.target.value,
                    }))
                  }
                />
              </div>

              <div>
                <label className="block text-sm text-gray-700 mb-2">
                  Password
                </label>
                <input
                  type="password"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white"
                  placeholder="Enter password..."
                  value={formData.password}
                  onChange={(e) =>
                    setFormData((prev) => ({
                      ...prev,
                      password: e.target.value,
                    }))
                  }
                />
              </div>

              <div>
                <label className="block text-sm text-gray-700 mb-2">
                  Location
                </label>
                <input
                  type="text"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white"
                  placeholder="Enter location..."
                  value={formData.location}
                  onChange={(e) =>
                    setFormData((prev) => ({
                      ...prev,
                      location: e.target.value,
                    }))
                  }
                />
              </div>

              <div>
                <label className="block text-sm text-gray-700 mb-2">
                  Phone Number
                </label>
                <input
                  type="text"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white"
                  placeholder="Enter phone number..."
                  value={formData.phoneNumber}
                  onChange={(e) =>
                    setFormData((prev) => ({
                      ...prev,
                      phoneNumber: e.target.value,
                    }))
                  }
                />
              </div>
            </div>

            <div className="p-6 border-t border-gray-200 flex gap-3 justify-end">
              <button
                onClick={() => {
                  setShowCreateModal(false);
                  setFormData(initialFormState);
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
              >
                Cancel
              </button>

              <button
                onClick={handleCreateAdmin}
                disabled={actionLoading}
                className="px-4 py-2 bg-[#1F7A8C] text-white rounded-lg hover:bg-[#18626F] transition-colors disabled:opacity-70"
              >
                {actionLoading ? "Creating..." : "Create Admin"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Edit Admin Modal */}
      {showEditModal && selectedAdmin && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full">
            <div className="p-6 border-b border-gray-200">
              <h3 className="text-gray-800">Edit Administrator</h3>
            </div>

            <div className="p-6 space-y-4">
              <div>
                <label className="block text-sm text-gray-700 mb-2">
                  Full Name
                </label>
                <input
                  type="text"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white"
                  placeholder="Enter full name..."
                  value={formData.fullName}
                  onChange={(e) =>
                    setFormData((prev) => ({
                      ...prev,
                      fullName: e.target.value,
                    }))
                  }
                />
              </div>

              <div>
                <label className="block text-sm text-gray-700 mb-2">
                  Email
                </label>
                <input
                  type="email"
                  disabled
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-gray-50 text-gray-500"
                  value={formData.email}
                />
              </div>

              <div>
                <label className="block text-sm text-gray-700 mb-2">
                  Location
                </label>
                <input
                  type="text"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white"
                  placeholder="Enter location..."
                  value={formData.location}
                  onChange={(e) =>
                    setFormData((prev) => ({
                      ...prev,
                      location: e.target.value,
                    }))
                  }
                />
              </div>

              <div>
                <label className="block text-sm text-gray-700 mb-2">
                  Phone Number
                </label>
                <input
                  type="text"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white"
                  placeholder="Enter phone number..."
                  value={formData.phoneNumber}
                  onChange={(e) =>
                    setFormData((prev) => ({
                      ...prev,
                      phoneNumber: e.target.value,
                    }))
                  }
                />
              </div>
            </div>

            <div className="p-6 border-t border-gray-200 flex gap-3 justify-end">
              <button
                onClick={() => {
                  setShowEditModal(false);
                  setSelectedAdmin(null);
                  setFormData(initialFormState);
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
              >
                Cancel
              </button>

              <button
                onClick={handleUpdateAdmin}
                disabled={actionLoading}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-70"
              >
                {actionLoading ? "Saving..." : "Save Changes"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}