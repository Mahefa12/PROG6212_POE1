using System;

namespace CMCS.Models
{
    public class ClaimModel
    {
        public int ClaimId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }

        // Calculating total amount based on hours worked and hourly rate
        public decimal TotalAmount => HoursWorked * HourlyRate;

        public DateTime SubmissionDate { get; set; }

        //Setting the status of the claim (initial status should be false for all, not approved, not verified, not rejected)
        public string Status { get; set; } 
        public bool IsVerified { get; set; } = false; 
        public bool IsApproved { get; set; } = false; 
        public bool IsRejected { get; set; } = false;  

        public string UploadedFilePath { get; set; }
    }
}

