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
                { "access_key", "1055e19f0b0e3c8a8091d6161d89d408" },
                { "profile_id", "20496836-60A9-43F9-8568-5466F6D448E1" },
                { "transaction_uuid", Guid.NewGuid().ToString() },
                { "signed_field_names", "access_key,profile_id,transaction_uuid,signed_field_names,unsigned_field_names,signed_date_time,locale,transaction_type,reference_number,amount,currency" },
                { "unsigned_field_names", "" },
                { "signed_date_time", DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'") },
                { "locale", "en" },
                { "transaction_type", "sale" },
                { "reference_number", "123456789" }, // Ensure this is unique for each transaction
                { "amount", "10.00" },
                { "currency", "USD" },
                {"submit","Submit" }
            };

            // Generate the signature
            string signature = Sign(parameters, "5ef065aba29d404bb0afbfadbe8d889105fc31d5716f46cf89e7c24fa629b373b559b201dd084fcba57862cd67ad24d1e37ef46df1ba446bb8151f90940d97172834fcec2fb940d1aedba8662c2b501af80bfa3609cb4435ac988866cf6c955758607987a86744f99175527f94a9ee635702a8de89284a7581d9ec91062f518c");
            parameters.Add("signature", signature);

            return View("SecureAcceptanceForm", parameters);
        }

        public static string Sign(IDictionary<string, string> paramsArray, string secret)
        {
            return Sign(BuildDataToSign(paramsArray), secret);
        }

        private static string Sign(string data, string secretKey)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secretKey);

            using (HMACSHA256 hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] messageBytes = encoding.GetBytes(data);
                return Convert.ToBase64String(hmacsha256.ComputeHash(messageBytes));
            }
        }

        private static string BuildDataToSign(IDictionary<string, string> paramsArray)
        {
            string[] signedFieldNames = paramsArray["signed_field_names"].Split(',');
            IList<string> dataToSign = new List<string>();

            foreach (string signedFieldName in signedFieldNames)
            {
                dataToSign.Add(signedFieldName + "=" + paramsArray[signedFieldName]);
            }

            return string.Join(",", dataToSign);
        }
    }
}
