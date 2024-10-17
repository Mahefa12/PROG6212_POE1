using CMCS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CMCS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        private static List<ClaimModel> _claims = new List<ClaimModel>();

        [HttpPost]
        public IActionResult SubmitClaim(ClaimModel claim, IFormFile file)
        {
            // Defining maximum allowed file size (Made it 5MB)
            const long maxFileSize = 5 * 1024 * 1024; // 5MB

            // Defining allowed file types
            var allowedFileTypes = new[] { ".pdf", ".docx", ".xlsx" };

            if (file != null)
            {
                // Checking the file size
                if (file.Length > maxFileSize)
                {
                    ModelState.AddModelError("File", "The file size exceeds the 5MB limit.");
                    return View(claim); // Returning the same view with an error message
                }

                // Checking the file extension
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedFileTypes.Contains(fileExtension))
                {
                    ModelState.AddModelError("File", "Only PDF, DOCX, and XLSX files are allowed.");
                    return View(claim); // Returning the same view with an error message
                }

                // File is valid so proceed with saving it
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Storing the file path in the claim model so that the file uploaded is linked to the claim submitted
                claim.UploadedFilePath = Path.Combine("/uploads", file.FileName);
            }

            // Setting other claim required informations
            claim.SubmissionDate = DateTime.Now;
            claim.Status = "Pending Verification";
            claim.IsVerified = false;
            claim.IsApproved = false;

            // Assigning a ClaimId
            claim.ClaimId = _claims.Count > 0 ? _claims.Max(c => c.ClaimId) + 1 : 1;

            _claims.Add(claim);
            return RedirectToAction("TrackStatus");
        }

        public IActionResult VerifyClaims()
        {
            // Showing the claims that are pending verification and have not been rejected by the coordinator
            var pendingClaims = _claims.Where(c => !c.IsVerified && !c.IsRejected).ToList();
            return View(pendingClaims);
        }

        [HttpPost]
        public IActionResult VerifyClaim(int claimId)
        {
            // Finding the claim and marking it as verified
            var claim = _claims.FirstOrDefault(c => c.ClaimId == claimId);

            if (claim == null)
            {
                return NotFound();
            }

            if (claim != null)
            {
                claim.IsVerified = true;
                claim.Status = "Pending Approval";
            }

            return RedirectToAction("VerifyClaims");
        }

        [HttpPost]
        public IActionResult RejectClaimByCoordinator(int id)
        {
            var claim = _claims.FirstOrDefault(c => c.ClaimId == id);

            if (claim == null)
            {
                return NotFound();
            }

            if (claim != null)
            {
                claim.IsRejected = true;
                claim.Status = "Rejected by Coordinator";
                claim.IsVerified = false;
            }
            return RedirectToAction("VerifyClaims");
        }

        public IActionResult ApproveClaims()
        {
            // Showing claims that have been verified, are pending approval, and have not been rejected by the manager
            var verifiedClaims = _claims.Where(c => c.IsVerified && !c.IsApproved && !c.IsRejected).ToList();
            return View(verifiedClaims);
        }


        public IActionResult ApproveClaim(int claimId)
        {
            // Finding the claim and marking it as approved
            var claim = _claims.FirstOrDefault(c => c.ClaimId == claimId);

            if (claim == null)
            {
                return NotFound();
            }

            if (claim != null)
            {
                claim.IsApproved = true;
                claim.Status = "Approved";
            }

            return RedirectToAction("ApproveClaims");
        }

        [HttpPost]
        public IActionResult RejectClaimByManager(int id)
        {
            var claim = _claims.FirstOrDefault(c => c.ClaimId == id);

            if (claim == null)
            {
                return NotFound();
            }

            if (claim != null)
            {
                claim.IsRejected = true;
                claim.Status = "Rejected by Manager";
                claim.IsApproved = false;
            }
            return RedirectToAction("ApproveClaims");
        }

        public IActionResult TrackStatus()
        {
            // Passing the list of claims to the view
            return View(_claims);
        }

    }
}
