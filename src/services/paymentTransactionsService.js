const API_BASE_URL = "https://localhost:7053/api";

function getAuthHeaders() {
  const token = localStorage.getItem("token");

  return {
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

async function handleJsonResponse(response) {
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

export async function fetchTransactionStats() {
  const response = await fetch(`${API_BASE_URL}/admin/transactions/stats`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleJsonResponse(response);
}

export async function fetchTransactions(search = "", status = "All") {
  const query = new URLSearchParams();

  if (search) query.append("search", search);
  if (status && status !== "All") query.append("status", status);

  const response = await fetch(
    `${API_BASE_URL}/admin/transactions?${query.toString()}`,
    {
      method: "GET",
      headers: getAuthHeaders(),
    }
  );

  return handleJsonResponse(response);
}

export async function fetchRecentFinancialActivity() {
  const response = await fetch(`${API_BASE_URL}/admin/transactions/activity`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleJsonResponse(response);
}

export async function exportTransactionsPdf(search = "", status = "All") {
  const query = new URLSearchParams();

  if (search) query.append("search", search);
  if (status && status !== "All") query.append("status", status);

  const response = await fetch(
    `${API_BASE_URL}/admin/transactions/export-pdf?${query.toString()}`,
    {
      method: "GET",
      headers: getAuthHeaders(),
    }
  );

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
  link.download = "transactions-report.pdf";
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
}