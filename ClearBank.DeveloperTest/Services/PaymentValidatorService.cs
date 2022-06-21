using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services
{
    public class PaymentValidatorService : IPaymentValidatorService
    {
        public PaymentValidatorService()
        {
        }

        public bool IsPaymentAllowed(Account account, PaymentScheme paymentScheme)
        {
            return account.AllowedPaymentSchemes.HasFlag(paymentScheme);
        }
    }
}
