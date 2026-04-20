const API_BASE_URL = "https://localhost:7053/api";

function getAuthHeaders() {
  const token = localStorage.getItem("token");

  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

async function handleResponse(response) {
  if (!response.ok) {
    let errorMessage = "Request failed";

    try {
      const errorData = await response.json();
      errorMessage = errorData.message || errorMessage;
    } catch {
      // ignore
    }

    throw new Error(errorMessage);
  }

  return response.json();
}

export async function sendNotification(payload) {
  const response = await fetch(`${API_BASE_URL}/admin/notifications/send`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(payload),
  });

  return handleResponse(response);
}

export async function fetchNotificationStats() {
  const response = await fetch(`${API_BASE_URL}/admin/notifications/stats`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function fetchRecentNotifications() {
  const response = await fetch(`${API_BASE_URL}/admin/notifications/recent`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}