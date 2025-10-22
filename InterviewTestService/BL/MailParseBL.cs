namespace InterviewTestService.BL
{
    public class MailParseBL : IMailParseBL
    {

        public TaxModel CalculateTax(decimal totalIncludingTax)
        {
            const decimal taxRate = 0.10m; // 10%
            var tax = new TaxModel()
            {
                TaxRate = taxRate,
                TotalExcludingTax = totalIncludingTax / (1 + taxRate),
                TaxAmt = totalIncludingTax - (totalIncludingTax / (1 + taxRate))
            };
            return tax;

        }

        public class TaxModel
        {       
            public decimal TotalExcludingTax { get; set; }
            public decimal TaxAmt { get; set; }
            public decimal TaxRate { get; set; } 
        }

    }
}
