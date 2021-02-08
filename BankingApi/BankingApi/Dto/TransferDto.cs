using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BankingApi.Dto
{
    public class TransferDto
    {
        [JsonProperty("fromAccount")]
        [Required]
        public int FromAccount { get; set; }

        [JsonProperty("toAccount")]
        [Required]
        public int ToAccount { get; set; }

        [JsonProperty("amount")]
        [Required]
        public decimal Amount { get; set; }

    }
}
