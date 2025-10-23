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


        public class MailData
        {
            public string content { get; set; }
        }

        [HttpPost("parsecontent")]
        public IActionResult ParseContent([FromBody] MailData mailData)
        {
            string content = mailData?.content;
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest(new { error = "No data provided." });


            // adding root node in order to make whole mail content valid XML
            string xmlWrapped = $"<root>{content}</root>";

            XElement xmlRoot;
            try
            {
                xmlRoot = XElement.Parse(xmlWrapped);
            }
            catch(Exception ex)
            {
                return BadRequest(new { error = "Opening tags that have no corresponding closing tag. see the details " + ex.Message});
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
            var result = new Response
            {
                cost_centre = costCentre,
                payment_method = paymentMethod,
                total_including_tax = totalIncludingTax,
                total_excluding_tax = Math.Round(tax.TotalExcludingTax, 2),
                tax_amt = Math.Round(tax.TaxAmt, 2),
                tax_rate = (tax.TaxRate * 100) + "%"
            };

            return Ok(result);
        }

        public class Response
        {
                public string cost_centre { get; set; }
                public string? payment_method { get; set; }
                public decimal total_including_tax { get; set; }
                public decimal total_excluding_tax { get; set; }
                public decimal tax_amt { get; set; }
                public string? tax_rate { get; set; }
        }











    }
}
