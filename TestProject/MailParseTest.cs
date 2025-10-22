using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using InterviewTestService.Controllers;
using InterviewTestService.BL;
using System;
using static InterviewTestService.BL.MailParseBL;
using static InterviewTestService.Controllers.MailParseController;

namespace TestProject
{
    public class MailParseTest
    {
        private readonly Mock<IMailParseBL> _mockMailParseBL;
        private readonly MailParseController _controller;
        public MailParseTest()
        {
            _mockMailParseBL = new Mock<IMailParseBL>();
            _controller = new MailParseController(_mockMailParseBL.Object);
        }

        [Fact]
        public void ParseDatav_ValidData_ReturnsOk()
        {
            // Arrange
            var xmlContent = "<cost_centre>DEV632</cost_centre><total>35000</total><payment_method>card</payment_method>";
            var mailData = new MailParseController.MailData { data = xmlContent };
            var tax = new TaxModel
            {
                TaxRate = 0.10m,
                TotalExcludingTax = 31818.18m,
                TaxAmt = 3181.82m
            };
            _mockMailParseBL.Setup(x => x.CalculateTax(It.IsAny<decimal>()))
            .Returns(tax);

            // Act
            var response = _controller.ParseDatav(mailData);
        
            // Assert
            var json = Assert.IsType<OkObjectResult>(response);
            dynamic jsonValue = json.Value;
            Assert.Equal(35000m, (decimal)jsonValue.total_including_tax);
            Assert.Equal(31818.18m, (decimal)jsonValue.total_excluding_tax);
            Assert.Equal("10.00%", (string)jsonValue.tax_rate);

        }

        [Fact]
        public void ParseDatav_MissingTotal_ReturnsBadRequest()
        {
            // Arrange
            var xmlContent = "<cost_centre>DEV632</cost_centre><payment_method>card</payment_method>";
            var mailData = new MailParseController.MailData { data = xmlContent };

            // Act
            var result = _controller.ParseDatav(mailData);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Missing required <total> field", badRequest.Value.ToString());
        }

        [Fact]
        public void ParseDatav_InvalidXml_ReturnsBadRequest()
        {
            // Arrange
            var xmlContent = "<cost_centre>DEV632"; // malformed XML
            var mailData = new MailParseController.MailData { data = xmlContent };

            // Act
            var result = _controller.ParseDatav(mailData);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Opening tags", badRequest.Value.ToString());
        }

        [Fact]
        public void ParseDatav_InvalidTotalFormat_ReturnsBadRequest()
        {
            // Arrange
            var xmlContent = "<cost_centre>DEV632</cost_centre><total>invalid</total>";
            var mailData = new MailParseController.MailData { data = xmlContent };

            // Act
            var result = _controller.ParseDatav(mailData);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid total amount format", badRequest.Value.ToString());
        }


    }
}