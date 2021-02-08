using System.Threading.Tasks;
using BankingApi.DataAccess;
using BankingApi.Dto;
using Microsoft.AspNetCore.Mvc;

namespace BankingApi.Controllers
{
    /// <summary>
    /// Controller for account actions
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;        

        public AccountController(IAccountRepository accountRepository)
        {            
            _accountRepository = accountRepository;
        }

        /// <summary>
        /// Update account balance
        /// </summary>
        /// <param name="id">Id of the account</param>
        /// <param name="balance">Balance to update</param>
        /// <returns>NotFound reponse when account is not found, NoContent response to indicate successful transaction</returns>
        [HttpPut]
        [Route("{id:int}")]
        public async Task<ActionResult> UpdateBalance(int id, [FromBody] decimal balance)
        {
            var account = await _accountRepository.GetAccount(id);

            if (account == null)
            {
                return NotFound();
            }
            else if (balance < 0)
            {
                ModelState.AddModelError("balance", "Balance should be greater than or equal to zero");
                return BadRequest(ModelState);
            }

            account.Balance = balance;
            await _accountRepository.UpdateBalance(account);
            return NoContent();
        }

        /// <summary>
        /// Transfer amount between accounts within an institution
        /// </summary>
        /// <param name="transferDto">Data transfer object for transfer request</param>
        /// <returns>
        /// NotFound reponse when account is not found. 
        /// NoContent response to indicate successful transaction.
        /// UnprocessableEntity response for validation errors.
        /// </returns>
        [HttpPut]
        [Route("Transfer")]
        public async Task<ActionResult> Transfer([FromBody] TransferDto transferDto)
        {
            var fromAccount = await _accountRepository.GetAccount(transferDto.FromAccount);
            var toAccount = await _accountRepository.GetAccount(transferDto.ToAccount);

            if (fromAccount == null || toAccount == null)
            {
                return NotFound();
            }

            if (fromAccount.InstitutionId != toAccount.InstitutionId)
            {
                ModelState.AddModelError("institutionId", "Transfer is only allowed for accounts within the institution");
                return UnprocessableEntity(ModelState);
            }
            else if (transferDto.Amount <= 0)
            {
                ModelState.AddModelError("amount", "Amount should be greater than zero");
                return UnprocessableEntity(ModelState);
            }
            else if ((fromAccount.Balance - transferDto.Amount) < 0)
            {
                ModelState.AddModelError("amount", "Insufficient funds to complete transaction");
                return UnprocessableEntity(ModelState);
            }

            fromAccount.Balance -= transferDto.Amount;
            toAccount.Balance += transferDto.Amount;
            await _accountRepository.TransferAmount(fromAccount, toAccount);
            return NoContent();
        }        
    }
}