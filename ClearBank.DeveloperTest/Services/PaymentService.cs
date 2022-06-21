using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Models;
using ClearBank.DeveloperTest.Types;
using Microsoft.Extensions.Options;
using System.Configuration;

namespace ClearBank.DeveloperTest.Services
{
    public class PaymentService : IPaymentService
    {
        private const string STORAGE_BACKUP = "Backup";

        private readonly IOptions<PaymentConfig> options;
        private readonly IAccountValidatorService accountValidatorService;
        private readonly IPaymentValidatorService paymentValidatorService;
        private readonly IBackupAccountDataStore backupAccountDataStore;
        private readonly IAccountDataStore accountDataStore;

        public PaymentService(
            IOptions<PaymentConfig> options,
            IAccountValidatorService accountValidatorService,
            IPaymentValidatorService paymentValidatorService,
            IBackupAccountDataStore backupAccountDataStore,
            IAccountDataStore accountDataStore)
        {
            this.options = options;
            this.accountValidatorService = accountValidatorService;
            this.paymentValidatorService = paymentValidatorService;
            this.backupAccountDataStore = backupAccountDataStore;
            this.accountDataStore = accountDataStore;
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            var dataStoreType = options.Value.DataStoreType;

            IBaseDataStore dataStore = null;
            if (dataStoreType == STORAGE_BACKUP)
            {
                dataStore = backupAccountDataStore;
            }
            else
            {
                dataStore = accountDataStore;
            }

            var account = dataStore.GetAccount(request.DebtorAccountNumber);

            if (accountValidatorService.IsAccountValid(account) == false ||
                accountValidatorService.IsEnoughBalance(account.Balance, request.Amount) == false ||
                paymentValidatorService.IsPaymentAllowed(account, request.PaymentScheme) == false)
            {
                return new MakePaymentResult()
                { 
                    Success = false,
                };
            }

            account.Balance -= request.Amount;
            dataStore.UpdateAccount(account);

            return new MakePaymentResult()
            { 
                Success = true,
            };
        }
    }
}
