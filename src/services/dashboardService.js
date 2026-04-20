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

export async function fetchDashboardSummary() {
  const response = await fetch(`${API_BASE_URL}/admin/dashboard/summary`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function fetchRequestStatusDistribution() {
  const response = await fetch(`${API_BASE_URL}/admin/dashboard/request-status-distribution`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function fetchWeeklyActivity() {
  const response = await fetch(`${API_BASE_URL}/admin/dashboard/weekly-activity`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function fetchRecentActivity(limit = 5) {
  const response = await fetch(`${API_BASE_URL}/admin/dashboard/recent-activity?limit=${limit}`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}