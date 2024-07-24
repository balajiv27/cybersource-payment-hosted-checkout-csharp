using Microsoft.AspNetCore.Mvc;
using payment_integration.Models;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace payment_integration.Controllers
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


        [HttpPost]
        public ActionResult GenerateSecureAcceptanceForm()
        {
            var parameters = new Dictionary<string, string>
            {
                { "access_key", "27b093adf2d3332eaadec751362393d1" },
                { "profile_id", "CFBF282F-BB79-4F9F-9D64-6F34E916769C" },
                { "transaction_uuid", Guid.NewGuid().ToString() },
                { "signed_field_names", "access_key,profile_id,transaction_uuid,signed_field_names,unsigned_field_names,signed_date_time,locale,transaction_type,reference_number,amount,currency" },
                { "unsigned_field_names", "" },
                { "signed_date_time", DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") },
                { "locale", "en" },
                { "transaction_type", "sale" },
                { "reference_number", "123456789" }, // Ensure this is unique for each transaction
                { "amount", "100.00" },
                { "currency", "USD" }
            };

            // Generate the signature
            string signature = Sign(parameters, "61a516f7af2240849620108b82342a482086cf174df44c4a878417c4f188fe48f6b73ebfaa2f4715b4c7df53de63743e49efa500ca904740ba570efc3dcb1088e7f405c982f24a4087aeaea5888e7ab354de219805b047eb96e8c1196675fbba911f5d01eb2d4fa7980264d11cc575749cea2e3315bf4b71a87227548f5bc1bb");
            parameters.Add("signature", signature);

            // Render the view with the parameters to post to CyberSource
            return View("SecureAcceptanceForm", parameters);
        }

        private string Sign(Dictionary<string, string> data, string secretKey)
        {
            var sortedData = new SortedDictionary<string, string>(data);
            var signingString = new StringBuilder();
            foreach (var item in sortedData)
            {
                signingString.Append(item.Key).Append("=").Append(item.Value).Append(",");
            }

            signingString.Length--; // Remove the last comma
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingString.ToString()));
                return Convert.ToBase64String(hash);
            }
        }
    }
}
