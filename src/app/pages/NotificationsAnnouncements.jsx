import { useEffect, useState } from "react";
import { Send, Users, UserCheck, Bell } from "lucide-react";
import {
  sendNotification,
  fetchNotificationStats,
  fetchRecentNotifications,
} from "../../services/notificationsAnnouncementsService";

export default function NotificationsAnnouncements() {
  const [title, setTitle] = useState("");
  const [message, setMessage] = useState("");
  const [target, setTarget] = useState("All Users");
  const [stats, setStats] = useState({
    allUsers: 0,
    nursesOnly: 0,
    patientsOnly: 0,
  });
  const [recentNotifications, setRecentNotifications] = useState([]);
  const [loading, setLoading] = useState(true);
  const [sending, setSending] = useState(false);
  const [error, setError] = useState("");

  const loadPageData = async () => {
    try {
      setLoading(true);
      setError("");

      const [statsData, recentData] = await Promise.all([
        fetchNotificationStats(),
        fetchRecentNotifications(),
      ]);

      setStats({
        allUsers: statsData.allUsers || 0,
        nursesOnly: statsData.nursesOnly || 0,
        patientsOnly: statsData.patientsOnly || 0,
      });

      setRecentNotifications(Array.isArray(recentData) ? recentData : []);
    } catch (err) {
      setError(err.message || "Failed to load notifications data");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPageData();
  }, []);

  const handleSend = async () => {
    if (!title.trim() || !message.trim()) {
      return;
    }

    try {
      setSending(true);

      await sendNotification({
        targetAudience: target,
        title,
        message,
      });

      setTitle("");
      setMessage("");
      setTarget("All Users");

      await loadPageData();
    } catch (err) {
    } finally {
      setSending(false);
    }
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
        <h2 className="text-gray-800 mb-1">Notifications & Announcements</h2>
        <p className="text-sm text-gray-500">
          Send system-wide or targeted notifications to users
        </p>
      </div>

      {/* Error */}
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-600 rounded-lg p-4">
          {error}
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Send New Notification */}
        <div className="bg-white rounded-lg shadow p-6">
          <h3 className="text-gray-800 mb-4 flex items-center gap-2">
            <Bell className="w-5 h-5 text-[#1F7A8C]" />
            Create New Notification
          </h3>

          <div className="space-y-4">
            <div>
              <label className="block text-sm text-gray-700 mb-2">
                Target Audience
              </label>
              <select
                className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white"
                value={target}
                onChange={(e) => setTarget(e.target.value)}
              >
                <option value="All Users">All Users</option>
                <option value="Nurses Only">Nurses Only</option>
                <option value="Patients Only">Patients Only</option>
              </select>
            </div>

            <div>
              <label className="block text-sm text-gray-700 mb-2">
                Notification Title
              </label>
              <input
                type="text"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white"
                placeholder="Enter notification title..."
                value={title}
                onChange={(e) => setTitle(e.target.value)}
              />
            </div>

            <div>
              <label className="block text-sm text-gray-700 mb-2">
                Message
              </label>
              <textarea
                className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white"
                rows={6}
                placeholder="Type your message here..."
                value={message}
                onChange={(e) => setMessage(e.target.value)}
              />
            </div>

            <button
              onClick={handleSend}
              disabled={sending}
              className="w-full px-4 py-2 bg-[#1F7A8C] text-white rounded-lg hover:bg-[#18626F] transition-colors flex items-center justify-center gap-2 disabled:opacity-70"
            >
              <Send className="w-4 h-4" />
              {sending ? "Sending..." : "Send Notification"}
            </button>
          </div>
        </div>

        {/* Quick Templates + Stats */}
        <div className="space-y-6">
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-gray-800 mb-4">Quick Templates</h3>
            <div className="space-y-3">
              <button
                onClick={() => {
                  setTitle("Nurse Verification Approved");
                  setMessage(
                    "Congratulations! Your nurse verification has been approved. You can now start accepting service requests."
                  );
                  setTarget("Nurses Only");
                }}
                className="w-full text-left p-3 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
              >
                <p className="text-sm text-gray-800">
                  Nurse Approval Notification
                </p>
                <p className="text-xs text-gray-500 mt-1">
                  For approved nurse verifications
                </p>
              </button>

              <button
                onClick={() => {
                  setTitle("Nurse Verification Rejected");
                  setMessage(
                    "Your nurse verification application requires additional information. Please review and resubmit."
                  );
                  setTarget("Nurses Only");
                }}
                className="w-full text-left p-3 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
              >
                <p className="text-sm text-gray-800">
                  Nurse Rejection Notification
                </p>
                <p className="text-xs text-gray-500 mt-1">
                  For rejected applications
                </p>
              </button>

              <button
                onClick={() => {
                  setTitle("Appointment Reminder");
                  setMessage(
                    "This is a reminder for your upcoming appointment scheduled for tomorrow."
                  );
                  setTarget("Patients Only");
                }}
                className="w-full text-left p-3 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
              >
                <p className="text-sm text-gray-800">Appointment Reminder</p>
                <p className="text-xs text-gray-500 mt-1">
                  For upcoming appointments
                </p>
              </button>
            </div>
          </div>

          {/* Target Audience Stats */}
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-gray-800 mb-4">Target Audience</h3>

            {loading ? (
              <p className="text-sm text-gray-500">Loading...</p>
            ) : (
              <div className="space-y-3">
                <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                  <div className="flex items-center gap-3">
                    <Users className="w-5 h-5 text-[#1F7A8C]" />
                    <span className="text-sm text-gray-800">All Users</span>
                  </div>
                  <span className="text-sm text-gray-600">
                    {stats.allUsers.toLocaleString()}
                  </span>
                </div>

                <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                  <div className="flex items-center gap-3">
                    <UserCheck className="w-5 h-5 text-[#1F7A8C]" />
                    <span className="text-sm text-gray-800">Nurses Only</span>
                  </div>
                  <span className="text-sm text-gray-600">
                    {stats.nursesOnly.toLocaleString()}
                  </span>
                </div>

                <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                  <div className="flex items-center gap-3">
                    <Users className="w-5 h-5 text-[#1F7A8C]" />
                    <span className="text-sm text-gray-800">Patients Only</span>
                  </div>
                  <span className="text-sm text-gray-600">
                    {stats.patientsOnly.toLocaleString()}
                  </span>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Recent Notifications */}
      <div className="bg-white rounded-lg shadow p-6">
        <h3 className="text-gray-800 mb-4">Recently Sent Notifications</h3>

        {loading ? (
          <p className="text-sm text-gray-500">Loading...</p>
        ) : recentNotifications.length === 0 ? (
          <p className="text-sm text-gray-500">
            No recent notifications found.
          </p>
        ) : (
          <div className="space-y-3">
            {recentNotifications.map((notif, index) => (
              <div
                key={notif.notificationId || index}
                className="p-4 border border-gray-200 rounded-lg"
              >
                <div className="flex items-start justify-between mb-2">
                  <h4 className="text-sm text-gray-800">{notif.title}</h4>
                  <span className="text-xs text-gray-500">
                    {formatDateTime(notif.createdAt)}
                  </span>
                </div>

                <p className="text-sm text-gray-600 mb-2">{notif.message}</p>

                <div className="flex items-center gap-4 text-xs text-gray-500">
                  <span>Target: {notif.targetAudience}</span>
                  <span>•</span>
                  <span>Sent by: {notif.sentBy}</span>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}