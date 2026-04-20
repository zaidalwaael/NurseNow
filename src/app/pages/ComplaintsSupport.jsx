import { useEffect, useState } from "react";
import { Search, Eye, MessageSquare, CheckCircle } from "lucide-react";
import {
  fetchComplaints,
  fetchComplaintDetails,
  respondToComplaint,
  resolveComplaint,
} from "../../services/complaintsSupportService";

export default function ComplaintsSupport() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("All");
  const [complaints, setComplaints] = useState([]);
  const [selectedComplaint, setSelectedComplaint] = useState(null);
  const [adminResponse, setAdminResponse] = useState("");
  const [loading, setLoading] = useState(true);
  const [detailsLoading, setDetailsLoading] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState("");

  const loadComplaints = async () => {
    try {
      setLoading(true);
      setError("");

      const data = await fetchComplaints(searchTerm, statusFilter);
      setComplaints(Array.isArray(data) ? data : []);
    } catch (err) {
      setError(err.message || "Failed to load complaints");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const delay = setTimeout(() => {
      loadComplaints();
    }, 300);

    return () => clearTimeout(delay);
  }, [searchTerm, statusFilter]);

  const getStatusBadge = (status) => {
    const styles = {
      Open: "bg-yellow-100 text-yellow-800",
      "In Progress": "bg-blue-100 text-blue-800",
      Resolved: "bg-green-100 text-green-800",
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

  const getCategoryColor = (category) => {
    const colors = {
      "Service Quality": "text-red-600",
      Billing: "text-orange-600",
      Technical: "text-blue-600",
      Other: "text-gray-600",
    };

    return colors[category] || "text-gray-600";
  };

  const handleViewComplaint = async (complaint) => {
    try {
      setDetailsLoading(true);
      const details = await fetchComplaintDetails(complaint.complaintId);
      setSelectedComplaint(details);
      setAdminResponse(details.adminResponse || "");
    } catch (err) {
      alert(err.message || "Failed to load complaint details");
    } finally {
      setDetailsLoading(false);
    }
  };

  const handleSendResponse = async () => {
    if (!selectedComplaint) return;

    if (!adminResponse.trim()) {
      alert("Please enter a response.");
      return;
    }

    try {
      setActionLoading(true);
      await respondToComplaint(selectedComplaint.complaintId, adminResponse);

      const updated = await fetchComplaintDetails(selectedComplaint.complaintId);
      setSelectedComplaint(updated);
      setAdminResponse(updated.adminResponse || "");

      await loadComplaints();
      alert("Response sent successfully.");
    } catch (err) {
      alert(err.message || "Failed to send response");
    } finally {
      setActionLoading(false);
    }
  };

  const handleResolveComplaint = async () => {
    if (!selectedComplaint) return;

    try {
      setActionLoading(true);
      await resolveComplaint(selectedComplaint.complaintId);

      await loadComplaints();
      const updated = await fetchComplaintDetails(selectedComplaint.complaintId);
      setSelectedComplaint(updated);

      alert("Complaint marked as resolved.");
    } catch (err) {
      alert(err.message || "Failed to resolve complaint");
    } finally {
      setActionLoading(false);
    }
  };

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

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex flex-col md:flex-row gap-4 items-start md:items-center justify-between">
          <div>
            <h2 className="text-gray-800 mb-1">Complaints & Support</h2>
            <p className="text-sm text-gray-500">
              Manage user complaints and support tickets
            </p>
          </div>

          <div className="flex gap-3 w-full md:w-auto">
            <div className="relative flex-1 md:flex-initial">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input
                type="text"
                placeholder="Search complaints..."
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
              <option value="Open">Open</option>
              <option value="In Progress">In Progress</option>
              <option value="Resolved">Resolved</option>
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

      {/* Complaints Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Complaint ID
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Submitted By
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Category
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Subject
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Date
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Status
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
                    colSpan="7"
                    className="px-6 py-6 text-sm text-gray-500 text-center"
                  >
                    Loading...
                  </td>
                </tr>
              ) : complaints.length === 0 ? (
                <tr>
                  <td
                    colSpan="7"
                    className="px-6 py-6 text-sm text-gray-500 text-center"
                  >
                    No complaints found.
                  </td>
                </tr>
              ) : (
                complaints.map((complaint) => (
                  <tr
                    key={complaint.complaintId}
                    className="hover:bg-gray-50 transition-colors"
                  >
                    <td className="px-6 py-4 text-sm text-gray-800">
                      {complaint.complaintCode}
                    </td>

                    <td className="px-6 py-4 text-sm text-gray-800">
                      {complaint.submittedBy}
                    </td>

                    <td className="px-6 py-4">
                      <span
                        className={`text-sm ${getCategoryColor(complaint.category)}`}
                      >
                        {complaint.category}
                      </span>
                    </td>

                    <td className="px-6 py-4 text-sm text-gray-800">
                      {complaint.subject}
                    </td>

                    <td className="px-6 py-4 text-sm text-gray-600">
                      {formatDate(complaint.createdAt)}
                    </td>

                    <td className="px-6 py-4">
                      {getStatusBadge(complaint.status)}
                    </td>

                    <td className="px-6 py-4">
                      <button
                        onClick={() => handleViewComplaint(complaint)}
                        disabled={detailsLoading}
                        className="inline-flex items-center gap-2 px-3 py-1 text-sm text-[#1F7A8C] hover:bg-[#1F7A8C]/10 rounded-lg transition-colors disabled:opacity-70"
                      >
                        <Eye className="w-4 h-4" />
                        View
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Complaint Detail Modal */}
      {selectedComplaint && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            <div className="p-6 border-b border-gray-200">
              <h3 className="text-gray-800">Complaint Details</h3>
            </div>

            <div className="p-6 space-y-6">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm text-gray-500">Complaint ID</p>
                  <p className="text-sm text-gray-800 mt-1">
                    {selectedComplaint.complaintCode}
                  </p>
                </div>

                <div>
                  <p className="text-sm text-gray-500">Status</p>
                  <div className="mt-1">
                    {getStatusBadge(selectedComplaint.status)}
                  </div>
                </div>

                <div>
                  <p className="text-sm text-gray-500">Submitted By</p>
                  <p className="text-sm text-gray-800 mt-1">
                    {selectedComplaint.submittedBy}
                  </p>
                </div>

                <div>
                  <p className="text-sm text-gray-500">User Email</p>
                  <p className="text-sm text-gray-800 mt-1">
                    {selectedComplaint.userEmail || "-"}
                  </p>
                </div>

                <div>
                  <p className="text-sm text-gray-500">Category</p>
                  <p
                    className={`text-sm mt-1 ${getCategoryColor(selectedComplaint.category)}`}
                  >
                    {selectedComplaint.category}
                  </p>
                </div>

                <div>
                  <p className="text-sm text-gray-500">Date Submitted</p>
                  <p className="text-sm text-gray-800 mt-1">
                    {formatDate(selectedComplaint.createdAt)}
                  </p>
                </div>

                {selectedComplaint.respondedAt && (
                  <div className="col-span-2">
                    <p className="text-sm text-gray-500">Last Response Time</p>
                    <p className="text-sm text-gray-800 mt-1">
                      {formatDateTime(selectedComplaint.respondedAt)}
                    </p>
                  </div>
                )}
              </div>

              <div>
                <p className="text-sm text-gray-500 mb-2">Subject</p>
                <p className="text-sm text-gray-800">
                  {selectedComplaint.subject}
                </p>
              </div>

              <div>
                <p className="text-sm text-gray-500 mb-2">Description</p>
                <p className="text-sm text-gray-700 leading-relaxed">
                  {selectedComplaint.description}
                </p>
              </div>

              <div>
                <label className="block text-sm text-gray-700 mb-2">
                  Admin Response
                </label>
                <textarea
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white"
                  rows={4}
                  placeholder="Type your response to the user..."
                  value={adminResponse}
                  onChange={(e) => setAdminResponse(e.target.value)}
                  disabled={selectedComplaint.status === "Resolved"}
                />
              </div>
            </div>

            <div className="p-6 border-t border-gray-200 flex flex-col sm:flex-row gap-3 justify-end">
              <button
                onClick={() => {
                  setSelectedComplaint(null);
                  setAdminResponse("");
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
              >
                Close
              </button>

              {selectedComplaint.status !== "Resolved" && (
                <>
                  <button
                    onClick={handleSendResponse}
                    disabled={actionLoading}
                    className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2 disabled:opacity-70"
                  >
                    <MessageSquare className="w-4 h-4" />
                    {actionLoading ? "Sending..." : "Send Response"}
                  </button>

                  <button
                    onClick={handleResolveComplaint}
                    disabled={actionLoading}
                    className="px-4 py-2 bg-[#1F7A8C] text-white rounded-lg hover:bg-[#18626F] transition-colors flex items-center gap-2 disabled:opacity-70"
                  >
                    <CheckCircle className="w-4 h-4" />
                    {actionLoading ? "Processing..." : "Mark as Resolved"}
                  </button>
                </>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}