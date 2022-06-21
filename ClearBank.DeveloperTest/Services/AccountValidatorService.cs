using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services
{
    public class AccountValidatorService : IAccountValidatorService
    {
        public AccountValidatorService()
        {

        }

        public bool IsAccountValid(Account account)
        {
            if (account == null)
            {
                return false;
            }

            return (account.Status == AccountStatus.Live);
        }

        public bool IsEnoughBalance(decimal balance, decimal amount)
        {
            return (balance > amount );
        }
    }
}
