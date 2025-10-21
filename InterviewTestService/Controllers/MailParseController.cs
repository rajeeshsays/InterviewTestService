using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Xml;
using Newtonsoft.Json;
using System.Xml.Linq;
namespace InterviewTestService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailParseController : ControllerBase
    {
        public MailParseController() { }

        [HttpPost("parsedata")]
        public IActionResult ParsedData([FromBody] MailData mailData)
        {
            try
            {


                //string xmlData = @"<expense>
                //              <cost_centre>DEV632</cost_centre>
                //              <total>35,000</total>
                //              <payment_method>personal card</payment_method>
                //           </expens>";

                // Load the XML
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(mailData.data);
                

                // Convert XML to JSON
                string json = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);

                // Display JSON
                Console.WriteLine(json);


                return Ok(json);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        public class MailData
        {
            public string data { get; set; }
        }

        [HttpPost("parsedatav")]
        public IActionResult ParseDatav([FromBody] MailData mailData)
        {
            string data = mailData?.data;
            if (string.IsNullOrWhiteSpace(data))
                return BadRequest(new { error = "No data provided." });

            // 🧹 Step 1: Extract all possible tagged fields (no outer expense block)
            // To detect malformed XML, wrap the message in a fake root node
            string xmlWrapped = $"<root>{data}</root>";

            XElement xmlRoot;
            try
            {
                xmlRoot = XElement.Parse(xmlWrapped);
            }
            catch(Exception ex)
            {
                return BadRequest(new { error = "Opening tags that have no corresponding closing tag." + ex.Message});
            }

            // 🔍 Search all descendants (not just top-level)
            string totalStr = xmlRoot.Descendants("total").FirstOrDefault()?.Value;
            string costCentre = xmlRoot.Descendants("cost_centre").FirstOrDefault()?.Value ?? "UNKNOWN";
            string paymentMethod = xmlRoot.Descendants("payment_method").FirstOrDefault()?.Value ?? "UNKNOWN";


            // 🧮 Step 3: Validate required <total>
            if (string.IsNullOrWhiteSpace(totalStr))
                return BadRequest(new { error = "Missing required <total> field." });

            if (!decimal.TryParse(totalStr.Replace(",", ""), out decimal totalIncludingTax))
                return BadRequest(new { error = "Invalid total amount format." });

            // 🧮 Step 4: Calculate tax
            const decimal taxRate = 0.10m; // 10%
            decimal totalExcludingTax = totalIncludingTax / (1 + taxRate);
            decimal salesTax = totalIncludingTax - totalExcludingTax;

            // ✅ Step 5: Build final response
            var result = new
            {
                cost_centre = costCentre,
                payment_method = paymentMethod,
                total_including_tax = totalIncludingTax,
                total_excluding_tax = Math.Round(totalExcludingTax, 2),
                sales_tax = Math.Round(salesTax, 2),
                tax_rate = (taxRate * 100) + "%"
            };

            return Ok(result);
        }













    }
}
