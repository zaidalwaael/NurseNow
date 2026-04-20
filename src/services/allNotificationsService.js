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

export async function fetchAllNotifications() {
  const response = await fetch(`${API_BASE_URL}/admin/all-notifications`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}