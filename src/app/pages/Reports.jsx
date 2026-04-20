import { useEffect, useState } from "react";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  LineChart,
  Line,
} from "recharts";
import { Download, TrendingUp, Calendar } from "lucide-react";
import {
  fetchReportsOverview,
  fetchMonthlyUsage,
  fetchRevenueTrend,
  fetchNursePerformance,
  exportReportsPdf,
} from "../../services/reportsService";

export default function Reports() {
  const [overview, setOverview] = useState({
    totalRequests: 0,
    completionRate: 0,
    revenue: 0,
    requestsGrowthPercentage: 0,
    completionGrowthPercentage: 0,
    revenueGrowthPercentage: 0,
    currentMonthLabel: "",
  });

  const [monthlyUsageData, setMonthlyUsageData] = useState([]);
  const [revenueTrendData, setRevenueTrendData] = useState([]);
  const [nursePerformance, setNursePerformance] = useState([]);
  const [loading, setLoading] = useState(true);
  const [exportLoading, setExportLoading] = useState(false);
  const [error, setError] = useState("");

  const loadReports = async () => {
    try {
      setLoading(true);
      setError("");

      const [overviewData, monthlyUsage, revenueTrend, nursePerformanceData] =
        await Promise.all([
          fetchReportsOverview(),
          fetchMonthlyUsage(),
          fetchRevenueTrend(),
          fetchNursePerformance(),
        ]);

      setOverview({
        totalRequests: overviewData.totalRequests || 0,
        completionRate: overviewData.completionRate || 0,
        revenue: overviewData.revenue || 0,
        requestsGrowthPercentage: overviewData.requestsGrowthPercentage || 0,
        completionGrowthPercentage: overviewData.completionGrowthPercentage || 0,
        revenueGrowthPercentage: overviewData.revenueGrowthPercentage || 0,
        currentMonthLabel: overviewData.currentMonthLabel || "",
      });

      setMonthlyUsageData(
        Array.isArray(monthlyUsage)
          ? monthlyUsage.map((item) => ({
              month: item.month,
              requests: item.totalRequests,
              completed: item.completedRequests,
            }))
          : []
      );

      setRevenueTrendData(
        Array.isArray(revenueTrend)
          ? revenueTrend.map((item) => ({
              month: item.month,
              revenue: Number(item.revenue || 0),
            }))
          : []
      );

      setNursePerformance(
        Array.isArray(nursePerformanceData)
          ? nursePerformanceData.map((item) => ({
              name: item.nurseName,
              completed: item.completedRequests,
              rating: Number(item.averageRating || 0),
              revenue: Number(item.revenueGenerated || 0),
            }))
          : []
      );
    } catch (err) {
      setError(err.message || "Failed to load reports");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadReports();
  }, []);

  const handleExportPdf = async () => {
    try {
      setExportLoading(true);
      await exportReportsPdf();
    } catch (err) {
      alert(err.message || "Failed to export PDF");
    } finally {
      setExportLoading(false);
    }
  };

  const formatMoney = (value) => {
    const amount = Number(value || 0);
    return `$${amount.toLocaleString(undefined, {
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    })}`;
  };

  const renderGrowthText = (value) => {
    const numericValue = Number(value || 0);
    const sign = numericValue > 0 ? "+" : "";
    return `${sign}${numericValue}% from last month`;
  };

  const renderStars = (rating) => {
    return [...Array(5)].map((_, i) => (
      <span
        key={i}
        className={i < Math.floor(rating) ? "text-yellow-400" : "text-gray-300"}
      >
        ★
      </span>
    ));
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex flex-col md:flex-row gap-4 items-start md:items-center justify-between">
          <div>
            <h2 className="text-gray-800 mb-1">Reports & Analytics</h2>
            <p className="text-sm text-gray-500">
              View performance metrics and export reports
            </p>
          </div>

          <div className="flex gap-3">
            <button
              onClick={handleExportPdf}
              disabled={exportLoading}
              className="px-4 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors flex items-center gap-2 disabled:opacity-70"
            >
              <Download className="w-4 h-4" />
              {exportLoading ? "Exporting..." : "Export PDF"}
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

      {/* Key Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center gap-3 mb-2">
            <div className="w-10 h-10 bg-[#1F7A8C]/10 rounded-lg flex items-center justify-center">
              <Calendar className="w-5 h-5 text-[#1F7A8C]" />
            </div>
            <div>
              <p className="text-sm text-gray-500">
                Total Requests ({overview.currentMonthLabel || "Current"})
              </p>
              <p className="text-xl text-gray-800">
                {loading ? "..." : overview.totalRequests}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-1 text-sm text-green-600">
            <TrendingUp className="w-4 h-4" />
            <span>{renderGrowthText(overview.requestsGrowthPercentage)}</span>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center gap-3 mb-2">
            <div className="w-10 h-10 bg-green-100 rounded-lg flex items-center justify-center">
              <Calendar className="w-5 h-5 text-green-600" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Completion Rate</p>
              <p className="text-xl text-gray-800">
                {loading ? "..." : `${overview.completionRate}%`}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-1 text-sm text-green-600">
            <TrendingUp className="w-4 h-4" />
            <span>{renderGrowthText(overview.completionGrowthPercentage)}</span>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center gap-3 mb-2">
            <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
              <Calendar className="w-5 h-5 text-blue-600" />
            </div>
            <div>
              <p className="text-sm text-gray-500">
                Revenue ({overview.currentMonthLabel || "Current"})
              </p>
              <p className="text-xl text-gray-800">
                {loading ? "..." : formatMoney(overview.revenue)}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-1 text-sm text-green-600">
            <TrendingUp className="w-4 h-4" />
            <span>{renderGrowthText(overview.revenueGrowthPercentage)}</span>
          </div>
        </div>
      </div>

      {/* Monthly Usage Report */}
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-gray-800 mb-4">Monthly Usage Report</h3>

        {loading ? (
          <p className="text-sm text-gray-500">Loading...</p>
        ) : (
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={monthlyUsageData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="month" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Bar dataKey="requests" fill="#1F7A8C" name="Total Requests" />
              <Bar dataKey="completed" fill="#4CAF50" name="Completed" />
            </BarChart>
          </ResponsiveContainer>
        )}
      </div>

      {/* Revenue Trend */}
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-gray-800 mb-4">Revenue Trend</h3>

        {loading ? (
          <p className="text-sm text-gray-500">Loading...</p>
        ) : (
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={revenueTrendData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="month" />
              <YAxis />
              <Tooltip />
              <Legend />
              <Line
                type="monotone"
                dataKey="revenue"
                stroke="#1F7A8C"
                strokeWidth={2}
                name="Revenue ($)"
              />
            </LineChart>
          </ResponsiveContainer>
        )}
      </div>

      {/* Nurse Performance Summary */}
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-gray-800 mb-4">Nurse Performance Summary</h3>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Nurse Name
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Completed Requests
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Average Rating
                </th>
                <th className="px-6 py-3 text-left text-xs text-gray-600">
                  Revenue Generated
                </th>
              </tr>
            </thead>

            <tbody className="divide-y divide-gray-200">
              {loading ? (
                <tr>
                  <td
                    colSpan="4"
                    className="px-6 py-6 text-sm text-gray-500 text-center"
                  >
                    Loading...
                  </td>
                </tr>
              ) : nursePerformance.length === 0 ? (
                <tr>
                  <td
                    colSpan="4"
                    className="px-6 py-6 text-sm text-gray-500 text-center"
                  >
                    No nurse performance data found.
                  </td>
                </tr>
              ) : (
                nursePerformance.map((nurse, index) => (
                  <tr key={index} className="hover:bg-gray-50 transition-colors">
                    <td className="px-6 py-4 text-sm text-gray-800">
                      {nurse.name}
                    </td>

                    <td className="px-6 py-4 text-sm text-gray-600">
                      {nurse.completed}
                    </td>

                    <td className="px-6 py-4">
                      <div className="flex items-center gap-2">
                        <span className="text-sm text-gray-800">
                          {nurse.rating.toFixed(1)}
                        </span>
                        <div className="flex">{renderStars(nurse.rating)}</div>
                      </div>
                    </td>

                    <td className="px-6 py-4 text-sm text-gray-800">
                      {formatMoney(nurse.revenue)}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}