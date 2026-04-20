import { useEffect, useState } from "react";
import {
  Users,
  UserCheck,
  Clock,
  ClipboardList,
  TrendingUp,
  TrendingDown,
} from "lucide-react";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
} from "recharts";

import {
  fetchDashboardSummary,
  fetchRequestStatusDistribution,
  fetchWeeklyActivity,
  fetchRecentActivity,
} from "../../services/dashboardService";

const CARD_CONFIG = {
  totalPatients: {
    title: "Total Patients",
    icon: Users,
    color: "#1F7A8C",
  },
  totalNurses: {
    title: "Total Nurses",
    icon: UserCheck,
    color: "#4CAF50",
  },
  pendingVerifications: {
    title: "Pending Verifications",
    icon: Clock,
    color: "#FFC107",
  },
  todaysRequests: {
    title: "Today's Requests",
    icon: ClipboardList,
    color: "#FF5722",
  },
};

const STATUS_COLORS = {
  Pending: "#FFC107",
  Assigned: "#2196F3",
  Accepted: "#2196F3",
  Active: "#9C27B0",
  Completed: "#4CAF50",
  Cancelled: "#FF5722",
  Rejected: "#F44336",
};

function formatRelativeTime(dateString) {
  const now = new Date();
  const created = new Date(dateString);
  const diffMs = now - created;

  const minutes = Math.floor(diffMs / (1000 * 60));
  const hours = Math.floor(diffMs / (1000 * 60 * 60));
  const days = Math.floor(diffMs / (1000 * 60 * 60 * 24));

  if (minutes < 1) return "Just now";
  if (minutes < 60) return `${minutes} min${minutes > 1 ? "s" : ""} ago`;
  if (hours < 24) return `${hours} hour${hours > 1 ? "s" : ""} ago`;
  return `${days} day${days > 1 ? "s" : ""} ago`;
}

export default function Dashboard() {
  const [summary, setSummary] = useState(null);
  const [statusData, setStatusData] = useState([]);
  const [weeklyActivity, setWeeklyActivity] = useState([]);
  const [recentActivities, setRecentActivities] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function loadDashboard() {
      try {
        setLoading(true);
        setError("");

        const [
          summaryRes,
          statusRes,
          weeklyRes,
          recentRes,
        ] = await Promise.all([
          fetchDashboardSummary(),
          fetchRequestStatusDistribution(),
          fetchWeeklyActivity(),
          fetchRecentActivity(5),
        ]);

        setSummary(summaryRes);

        const mappedStatusData = statusRes.map((item) => ({
          name: item.status,
          value: item.count,
          color: STATUS_COLORS[item.status] || "#1F7A8C",
        }));
        setStatusData(mappedStatusData);

        const mappedWeeklyData = weeklyRes.map((item) => ({
          day: item.day,
          requests: item.requests,
          completed: item.completed,
        }));
        setWeeklyActivity(mappedWeeklyData);

        const mappedRecentActivities = recentRes.map((item) => ({
          title: item.title,
          description: item.description,
          time: formatRelativeTime(item.createdAt),
        }));
        setRecentActivities(mappedRecentActivities);
      } catch (err) {
        setError(err.message || "Failed to load dashboard data");
      } finally {
        setLoading(false);
      }
    }

    loadDashboard();
  }, []);

  const statsCards = summary
    ? [
        {
          key: "totalPatients",
          title: CARD_CONFIG.totalPatients.title,
          value: summary.totalPatients ?? 0,
          icon: CARD_CONFIG.totalPatients.icon,
          color: CARD_CONFIG.totalPatients.color,
        },
        {
          key: "totalNurses",
          title: CARD_CONFIG.totalNurses.title,
          value: summary.totalNurses ?? 0,
          icon: CARD_CONFIG.totalNurses.icon,
          color: CARD_CONFIG.totalNurses.color,
        },
        {
          key: "pendingVerifications",
          title: CARD_CONFIG.pendingVerifications.title,
          value: summary.pendingVerifications ?? 0,
          icon: CARD_CONFIG.pendingVerifications.icon,
          color: CARD_CONFIG.pendingVerifications.color,
        },
        {
          key: "todaysRequests",
          title: CARD_CONFIG.todaysRequests.title,
          value: summary.todaysRequests ?? 0,
          icon: CARD_CONFIG.todaysRequests.icon,
          color: CARD_CONFIG.todaysRequests.color,
        },
      ]
    : [];

  if (loading) {
    return (
      <div className="p-6">
        <p className="text-gray-600">Loading dashboard...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-6">
        <div className="bg-red-50 text-red-600 border border-red-200 rounded-lg p-4">
          {error}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {statsCards.map((stat) => {
          const Icon = stat.icon;

          return (
            <div key={stat.key} className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center justify-between mb-4">
                <div
                  className="w-12 h-12 rounded-lg flex items-center justify-center"
                  style={{ backgroundColor: `${stat.color}20` }}
                >
                  <Icon className="w-6 h-6" style={{ color: stat.color }} />
                </div>

                <div className="flex items-center gap-1 text-sm text-gray-500">
                  <TrendingUp className="w-4 h-4" />
                </div>
              </div>

              <h3 className="text-gray-500 text-sm mb-1">{stat.title}</h3>
              <p className="text-2xl text-gray-800">{stat.value}</p>
            </div>
          );
        })}
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Request Status Chart */}
        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-gray-800 mb-4">Request Status Distribution</h3>

          {statusData.length === 0 ? (
            <p className="text-gray-500 text-sm">No data available.</p>
          ) : (
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={statusData}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={({ name, percent }) =>
                    `${name}: ${(percent * 100).toFixed(0)}%`
                  }
                  outerRadius={100}
                  dataKey="value"
                >
                  {statusData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          )}
        </div>

        {/* Weekly Activity Chart */}
        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-gray-800 mb-4">Weekly Activity Overview</h3>

          {weeklyActivity.length === 0 ? (
            <p className="text-gray-500 text-sm">No data available.</p>
          ) : (
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={weeklyActivity}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="day" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Line
                  type="monotone"
                  dataKey="requests"
                  stroke="#1F7A8C"
                  strokeWidth={2}
                  name="Requests"
                />
                <Line
                  type="monotone"
                  dataKey="completed"
                  stroke="#4CAF50"
                  strokeWidth={2}
                  name="Completed"
                />
              </LineChart>
            </ResponsiveContainer>
          )}
        </div>
      </div>

      {/* Recent Activity Log */}
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-gray-800 mb-4">Recent Activity</h3>

        <div className="space-y-3">
          {recentActivities.length === 0 ? (
            <p className="text-gray-500 text-sm">No recent activity.</p>
          ) : (
            recentActivities.map((activity, index) => (
              <div
                key={index}
                className="flex items-start gap-4 p-3 hover:bg-gray-50 rounded-lg transition-colors"
              >
                <div className="w-2 h-2 bg-[#1F7A8C] rounded-full mt-2"></div>
                <div className="flex-1">
                  <p className="text-sm text-gray-800">{activity.title}</p>
                  {activity.description && (
                    <p className="text-sm text-gray-600 mt-1">
                      {activity.description}
                    </p>
                  )}
                  <p className="text-xs text-gray-500 mt-1">{activity.time}</p>
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
}