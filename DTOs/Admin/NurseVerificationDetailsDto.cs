using System;
using System.Collections.Generic;

namespace NurseNow.DTOs.Admin
{
    public class NurseVerificationDetailsDto
    {
        public string Id { get; set; } = "";
        public string NurseId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public DateTime? RegistrationDate { get; set; }
        public string Status { get; set; } = "";
        public List<NurseVerificationDocumentDto> Documents { get; set; } = new();

        public string? RejectionReason { get; set; }

    }
}