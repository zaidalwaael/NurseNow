import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router";
import { Bell, Check, CheckCheck, Eye } from "lucide-react";

const mockNotifications = [
  {
    id: "N001",
    title: "New Nurse Verification Request",
    description: "Dr. Sarah Johnson submitted verification documents",
    timestamp: "5 mins ago",
    isRead: false,
    type: "verification",
  },
  {
    id: "N002",
    title: "Payment Completed",
    description: "Transaction #TXN001234 has been processed successfully",
    timestamp: "15 mins ago",
    isRead: false,
    type: "payment",
  },
  {
    id: "N003",
    title: "New Complaint Submitted",
    description: "Patient John Smith submitted complaint #4523",
    timestamp: "1 hour ago",
    isRead: false,
    type: "complaint",
  },
  {
    id: "N004",
    title: "Booking Status Changed",
    description: "Booking #BK2025007 status changed to Completed",
    timestamp: "2 hours ago",
    isRead: true,
    type: "booking",
  },
  {
    id: "N005",
    title: "New Nurse Verification Request",
    description: "Nurse Michael Brown submitted verification documents",
    timestamp: "3 hours ago",
    isRead: true,
    type: "verification",
  },
];

export function NotificationDropdown({ isOpen, onClose }) {
  const dropdownRef = useRef(null);
  const navigate = useNavigate();
  const [notifications, setNotifications] = useState(mockNotifications);

  useEffect(() => {
    function handleClickOutside(event) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        onClose();
      }
    }

    if (isOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  const unreadCount = notifications.filter((n) => !n.isRead).length;

  const markAsRead = (id) => {
    setNotifications((prev) =>
      prev.map((notif) =>
        notif.id === id ? { ...notif, isRead: true } : notif,
      ),
    );
  };

  const markAllAsRead = () => {
    setNotifications((prev) =>
      prev.map((notif) => ({ ...notif, isRead: true })),
    );
  };

  const getTypeColor = (type) => {
    const colors = {
      verification: "bg-blue-100 text-blue-600",
      payment: "bg-green-100 text-green-600",
      complaint: "bg-red-100 text-red-600",
      booking: "bg-purple-100 text-purple-600",
    };
    return colors[type] || "bg-gray-100 text-gray-600";
  };

  return (
    <div
      ref={dropdownRef}
      className="absolute top-14 right-0 w-96 bg-white rounded-lg shadow-lg border border-gray-200 z-50"
    >
      {/* Header */}
      <div className="p-4 border-b border-gray-200 flex items-center justify-between">
        <div>
          <h3 className="text-gray-800">Notifications</h3>
          {unreadCount > 0 && (
            <p className="text-xs text-gray-500 mt-1">
              {unreadCount} unread notification(s)
            </p>
          )}
        </div>
        {unreadCount > 0 && (
          <button
            onClick={markAllAsRead}
            className="text-xs text-[#1F7A8C] hover:text-[#18626F] transition-colors flex items-center gap-1"
          >
            <CheckCheck className="w-3 h-3" />
            Mark all read
          </button>
        )}
      </div>

      {/* Notifications List */}
      <div className="max-h-96 overflow-y-auto smooth-scrollbar">
        {notifications.map((notification) => (
          <div
            key={notification.id}
            className={`p-4 border-b border-gray-100 hover:bg-gray-50 transition-colors ${
              !notification.isRead ? "bg-blue-50/30" : ""
            }`}
          >
            <div className="flex items-start gap-3">
              <div
                className={`w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0 ${getTypeColor(notification.type)}`}
              >
                <Bell className="w-4 h-4" />
              </div>

              <div className="flex-1 min-w-0">
                <div className="flex items-start justify-between gap-2">
                  <h4 className="text-sm text-gray-800">
                    {notification.title}
                  </h4>
                  {!notification.isRead && (
                    <div className="w-2 h-2 bg-[#1F7A8C] rounded-full flex-shrink-0 mt-1"></div>
                  )}
                </div>
                <p className="text-xs text-gray-600 mt-1">
                  {notification.description}
                </p>
                <p className="text-xs text-gray-500 mt-2">
                  {notification.timestamp}
                </p>

                {!notification.isRead && (
                  <button
                    onClick={() => markAsRead(notification.id)}
                    className="text-xs text-[#1F7A8C] hover:text-[#18626F] transition-colors mt-2 flex items-center gap-1"
                  >
                    <Check className="w-3 h-3" />
                    Mark as read
                  </button>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Footer */}
      <div className="p-3 border-t border-gray-200 text-center">
        <button
          onClick={() => {
            navigate("/notifications/all");
            onClose();
          }}
          className="text-sm text-[#1F7A8C] hover:text-[#18626F] transition-colors flex items-center justify-center gap-1 w-full"
        >
          <Eye className="w-4 h-4" />
          View all notifications
        </button>
      </div>
    </div>
  );
}
