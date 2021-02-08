using BankingApi.Controllers;
using BankingApi.DataAccess;
using BankingApi.Dto;
using BankingApi.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BankingApi
{
    public class AccountControllerTests
    {
        private readonly AccountController _controller;
        private readonly Mock<IAccountRepository> _mockAccountRepository;

        public AccountControllerTests()
        {
            _mockAccountRepository = new Mock<IAccountRepository>();
            _controller = new AccountController(_mockAccountRepository.Object);
        }

        [Fact]
        public async Task UpdateBalance_UpdateIsSuccessful_WhenRequestIsValid()
        {
            //Setup to return account from GetAccount mock service call
            var account = new Account() {
                AccountId = 1,
                Balance = 10.25m
            };
            _mockAccountRepository.Setup(x => x.GetAccount(1)).
                Returns(Task.FromResult(account));

            //Act
            var actionResult = await _controller.UpdateBalance(1, 15.60m) as NoContentResult;

            //Assert
            _mockAccountRepository.Verify(x => x.UpdateBalance(
               It.Is<Account>(x => x.AccountId == 1
               && x.Balance == 15.60m)), Times.Once);
            Assert.IsType<NoContentResult>(actionResult);
        }

        [Fact]
        public async Task UpdateBalance_UpdateIsSuccessful_WhenBalanceIsZero()
        {
            //Setup to return account from GetAccount mock service call
            var account = new Account()
            {
                AccountId = 1,
                Balance = 10.25m
            };
            _mockAccountRepository.Setup(x => x.GetAccount(1)).
                Returns(Task.FromResult(account));

            //Act
            var actionResult = await _controller.UpdateBalance(1, 0) as NoContentResult;

            //Assert
            _mockAccountRepository.Verify(x => x.UpdateBalance(
               It.Is<Account>(x => x.AccountId == 1
               && x.Balance == 0)), Times.Once);
            Assert.IsType<NoContentResult>(actionResult);
        }

        [Fact]
        public async Task UpdateBalance_ReturnsNotFound_WhenNoAccountIsFound()
        {
            //Setup to return no account from GetAccount mock service call
            _mockAccountRepository.Setup(x => x.GetAccount(1)).
                Returns(Task.FromResult(default(Account)));

            //Act
            var actionResult = await _controller.UpdateBalance(1, 15.60m) as NotFoundResult;

            //Assert. Verify no update balance call is made
            _mockAccountRepository.Verify(x => x.UpdateBalance(
               It.IsAny<Account>()), Times.Never);
            Assert.IsType<NotFoundResult>(actionResult);
        }
        [Fact]
        public async Task UpdateBalance_ReturnsBadRequest_WhenBalanceIsNegative()
        {
            var account = new Account()
            {
                AccountId = 1,
                Balance = 10.25m
            };

            //Setup to return account from GetAccount mock service call
            _mockAccountRepository.Setup(x => x.GetAccount(1)).
                Returns(Task.FromResult(account));

            //Act
            var actionResult = await _controller.UpdateBalance(1, -0.01m) as BadRequestObjectResult;
            var errorResponse = (SerializableError)actionResult.Value;

            //Assert. Verify UpdateBalance is not called
            _mockAccountRepository.Verify(x => x.UpdateBalance(
               It.IsAny<Account>()), Times.Never);
            Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.Single(errorResponse);
            Assert.True(errorResponse.ContainsKey("balance"));
            Assert.Equal("Balance should be greater than or equal to zero", ((string[])errorResponse["balance"])[0]);
        }

        [Fact]
        public async Task TransferAmount_TransferIsSuccessful_WhenRequestIsValid()
        {
            var fromAccount = SetupAccount(1, 10.50m);
            var toAccount = SetupAccount(2, 3.60m);
            var request = SetupTransferDto();

            //Setup to return account from GetAccount mock service call
            _mockAccountRepository.Setup(x => x.GetAccount(1)).
                Returns(Task.FromResult(fromAccount));
            _mockAccountRepository.Setup(x => x.GetAccount(2)).
                Returns(Task.FromResult(toAccount));

            //Act
            var actionResult = await _controller.Transfer(request) as NoContentResult;

            //Assert
            _mockAccountRepository.Verify(x => x.TransferAmount(
               It.Is<Account>(x => x.AccountId == 1
               && x.Balance == 9.3m),
               It.Is<Account>(x => x.AccountId == 2
               && x.Balance == 4.8m)), Times.Once);
            Assert.IsType<NoContentResult>(actionResult);
        }

        [Fact]
        public async Task TransferAmount_ReturnsNotFound_WhenFromAccountNotFound()
        {
            var request = SetupTransferDto();
            var toAccount = SetupAccount(2, 3.60m);

            //Setup to return valid to account and no from account
            _mockAccountRepository.Setup(x => x.GetAccount(1)).
                Returns(Task.FromResult(default(Account)));
            _mockAccountRepository.Setup(x => x.GetAccount(2)).
                Returns(Task.FromResult(toAccount));

            //Act
            var actionResult = await _controller.Transfer(request) as NotFoundResult;

            //Assert. Verify no transfer call when validation fails
            _mockAccountRepository.Verify(x => x.TransferAmount(
               It.IsAny<Account>(), It.IsAny<Account>()), Times.Never);
            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task TransferAmount_ReturnsNotFound_WhenToAccountNotFound()
        {
            var request = SetupTransferDto();
            var fromAccount = SetupAccount(1, 10.50m);

            //Setup to return valid from account and no to account
            _mockAccountRepository.Setup(x => x.GetAccount(1)).
                Returns(Task.FromResult(fromAccount));
            _mockAccountRepository.Setup(x => x.GetAccount(2)).
                Returns(Task.FromResult(default(Account)));

            //Act
            var actionResult = await _controller.Transfer(request) as NotFoundResult;

            //Assert. Verify no transfer call when validation fails
            _mockAccountRepository.Verify(x => x.TransferAmount(
               It.IsAny<Account>(), It.IsAny<Account>()), Times.Never);
            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task TransferAmount_ReturnsUnprocessableEntiry_WhenAccountsInstitutionIsNotSame()
        {
            var fromAccount = SetupAccount(1, 10.50m);
            var toAccount = SetupAccount(2, 3.60m);
            toAccount.InstitutionId = 2;

            var request = SetupTransferDto();

            //Setup to return account from GetAccount mock service call
            _mockAccountRepository.Setup(x => x.GetAccount(1)).
                Returns(Task.FromResult(fromAccount));
            _mockAccountRepository.Setup(x => x.GetAccount(2)).
                Returns(Task.FromResult(toAccount));

            //Act
            var actionResult = await _controller.Transfer(request) as UnprocessableEntityObjectResult;
            var errorResponse = (SerializableError)actionResult.Value;

            //Assert. Verify TransferAmount is not called
            _mockAccountRepository.Verify(x => x.TransferAmount(
               It.IsAny<Account>(), It.IsAny<Account>()), Times.Never);
            Assert.IsType<UnprocessableEntityObjectResult>(actionResult);
            Assert.True(errorResponse.ContainsKey("institutionId"));
            Assert.Equal("Transfer is only allowed for accounts within the institution", ((string[])errorResponse["institutionId"])[0]);
        }

        [Fact]
        public async Task TransferAmount_ReturnsUnprocessableEntiry_WhenAmountIsZero()
        {
            var fromAccount = SetupAccount(1, 10.50m);
            var toAccount = SetupAccount(2, 3.60m);
            var request = SetupTransferDto();
            request.Amount = 0;

            //Setup to return account from GetAccount mock service call
            _mockAccountRepository.Setup(x => x.GetAccount(1)).
                Returns(Task.FromResult(fromAccount));
            _mockAccountRepository.Setup(x => x.GetAccount(2)).
                Returns(Task.FromResult(toAccount));

            //Act
            var actionResult = await _controller.Transfer(request) as UnprocessableEntityObjectResult;
            var errorResponse = (SerializableError)actionResult.Value;

            //Assert. Verify TransferAmount is not called
            _mockAccountRepository.Verify(x => x.TransferAmount(
               It.IsAny<Account>(), It.IsAny<Account>()), Times.Never);
            Assert.IsType<UnprocessableEntityObjectResult>(actionResult);
            Assert.True(errorResponse.ContainsKey("amount"));
            Assert.Equal("Amount should be greater than zero", ((string[])errorResponse["amount"])[0]);
        }

        [Fact]
        public async Task TransferAmount_ReturnsUnprocessableEntiry_WhenAmountIsNegative()
        {
            var fromAccount = SetupAccount(1, 10.50m);
            var toAccount = SetupAccount(2, 3.60m);
            var request = SetupTransferDto();
            request.Amount = -0.01m;

            //Setup to return account from GetAccount mock service call
            _mockAccountRepository.Setup(x => x.GetAccount(1)).
                Returns(Task.FromResult(fromAccount));
            _mockAccountRepository.Setup(x => x.GetAccount(2)).
                Returns(Task.FromResult(toAccount));

            //Act
            var actionResult = await _controller.Transfer(request) as UnprocessableEntityObjectResult;
            var errorResponse = (SerializableError)actionResult.Value;

            //Assert. Verify TransferAmount is not called
            _mockAccountRepository.Verify(x => x.TransferAmount(
               It.IsAny<Account>(), It.IsAny<Account>()), Times.Never);
            Assert.IsType<UnprocessableEntityObjectResult>(actionResult);
            Assert.True(errorResponse.ContainsKey("amount"));
            Assert.Equal("Amount should be greater than zero", ((string[])errorResponse["amount"])[0]);
        }

        [Fact]
        public async Task TransferAmount_ReturnsUnprocessableEntiry_WhenFromAccountHasInSufficientBalance()
        {
            var fromAccount = SetupAccount(1, 10.50m);
            var toAccount = SetupAccount(2, 3.60m);
            var request = SetupTransferDto();
            request.Amount = 10.51m;

            //Setup to return account from GetAccount mock service call
            _mockAccountRepository.Setup(x => x.GetAccount(1)).
                Returns(Task.FromResult(fromAccount));
            _mockAccountRepository.Setup(x => x.GetAccount(2)).
                Returns(Task.FromResult(toAccount));

            //Act
            var actionResult = await _controller.Transfer(request) as UnprocessableEntityObjectResult;
            var errorResponse = (SerializableError)actionResult.Value;

            //Assert. Verify TransferAmount is not called
            _mockAccountRepository.Verify(x => x.TransferAmount(
               It.IsAny<Account>(), It.IsAny<Account>()), Times.Never);
            Assert.IsType<UnprocessableEntityObjectResult>(actionResult);
            Assert.True(errorResponse.ContainsKey("amount"));
            Assert.Equal("Insufficient funds to complete transaction", ((string[])errorResponse["amount"])[0]);
        }

        private Account SetupAccount(int accountId, decimal balance)
        {
            return new Account
            {
                AccountId = accountId,
                Balance = balance,
                InstitutionId = 1
            };
        }

        private TransferDto SetupTransferDto()
        {
            return new TransferDto
            {
                FromAccount = 1,
                ToAccount = 2,
                Amount = 1.2m
            };
        }

    }
}
