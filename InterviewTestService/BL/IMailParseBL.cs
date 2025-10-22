using static InterviewTestService.BL.MailParseBL;

namespace InterviewTestService.BL
{
    public interface IMailParseBL
    {
        TaxModel CalculateTax(decimal totalIncludingTax);
    }

}
