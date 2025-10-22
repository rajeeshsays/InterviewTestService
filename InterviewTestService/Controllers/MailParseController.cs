using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Xml;
using Newtonsoft.Json;
using System.Xml.Linq;
using InterviewTestService.BL;
namespace InterviewTestService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailParseController : ControllerBase
    {
        IMailParseBL _mailParseBL;

        public MailParseController(IMailParseBL mailParseBL) {
            _mailParseBL = mailParseBL;
        }

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


            // adding root node in order to make whole mail content valid XML
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

            // find required tags and gets their values
            string totalStr = xmlRoot.Descendants("total").FirstOrDefault()?.Value;
            string costCentre = xmlRoot.Descendants("cost_centre").FirstOrDefault()?.Value ?? "UNKNOWN";
            string paymentMethod = xmlRoot.Descendants("cost_centre").FirstOrDefault()?.Value;
             // Validate required <total> tag
            if (string.IsNullOrWhiteSpace(totalStr))
                return BadRequest(new { error = "Missing required <total> field." });

            if (!decimal.TryParse(totalStr.Replace(",", ""), out decimal totalIncludingTax))
                return BadRequest(new { error = "Invalid total amount format." });

            // Calculate tax
            var tax = _mailParseBL.CalculateTax(totalIncludingTax);

            // Build final response
            var result = new
            {
                cost_centre = costCentre,
                payment_method = paymentMethod,
                total_including_tax = totalIncludingTax,
                total_excluding_tax = Math.Round(tax.TotalExcludingTax, 2),
                tax_amt = Math.Round(tax.SalesTax, 2),
                tax_rate = (tax.TaxRate * 100) + "%"
            };

            return Ok(result);
        }













    }
}
