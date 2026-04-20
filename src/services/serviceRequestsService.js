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

export async function fetchServiceRequests(search = "", status = "All") {
  const query = new URLSearchParams();

  if (search) query.append("search", search);
  if (status && status !== "All") query.append("status", status);

  const response = await fetch(
    `${API_BASE_URL}/admin/requests?${query.toString()}`,
    {
      method: "GET",
      headers: getAuthHeaders(),
    }
  );

  return handleResponse(response);
}

export async function fetchServiceRequestDetails(id) {
  const response = await fetch(`${API_BASE_URL}/admin/requests/${id}`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  return handleResponse(response);
}

export async function reassignServiceRequest(id, nurseId) {
  const response = await fetch(`${API_BASE_URL}/admin/requests/${id}/reassign`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify({ nurseId }),
  });

  return handleResponse(response);
}

export async function cancelServiceRequest(id, reason = "") {
  const response = await fetch(`${API_BASE_URL}/admin/requests/${id}/cancel`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify({ reason }),
  });

  return handleResponse(response);
}