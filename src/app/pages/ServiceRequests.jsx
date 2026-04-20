import { useEffect, useState } from "react";
import { Search, Eye, X, UserX } from "lucide-react";
import {
  fetchServiceRequests,
  fetchServiceRequestDetails,
  reassignServiceRequest,
  cancelServiceRequest,
} from "../../services/serviceRequestsService";

const getStatusBadge = (status) => {
  const styles = {
    Pending: "bg-yellow-100 text-yellow-800",
    Assigned: "bg-blue-100 text-blue-800",
    "In Progress": "bg-purple-100 text-purple-800",
    Completed: "bg-green-100 text-green-800",
    Cancelled: "bg-red-100 text-red-800",
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

export default function ServiceRequests() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("All");
  const [requests, setRequests] = useState([]);
  const [selectedRequest, setSelectedRequest] = useState(null);
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [showReassignModal, setShowReassignModal] = useState(false);
  const [cancelReason, setCancelReason] = useState("");
  const [newNurseId, setNewNurseId] = useState("");
  const [loading, setLoading] = useState(true);
  const [detailsLoading, setDetailsLoading] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState("");

  const loadRequests = async () => {
    try {
      setLoading(true);
      setError("");

      const data = await fetchServiceRequests(searchTerm, statusFilter);
      setRequests(Array.isArray(data) ? data : []);
    } catch (err) {
      setError(err.message || "Failed to load service requests");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const delay = setTimeout(() => {
      loadRequests();
    }, 300);

    return () => clearTimeout(delay);
  }, [searchTerm, statusFilter]);

  const handleViewDetails = async (request) => {
    try {
      setDetailsLoading(true);
      const details = await fetchServiceRequestDetails(request.requestId);
      setSelectedRequest(details);
    } catch (err) {
      alert(err.message || "Failed to load request details");
    } finally {
      setDetailsLoading(false);
    }
  };

  const handleCancelRequest = async () => {
    if (!selectedRequest) return;

    try {
      setActionLoading(true);
      await cancelServiceRequest(selectedRequest.requestId, cancelReason);
      await loadRequests();
      setShowCancelModal(false);
      setCancelReason("");
      setSelectedRequest(null);
    } catch (err) {
      alert(err.message || "Failed to cancel request");
    } finally {
      setActionLoading(false);
    }
  };

  const handleReassignRequest = async () => {
    if (!selectedRequest) return;

    if (!newNurseId.trim()) {
      alert("Please enter nurse ID.");
      return;
    }

    try {
      setActionLoading(true);
      await reassignServiceRequest(selectedRequest.requestId, newNurseId);
      await loadRequests();

      const updatedDetails = await fetchServiceRequestDetails(selectedRequest.requestId);
      setSelectedRequest(updatedDetails);

      setShowReassignModal(false);
      setNewNurseId("");
    } catch (err) {
      alert(err.message || "Failed to reassign request");
    } finally {
      setActionLoading(false);
    }
  };

  const canManageRequest = (status) =>
    status !== "Cancelled" && status !== "Completed";

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex flex-col md:flex-row gap-4 items-start md:items-center justify-between">
          <div>
            <h2 className="text-gray-800 mb-1">Service Requests Management</h2>
            <p className="text-sm text-gray-500">
              Monitor and manage all service requests
            </p>
          </div>

          <div className="flex gap-3 w-full md:w-auto">
            <div className="relative flex-1 md:flex-initial">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input
                type="text"
                placeholder="Search requests..."
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
              <option value="Pending">Pending</option>
              <option value="Assigned">Assigned</option>
              <option value="In Progress">In Progress</option>
              <option value="Completed">Completed</option>
              <option value="Cancelled">Cancelled</option>
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

      {/* Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Request ID
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Patient Name
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Assigned Nurse
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Service Type
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Date &amp; Time
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
                  <td colSpan="7" className="px-6 py-6 text-sm text-gray-500 text-center">
                    Loading...
                  </td>
                </tr>
              ) : requests.length === 0 ? (
                <tr>
                  <td colSpan="7" className="px-6 py-6 text-sm text-gray-500 text-center">
                    No requests found.
                  </td>
                </tr>
              ) : (
                requests.map((request) => (
                  <tr
                    key={request.requestId}
                    className="hover:bg-gray-50 transition-colors"
                  >
                    <td className="px-6 py-4 text-sm text-gray-800">
                      {request.requestId}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-800">
                      {request.patientName}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600">
                      {!request.assignedNurse || request.assignedNurse === "Unassigned" ? (
                        <span className="text-yellow-600 italic">Unassigned</span>
                      ) : (
                        request.assignedNurse
                      )}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600">
                      {request.serviceType}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600">
                      {request.bookingDate
                        ? new Date(request.bookingDate).toLocaleDateString()
                        : "-"}{" "}
                      at {request.startTime}
                    </td>
                    <td className="px-6 py-4">
                      {getStatusBadge(request.status)}
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2">
                        <button
                          onClick={() => handleViewDetails(request)}
                          disabled={detailsLoading}
                          className="p-1 text-[#1F7A8C] hover:bg-[#1F7A8C]/10 rounded transition-colors disabled:opacity-70"
                          title="View Details"
                        >
                          <Eye className="w-4 h-4" />
                        </button>

                        {canManageRequest(request.status) && (
                          <>
                            <button
                              onClick={() => {
                                setSelectedRequest(request);
                                setShowReassignModal(true);
                              }}
                              className="p-1 text-blue-600 hover:bg-blue-50 rounded transition-colors"
                              title="Reassign Nurse"
                            >
                              <UserX className="w-4 h-4" />
                            </button>

                            <button
                              onClick={() => {
                                setSelectedRequest(request);
                                setShowCancelModal(true);
                              }}
                              className="p-1 text-red-600 hover:bg-red-50 rounded transition-colors"
                              title="Cancel Request"
                            >
                              <X className="w-4 h-4" />
                            </button>
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Request Detail Modal */}
      {selectedRequest && !showCancelModal && !showReassignModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-2xl w-full">
            <div className="p-6 border-b border-gray-200">
              <h3 className="text-gray-800">Service Request Details</h3>
            </div>

            <div className="p-6 space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm text-gray-500">Request ID</p>
                  <p className="text-sm text-gray-800 mt-1">
                    {selectedRequest.requestId}
                  </p>
                </div>

                <div>
                  <p className="text-sm text-gray-500">Status</p>
                  <div className="mt-1">
                    {getStatusBadge(selectedRequest.status)}
                  </div>
                </div>

                <div>
                  <p className="text-sm text-gray-500">Patient Name</p>
                  <p className="text-sm text-gray-800 mt-1">
                    {selectedRequest.patientName}
                  </p>
                </div>

                <div>
                  <p className="text-sm text-gray-500">Assigned Nurse</p>
                  <p className="text-sm text-gray-800 mt-1">
                    {selectedRequest.assignedNurse || "Unassigned"}
                  </p>
                </div>

                <div>
                  <p className="text-sm text-gray-500">Service Type</p>
                  <p className="text-sm text-gray-800 mt-1">
                    {selectedRequest.serviceType}
                  </p>
                </div>

                <div>
                  <p className="text-sm text-gray-500">Date &amp; Time</p>
                  <p className="text-sm text-gray-800 mt-1">
                    {selectedRequest.bookingDate
                      ? new Date(selectedRequest.bookingDate).toLocaleDateString()
                      : "-"}{" "}
                    at {selectedRequest.startTime}
                  </p>
                </div>

                <div className="col-span-2">
                  <p className="text-sm text-gray-500">Address</p>
                  <p className="text-sm text-gray-800 mt-1">
                    {selectedRequest.serviceAddress || "-"}
                  </p>
                </div>

                <div className="col-span-2">
                  <p className="text-sm text-gray-500">Additional Notes</p>
                  <p className="text-sm text-gray-800 mt-1">
                    {selectedRequest.additionalNotes || "-"}
                  </p>
                </div>
              </div>
            </div>

            <div className="p-6 border-t border-gray-200 flex gap-3 justify-end">
              <button
                onClick={() => setSelectedRequest(null)}
                className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
              >
                Close
              </button>

              {canManageRequest(selectedRequest.status) && (
                <>
                  <button
                    onClick={() => setShowReassignModal(true)}
                    className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                  >
                    Reassign Nurse
                  </button>

                  <button
                    onClick={() => setShowCancelModal(true)}
                    className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
                  >
                    Cancel Request
                  </button>
                </>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Cancel Modal */}
      {showCancelModal && selectedRequest && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-[60] p-4">
          <div className="bg-white rounded-lg max-w-md w-full">
            <div className="p-6 border-b border-gray-200">
              <h3 className="text-gray-800">Cancel Request</h3>
            </div>

            <div className="p-6 space-y-4">
              <p className="text-sm text-gray-600">
                Cancel request <span className="font-medium">#{selectedRequest.requestId}</span>
              </p>

              <div>
                <label className="block text-sm text-gray-700 mb-2">
                  Cancellation Reason
                </label>
                <textarea
                  rows={4}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white"
                  placeholder="Optional reason..."
                  value={cancelReason}
                  onChange={(e) => setCancelReason(e.target.value)}
                />
              </div>
            </div>

            <div className="p-6 border-t border-gray-200 flex gap-3 justify-end">
              <button
                onClick={() => {
                  setShowCancelModal(false);
                  setCancelReason("");
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
              >
                Close
              </button>

              <button
                onClick={handleCancelRequest}
                disabled={actionLoading}
                className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors disabled:opacity-70"
              >
                {actionLoading ? "Processing..." : "Confirm Cancel"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Reassign Modal */}
      {showReassignModal && selectedRequest && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-[60] p-4">
          <div className="bg-white rounded-lg max-w-md w-full">
            <div className="p-6 border-b border-gray-200">
              <h3 className="text-gray-800">Reassign Nurse</h3>
            </div>

            <div className="p-6 space-y-4">
              <p className="text-sm text-gray-600">
                Reassign request <span className="font-medium">#{selectedRequest.requestId}</span>
              </p>

              <div>
                <label className="block text-sm text-gray-700 mb-2">
                  New Nurse ID
                </label>
                <input
                  type="text"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white"
                  placeholder="Enter nurse id"
                  value={newNurseId}
                  onChange={(e) => setNewNurseId(e.target.value)}
                />
              </div>
            </div>

            <div className="p-6 border-t border-gray-200 flex gap-3 justify-end">
              <button
                onClick={() => {
                  setShowReassignModal(false);
                  setNewNurseId("");
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
              >
                Close
              </button>

              <button
                onClick={handleReassignRequest}
                disabled={actionLoading}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-70"
              >
                {actionLoading ? "Processing..." : "Confirm Reassign"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}