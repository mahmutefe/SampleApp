using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Data
{
    public interface IBaseDataStore
    {
        Account GetAccount(string accountNumber);

        void UpdateAccount(Account account);
    }
}