import { useEffect, useState } from "react";
import { Link } from "react-router";
import {
  Bell,
  ArrowLeft,
  CheckCircle,
  AlertTriangle,
  Info,
} from "lucide-react";
import { fetchAllNotifications } from "../../services/allNotificationsService";

const typeStyles = {
  success: "bg-green-100 text-green-700",
  warning: "bg-yellow-100 text-yellow-700",
  info: "bg-blue-100 text-blue-700",
};

const typeIcon = {
  success: CheckCircle,
  warning: AlertTriangle,
  info: Info,
};

export default function AllNotifications() {
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const getNotificationType = (type) => {
    if (!type) return "info";

    const normalized = type.toLowerCase();

    if (
      normalized.includes("payment") ||
      normalized.includes("resolved") ||
      normalized.includes("success")
    ) {
      return "success";
    }

    if (
      normalized.includes("complaint") ||
      normalized.includes("warning") ||
      normalized.includes("rejected") ||
      normalized.includes("failed")
    ) {
      return "warning";
    }

    return "info";
  };

  const formatRelativeTime = (value) => {
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

  const loadNotifications = async () => {
    try {
      setLoading(true);
      setError("");

      const data = await fetchAllNotifications();
      setNotifications(Array.isArray(data) ? data : []);
    } catch (err) {
      setError(err.message || "Failed to load notifications");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadNotifications();
  }, []);

  return (
    <div className="space-y-6">
      <div className="bg-white rounded-lg shadow p-6 flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
        <div className="flex items-center gap-4">
          <div className="w-12 h-12 rounded-full bg-[#1F7A8C] text-white flex items-center justify-center">
            <Bell className="w-6 h-6" />
          </div>

          <div>
            <h2 className="text-2xl font-semibold text-gray-800">
              All Notifications
            </h2>
            <p className="text-sm text-gray-500 mt-1">
              View every notification received by the admin panel.
            </p>
          </div>
        </div>

        <Link
          to="/notifications"
          className="inline-flex items-center gap-2 px-4 py-2 rounded-lg border border-gray-200 text-gray-700 hover:bg-gray-50 transition-colors"
        >
          <ArrowLeft className="w-4 h-4" />
          Back to Notifications
        </Link>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-600 rounded-lg p-4">
          {error}
        </div>
      )}

      {loading ? (
        <div className="bg-white rounded-lg shadow border border-gray-200 p-6 text-sm text-gray-500">
          Loading...
        </div>
      ) : notifications.length === 0 ? (
        <div className="bg-white rounded-lg shadow border border-gray-200 p-6 text-sm text-gray-500">
          No notifications found.
        </div>
      ) : (
        <div className="grid gap-4">
          {notifications.map((notification, index) => {
            const visualType = getNotificationType(notification.type);
            const Icon = typeIcon[visualType] || Info;

            return (
              <div
                key={notification.notificationId || index}
                className="bg-white rounded-lg shadow border border-gray-200 p-5"
              >
                <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
                  <div className="flex items-center gap-4">
                    <div
                      className={`w-11 h-11 rounded-full flex items-center justify-center ${typeStyles[visualType]}`}
                    >
                      <Icon className="w-5 h-5" />
                    </div>

                    <div>
                      <h3 className="text-lg font-semibold text-gray-800">
                        {notification.title}
                      </h3>
                      <p className="text-sm text-gray-500 mt-1">
                        {formatRelativeTime(notification.createdAt)} ·{" "}
                        {notification.targetAudience || "Admin"}
                      </p>
                    </div>
                  </div>

                  <span className="text-xs text-gray-500">
                    Sent by {notification.sentBy || "System"}
                  </span>
                </div>

                <p className="text-sm text-gray-600 mt-4">
                  {notification.message}
                </p>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}