import { useEffect, useState } from "react";
import {
  DollarSign,
  TrendingUp,
  Users,
  RefreshCcw,
  Clock,
  Search,
  Filter,
  Download,
} from "lucide-react";
import {
  fetchTransactionStats,
  fetchTransactions,
  fetchRecentFinancialActivity,
  exportTransactionsPdf,
} from "../../services/paymentTransactionsService";

export default function PaymentTransactions() {
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("All");
  const [stats, setStats] = useState({
    totalRevenue: 0,
    platformCommission: 0,
    nursePayouts: 0,
    refundedTransactions: 0,
    pendingPayments: 0,
  });
  const [transactions, setTransactions] = useState([]);
  const [recentActivity, setRecentActivity] = useState([]);
  const [loading, setLoading] = useState(true);
  const [exportLoading, setExportLoading] = useState(false);
  const [error, setError] = useState("");

  const getStatusBadge = (status) => {
    const styles = {
      Paid: "bg-green-100 text-green-800",
      Pending: "bg-yellow-100 text-yellow-800",
      Refunded: "bg-blue-100 text-blue-800",
      Failed: "bg-red-100 text-red-800",
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

  const formatMoney = (value) => {
    const amount = Number(value || 0);
    return `$${amount.toFixed(2)}`;
  };

  const formatDateTime = (value) => {
    if (!value) return "-";

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return value;

    return date.toLocaleString();
  };

  const getRelativeTime = (dateValue) => {
    if (!dateValue) return "-";

    const date = new Date(dateValue);
    if (Number.isNaN(date.getTime())) return "-";

    const seconds = Math.floor((Date.now() - date.getTime()) / 1000);

    if (seconds < 60) return "Just now";

    const minutes = Math.floor(seconds / 60);
    if (minutes < 60) return `${minutes} mins ago`;

    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours} hours ago`;

    const days = Math.floor(hours / 24);
    return `${days} days ago`;
  };

  const mapActivityType = (type) => {
    if (type === "Income") return "credit";
    return "debit";
  };

  const loadPageData = async () => {
    try {
      setLoading(true);
      setError("");

      const [statsData, transactionsData, activityData] = await Promise.all([
        fetchTransactionStats(),
        fetchTransactions(searchTerm, statusFilter),
        fetchRecentFinancialActivity(),
      ]);

      setStats({
        totalRevenue: statsData.totalRevenue || 0,
        platformCommission: statsData.platformCommission || 0,
        nursePayouts: statsData.nursePayouts || 0,
        refundedTransactions: statsData.refundedTransactions || 0,
        pendingPayments: statsData.pendingPayments || 0,
      });

      setTransactions(Array.isArray(transactionsData) ? transactionsData : []);
      setRecentActivity(Array.isArray(activityData) ? activityData : []);
    } catch (err) {
      setError(err.message || "Failed to load transactions");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const delay = setTimeout(() => {
      loadPageData();
    }, 300);

    return () => clearTimeout(delay);
  }, [searchTerm, statusFilter]);

  const handleExport = async () => {
    try {
      setExportLoading(true);
      await exportTransactionsPdf(searchTerm, statusFilter);
    } catch (err) {
      alert(err.message || "Failed to export PDF");
    } finally {
      setExportLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-6">
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center justify-between mb-3">
            <div className="w-12 h-12 bg-[#1F7A8C]/10 rounded-lg flex items-center justify-center">
              <DollarSign className="w-6 h-6 text-[#1F7A8C]" />
            </div>
          </div>
          <p className="text-xs text-gray-500 mb-1">Total Revenue</p>
          <h3 className="text-gray-800">{formatMoney(stats.totalRevenue)}</h3>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center justify-between mb-3">
            <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
              <TrendingUp className="w-6 h-6 text-green-600" />
            </div>
          </div>
          <p className="text-xs text-gray-500 mb-1">Platform Commission</p>
          <h3 className="text-gray-800">
            {formatMoney(stats.platformCommission)}
          </h3>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center justify-between mb-3">
            <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
              <Users className="w-6 h-6 text-blue-600" />
            </div>
          </div>
          <p className="text-xs text-gray-500 mb-1">Nurse Payouts</p>
          <h3 className="text-gray-800">{formatMoney(stats.nursePayouts)}</h3>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center justify-between mb-3">
            <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center">
              <RefreshCcw className="w-6 h-6 text-purple-600" />
            </div>
          </div>
          <p className="text-xs text-gray-500 mb-1">Refunded Transactions</p>
          <h3 className="text-gray-800">
            {formatMoney(stats.refundedTransactions)}
          </h3>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center justify-between mb-3">
            <div className="w-12 h-12 bg-yellow-100 rounded-lg flex items-center justify-center">
              <Clock className="w-6 h-6 text-yellow-600" />
            </div>
          </div>
          <p className="text-xs text-gray-500 mb-1">Pending Payments</p>
          <h3 className="text-gray-800">
            {formatMoney(stats.pendingPayments)}
          </h3>
        </div>
      </div>

      {/* Filters and Search */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex flex-col md:flex-row gap-4">
          <div className="flex-1 relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
            <input
              type="text"
              placeholder="Search by Transaction ID, Booking ID, Patient, or Nurse..."
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg bg-white"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>

          <div className="flex gap-3">
            <div className="relative">
              <Filter className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
              <select
                className="pl-10 pr-8 py-2 border border-gray-300 rounded-lg bg-white appearance-none cursor-pointer"
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
              >
                <option value="All">All Status</option>
                <option value="Paid">Paid</option>
                <option value="Pending">Pending</option>
                <option value="Refunded">Refunded</option>
                <option value="Failed">Failed</option>
              </select>
            </div>

            <button
              onClick={handleExport}
              disabled={exportLoading}
              className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors flex items-center gap-2 disabled:opacity-70"
            >
              <Download className="w-4 h-4" />
              {exportLoading ? "Exporting..." : "Export"}
            </button>
          </div>
        </div>
      </div>

      {/* Error */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-600 rounded-lg p-4">
          {error}
        </div>
      )}

      {/* Transactions Table */}
      <div className="bg-white rounded-lg shadow overflow-hidden">
        <div className="p-6 border-b border-gray-200">
          <h3 className="text-gray-800">All Transactions</h3>
          <p className="text-sm text-gray-500 mt-1">
            Showing {transactions.length} transactions
          </p>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Transaction ID
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Booking ID
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Patient
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Nurse
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Service
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Total Amount
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Commission
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Nurse Amount
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Status
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Payment Method
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Date
                </th>
              </tr>
            </thead>

            <tbody className="divide-y divide-gray-200">
              {loading ? (
                <tr>
                  <td
                    colSpan="11"
                    className="px-6 py-6 text-sm text-gray-500 text-center"
                  >
                    Loading...
                  </td>
                </tr>
              ) : transactions.length === 0 ? (
                <tr>
                  <td
                    colSpan="11"
                    className="px-6 py-6 text-sm text-gray-500 text-center"
                  >
                    No transactions found matching your criteria
                  </td>
                </tr>
              ) : (
                transactions.map((transaction) => (
                  <tr
                    key={transaction.paymentId}
                    className="hover:bg-gray-50 transition-colors"
                  >
                    <td className="px-6 py-4 text-sm text-gray-800">
                      {transaction.transactionId}
                    </td>
                    <td className="px-6 py-4 text-sm text-[#1F7A8C]">
                      {transaction.bookingCode}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-800">
                      {transaction.patientName}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-800">
                      {transaction.nurseName}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600">
                      {transaction.serviceName}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-800">
                      {formatMoney(transaction.totalAmount)}
                    </td>
                    <td className="px-6 py-4 text-sm text-green-600">
                      {formatMoney(transaction.commission)}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-800">
                      {formatMoney(transaction.nurseAmount)}
                    </td>
                    <td className="px-6 py-4">
                      {getStatusBadge(transaction.status)}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600">
                      {transaction.paymentMethod}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-600">
                      {formatDateTime(transaction.createdAt)}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Recent Financial Activity */}
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-gray-800 mb-4">Recent Financial Activity</h3>

        {loading ? (
          <p className="text-sm text-gray-500">Loading...</p>
        ) : recentActivity.length === 0 ? (
          <p className="text-sm text-gray-500">No recent activity found.</p>
        ) : (
          <div className="space-y-3">
            {recentActivity.map((activity, index) => {
              const activityType = mapActivityType(activity.type);

              return (
                <div
                  key={`${activity.createdAt}-${index}`}
                  className="flex items-center justify-between p-3 hover:bg-gray-50 rounded-lg transition-colors"
                >
                  <div className="flex items-center gap-4">
                    <div
                      className={`w-2 h-2 rounded-full ${
                        activityType === "credit"
                          ? "bg-green-500"
                          : "bg-red-500"
                      }`}
                    ></div>

                    <div>
                      <p className="text-sm text-gray-800">
                        {activity.title || activity.description}
                      </p>
                      <p className="text-xs text-gray-500 mt-1">
                        {getRelativeTime(activity.createdAt)}
                      </p>
                    </div>
                  </div>

                  <span
                    className={`text-sm ${
                      activityType === "credit"
                        ? "text-green-600"
                        : "text-red-600"
                    }`}
                  >
                    {activityType === "credit" ? "+" : "-"}
                    {formatMoney(activity.amount).replace("$", "$")}
                  </span>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}