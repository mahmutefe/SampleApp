using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Services
{
    public interface IAccountValidatorService
    {
        bool IsAccountValid(Account account);
        bool IsEnoughBalance(decimal balance, decimal amount);
    }
}