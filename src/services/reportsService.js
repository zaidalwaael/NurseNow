const API_BASE_URL = "https://localhost:7053/api";

function getAuthHeaders() {
  const token = localStorage.getItem("token");

  return {
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

export async function fetchReportsOverview() {
  const response = await fetch(`${API_BASE_URL}/admin/reports/overview`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function fetchMonthlyUsage() {
  const response = await fetch(`${API_BASE_URL}/admin/reports/monthly-usage`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function fetchRevenueTrend() {
  const response = await fetch(`${API_BASE_URL}/admin/reports/revenue-trend`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function fetchNursePerformance() {
  const response = await fetch(`${API_BASE_URL}/admin/reports/nurse-performance`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function exportReportsPdf() {
  const response = await fetch(`${API_BASE_URL}/admin/reports/export-pdf`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  if (!response.ok) {
    let errorMessage = "Failed to export PDF";

    try {
      const errorData = await response.json();
      errorMessage = errorData.message || errorMessage;
    } catch {
      // ignore
    }

    throw new Error(errorMessage);
  }

  const blob = await response.blob();
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = "reports-analytics.pdf";
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
}