using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services
{
    public interface IPaymentValidatorService
    {
        bool IsPaymentAllowed(Account account, PaymentScheme paymentScheme);
    }
}