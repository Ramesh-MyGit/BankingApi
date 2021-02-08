using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BankingApi.Dto
{
    public class AccountDto
    {
        [JsonProperty("accountId")]
        public int AccountId { get; set; }
        [JsonProperty("balance")]        
        public decimal Balance { get; set; }
    }
}
