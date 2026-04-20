import { useEffect, useState } from "react";
import { Search, Eye, CheckCircle, XCircle, FileText } from "lucide-react";
import {
  fetchNurseVerifications,
  fetchNurseVerificationDetails,
  approveNurseVerification,
  rejectNurseVerification,
} from "../../services/nurseVerificationService";

export default function NurseVerification() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("All");
  const [nurses, setNurses] = useState([]);
  const [selectedNurse, setSelectedNurse] = useState(null);
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [rejectReason, setRejectReason] = useState("");
  const [loading, setLoading] = useState(true);
  const [detailsLoading, setDetailsLoading] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState("");

  const loadNurses = async () => {
    try {
      setLoading(true);
      setError("");

      const data = await fetchNurseVerifications(searchTerm, statusFilter);
      setNurses(Array.isArray(data) ? data : []);
    } catch (err) {
      setError(err.message || "Failed to load nurse verifications");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const delay = setTimeout(() => {
      loadNurses();
    }, 300);

    return () => clearTimeout(delay);
  }, [searchTerm, statusFilter]);

  const getStatusBadge = (status) => {
    const styles = {
      Pending: "bg-yellow-100 text-yellow-800",
      Approved: "bg-green-100 text-green-800",
      Rejected: "bg-red-100 text-red-800",
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

  const handleReview = async (nurse) => {
    try {
      setDetailsLoading(true);
      const details = await fetchNurseVerificationDetails(nurse.id);
      setSelectedNurse(details);
    } catch (err) {
      alert(err.message || "Failed to load nurse details");
    } finally {
      setDetailsLoading(false);
    }
  };

  const handleApprove = async () => {
    if (!selectedNurse) return;

    try {
      setActionLoading(true);
      await approveNurseVerification(selectedNurse.id);
      await loadNurses();
      setSelectedNurse(null);
    } catch (err) {
      alert(err.message || "Failed to approve nurse");
    } finally {
      setActionLoading(false);
    }
  };

  const handleReject = async () => {
    if (!selectedNurse) return;

    if (!rejectReason.trim()) {
      alert("Please provide a rejection reason.");
      return;
    }

    try {
      setActionLoading(true);
      await rejectNurseVerification(selectedNurse.id, rejectReason);
      await loadNurses();
      setShowRejectModal(false);
      setRejectReason("");
      setSelectedNurse(null);
    } catch (err) {
      alert(err.message || "Failed to reject nurse");
    } finally {
      setActionLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex flex-col md:flex-row gap-4 items-start md:items-center justify-between">
          <div>
            <h2 className="text-gray-800 mb-1">Nurse Verification</h2>
            <p className="text-sm text-gray-500">
              Review and verify nurse registration applications
            </p>
          </div>

          <div className="flex gap-3 w-full md:w-auto">
            <div className="relative flex-1 md:flex-initial">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input
                type="text"
                placeholder="Search by name or email..."
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
              <option value="Approved">Approved</option>
              <option value="Rejected">Rejected</option>
            </select>
          </div>
        </div>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-600 rounded-lg p-4">
          {error}
        </div>
      )}

      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Nurse ID
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Name
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Email
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Phone
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Registration Date
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
              ) : nurses.length === 0 ? (
                <tr>
                  <td
                    colSpan="7"
                    className="px-6 py-6 text-sm text-gray-500 text-center"
                  >
                    No nurse verifications found.
                  </td>
                </tr>
              ) : (
                nurses.map((nurse) => (
                  <tr
                    key={nurse.id}
                    className="hover:bg-gray-50 transition-colors"
                  >
                    <td className="px-6 py-4 text-sm text-gray-800">
                      {nurse.nurseId}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-800">
                      {nurse.fullName}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600">
                      {nurse.email}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600">
                      {nurse.phone}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600">
                      {nurse.registrationDate
                        ? new Date(nurse.registrationDate).toLocaleDateString()
                        : "-"}
                    </td>
                    <td className="px-6 py-4">
                      {getStatusBadge(nurse.status)}
                    </td>
                    <td className="px-6 py-4">
                      <button
                        onClick={() => handleReview(nurse)}
                        disabled={detailsLoading}
                        className="inline-flex items-center gap-2 px-3 py-1 text-sm text-[#1F7A8C] hover:bg-[#1F7A8C]/10 rounded-lg transition-colors disabled:opacity-70"
                      >
                        <Eye className="w-4 h-4" />
                        Review
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {selectedNurse && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            <div className="p-6 border-b border-gray-200">
              <h3 className="text-gray-800">Nurse Profile Review</h3>
            </div>

            <div className="p-6 space-y-6">
              <div>
                <h4 className="text-gray-700 mb-3">Basic Information</h4>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm text-gray-500">Full Name</p>
                    <p className="text-sm text-gray-800 mt-1">
                      {selectedNurse.fullName}
                    </p>
                  </div>

                  <div>
                    <p className="text-sm text-gray-500">Nurse ID</p>
                    <p className="text-sm text-gray-800 mt-1">
                      {selectedNurse.nurseId}
                    </p>
                  </div>

                  <div>
                    <p className="text-sm text-gray-500">Email</p>
                    <p className="text-sm text-gray-800 mt-1">
                      {selectedNurse.email}
                    </p>
                  </div>

                  <div>
                    <p className="text-sm text-gray-500">Phone</p>
                    <p className="text-sm text-gray-800 mt-1">
                      {selectedNurse.phone}
                    </p>
                  </div>

                  <div>
                    <p className="text-sm text-gray-500">Registration Date</p>
                    <p className="text-sm text-gray-800 mt-1">
                      {selectedNurse.registrationDate
                        ? new Date(
                            selectedNurse.registrationDate
                          ).toLocaleDateString()
                        : "-"}
                    </p>
                  </div>

                  <div>
                    <p className="text-sm text-gray-500">Status</p>
                    <div className="mt-1">
                      {getStatusBadge(selectedNurse.status)}
                    </div>
                  </div>

                  {selectedNurse.rejectionReason && (
                    <div className="col-span-2">
                      <p className="text-sm text-gray-500">Rejection Reason</p>
                      <p className="text-sm text-red-600 mt-1">
                        {selectedNurse.rejectionReason}
                      </p>
                    </div>
                  )}
                </div>
              </div>

              <div>
                <h4 className="text-gray-700 mb-3">Uploaded Documents</h4>
                <div className="space-y-2">
                  {!selectedNurse.documents ||
                  selectedNurse.documents.length === 0 ? (
                    <p className="text-sm text-gray-500">
                      No documents available.
                    </p>
                  ) : (
                    selectedNurse.documents.map((doc) => (
                      <div
                        key={doc.id}
                        className="flex items-center justify-between p-3 border border-gray-200 rounded-lg hover:bg-gray-50"
                      >
                        <div className="flex items-center gap-3">
                          <FileText className="w-5 h-5 text-[#1F7A8C]" />
                          <span className="text-sm text-gray-800">
                            {doc.name}
                          </span>
                        </div>

                        <a
                          href={doc.fileUrl}
                          target="_blank"
                          rel="noreferrer"
                          className="text-sm text-[#1F7A8C] hover:underline"
                        >
                          View
                        </a>
                      </div>
                    ))
                  )}
                </div>
              </div>
            </div>

            <div className="p-6 border-t border-gray-200 flex flex-col sm:flex-row gap-3 justify-end">
              <button
                onClick={() => {
                  setSelectedNurse(null);
                  setRejectReason("");
                  setShowRejectModal(false);
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
              >
                Close
              </button>

              {selectedNurse.status === "Pending" && (
                <>
                  <button
                    onClick={() => setShowRejectModal(true)}
                    disabled={actionLoading}
                    className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors flex items-center gap-2 disabled:opacity-70"
                  >
                    <XCircle className="w-4 h-4" />
                    Reject
                  </button>

                  <button
                    onClick={handleApprove}
                    disabled={actionLoading}
                    className="px-4 py-2 bg-[#1F7A8C] text-white rounded-lg hover:bg-[#18626F] transition-colors flex items-center gap-2 disabled:opacity-70"
                  >
                    <CheckCircle className="w-4 h-4" />
                    {actionLoading ? "Processing..." : "Approve"}
                  </button>
                </>
              )}
            </div>
          </div>
        </div>
      )}

      {showRejectModal && selectedNurse && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-[60] p-4">
          <div className="bg-white rounded-lg max-w-md w-full">
            <div className="p-6 border-b border-gray-200">
              <h3 className="text-gray-800">Reject Nurse Application</h3>
            </div>

            <div className="p-6">
              <label className="block text-sm text-gray-700 mb-2">
                Rejection Reason
              </label>

              <textarea
                className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white"
                rows={4}
                placeholder="Provide a reason for rejection..."
                value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
              />
            </div>

            <div className="p-6 border-t border-gray-200 flex gap-3 justify-end">
              <button
                onClick={() => {
                  setShowRejectModal(false);
                  setRejectReason("");
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
              >
                Cancel
              </button>

              <button
                onClick={handleReject}
                disabled={actionLoading}
                className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors disabled:opacity-70"
              >
                {actionLoading ? "Processing..." : "Confirm Rejection"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}