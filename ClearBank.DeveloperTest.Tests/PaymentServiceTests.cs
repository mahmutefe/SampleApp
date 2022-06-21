using AutoFixture.Xunit2;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Models;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Types;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ClearBank.DeveloperTest.Tests
{
    public class PaymentServiceTests
    {
        private readonly Mock<IAccountValidatorService> accountValidatorServiceMock = new Mock<IAccountValidatorService>();
        private readonly Mock<IPaymentValidatorService> paymentValidatorServiceMock = new Mock<IPaymentValidatorService>();
        private readonly Mock<IBackupAccountDataStore> backupAccountDataStoreMock = new Mock<IBackupAccountDataStore>();
        private readonly Mock<IAccountDataStore> accountDataStoreMock = new Mock<IAccountDataStore>();
        private readonly IOptions<PaymentConfig> optionsMock;
        PaymentService paymentServiceMock;

        public PaymentServiceTests()
        {
            optionsMock = Options.Create(new PaymentConfig()
            {
                DataStoreType = "Backup",
            });

            paymentServiceMock = new PaymentService(
                optionsMock,
                accountValidatorServiceMock.Object,
                paymentValidatorServiceMock.Object,
                backupAccountDataStoreMock.Object,
                accountDataStoreMock.Object
                );
        }

        [Theory]
        [AutoData]
        public void MakePayment_GiveNullAccount_ShouldReturnFalse(MakePaymentRequest request)
        {
            // Arrange

            this.backupAccountDataStoreMock
                .Setup(x => x.GetAccount(request.DebtorAccountNumber))
                .Returns((Account)null)
                .Verifiable();

            this.accountValidatorServiceMock
                .Setup(x => x.IsAccountValid(null))
                .Returns(false)
                .Verifiable();

            // Act
            var response = this.paymentServiceMock.MakePayment(request);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().Be(false);
            backupAccountDataStoreMock.Verify(x => x.GetAccount(It.IsAny<string>()), Times.Once);
            accountValidatorServiceMock.Verify();
        }

        [Theory]
        [AutoData]
        public void MakePayment_GivenDisabledAccount_ShouldReturnFalse(MakePaymentRequest request, Account account)
        {
            // Arrange
            account.Status = AccountStatus.Disabled;

            this.backupAccountDataStoreMock
                .Setup(x => x.GetAccount(request.DebtorAccountNumber))
                .Returns(account)
                .Verifiable();

            this.accountValidatorServiceMock
                .Setup(x => x.IsAccountValid(account))
                .Returns(false)
                .Verifiable();

            // Act
            var response = this.paymentServiceMock.MakePayment(request);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().Be(false);
            backupAccountDataStoreMock.Verify(x => x.GetAccount(It.IsAny<string>()), Times.Once);
            accountValidatorServiceMock.Verify();
        }

        [Theory]
        [AutoData]
        public void MakePayment_GivenInsufficientBalanceAccount_ShouldReturnFalse(MakePaymentRequest request, Account account)
        {
            // Arrange
            request.Amount = 11;
            account.Balance = 10;
            account.Status = AccountStatus.Live;

            this.backupAccountDataStoreMock
                .Setup(x => x.GetAccount(request.DebtorAccountNumber))
                .Returns(account)
                .Verifiable();

            this.accountValidatorServiceMock
                .Setup(x => x.IsAccountValid(account))
                .Returns(true)
                .Verifiable();

            this.accountValidatorServiceMock
                .Setup(x => x.IsEnoughBalance(account.Balance, request.Amount))
                .Returns(false)
                .Verifiable();

            // Act
            var response = this.paymentServiceMock.MakePayment(request);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().Be(false);
            backupAccountDataStoreMock.Verify(x => x.GetAccount(It.IsAny<string>()), Times.Once);
            accountValidatorServiceMock.VerifyAll();
        }

        [Theory]
        [AutoData]
        public void MakePayment_GivenNotAllowedPayment_ShouldReturnFalse(MakePaymentRequest request, Account account)
        {
            // Arrange
            request.Amount = 9;
            request.PaymentScheme = PaymentScheme.Chaps;
            account.Balance = 10;
            account.Status = AccountStatus.Live;
            account.AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments;

            this.backupAccountDataStoreMock
                .Setup(x => x.GetAccount(request.DebtorAccountNumber))
                .Returns(account)
                .Verifiable();

            this.accountValidatorServiceMock
                .Setup(x => x.IsAccountValid(account))
                .Returns(true)
                .Verifiable();

            this.accountValidatorServiceMock
                .Setup(x => x.IsEnoughBalance(account.Balance, request.Amount))
                .Returns(true)
                .Verifiable();

            this.paymentValidatorServiceMock
                .Setup(x => x.IsPaymentAllowed(account, request.PaymentScheme))
                .Returns(false)
                .Verifiable();

            // Act
            var response = this.paymentServiceMock.MakePayment(request);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().Be(false);
            backupAccountDataStoreMock.Verify(x => x.GetAccount(It.IsAny<string>()), Times.Once);
            accountValidatorServiceMock.VerifyAll();
            paymentValidatorServiceMock.Verify();
        }

        [Theory]
        [AutoData]
        public void MakePayment_GivenValidAccountAndAllowedPayment_ShouldReturnTrue(MakePaymentRequest request, Account account)
        {
            // Arrange
            request.Amount = 9;
            request.PaymentScheme = PaymentScheme.Chaps;
            account.Balance = 10;
            account.Status = AccountStatus.Live;
            account.AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps;

            this.backupAccountDataStoreMock
                .Setup(x => x.GetAccount(request.DebtorAccountNumber))
                .Returns(account)
                .Verifiable();

            this.accountValidatorServiceMock
                .Setup(x => x.IsAccountValid(account))
                .Returns(true)
                .Verifiable();

            this.accountValidatorServiceMock
                .Setup(x => x.IsEnoughBalance(account.Balance, request.Amount))
                .Returns(true)
                .Verifiable();

            this.paymentValidatorServiceMock
                .Setup(x => x.IsPaymentAllowed(account, request.PaymentScheme))
                .Returns(true)
                .Verifiable();

            // Act
            var response = this.paymentServiceMock.MakePayment(request);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().Be(true);
            backupAccountDataStoreMock.Verify(x => x.GetAccount(It.IsAny<string>()), Times.Once);
            accountValidatorServiceMock.VerifyAll();
            paymentValidatorServiceMock.Verify();
        }
    }
}
