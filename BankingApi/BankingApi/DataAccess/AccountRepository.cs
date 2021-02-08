using BankingApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankingApi.DataAccess
{    
    public interface IAccountRepository
    {
        Task<Account> GetAccount(int id);
        Task UpdateBalance(Account account);
        Task TransferAmount(Account fromAccount, Account toAccount);
    }

    public class AccountRepository : IAccountRepository
    {
        private readonly BankingDbContext _dbContext;

        public AccountRepository(BankingDbContext context)
        {
            _dbContext = context;
        }

        public async Task<Account> GetAccount(int id)
        {
            IQueryable<Account> account = _dbContext.Accounts.Where(x => x.AccountId == id);
            
            return await account.FirstOrDefaultAsync();
        }

        public async Task UpdateBalance(Account account)
        {
            _dbContext.Accounts.Update(account);
            await _dbContext.SaveChangesAsync();
        }

        public async Task TransferAmount(Account fromAccount, Account toAccount)
        {
            //Save Changes will apply transaction by default. So no explicit transaction needed.
            _dbContext.Accounts.Update(fromAccount);
            _dbContext.Accounts.Update(toAccount);
            await _dbContext.SaveChangesAsync();
        }
    }
}
