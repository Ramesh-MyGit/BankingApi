using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BankingApi.Dto
{
    public class MemberDto
    {
        [JsonProperty("memberId")]
        [Required]
        public int MemberId { get; set; }

        [JsonProperty("givenName")]
        [Required]
        public string GivenName { get; set; }

        [JsonProperty("surname")]
        [Required]
        public string Surname { get; set; }

        [JsonProperty("institutionId")]
        [Required]
        public int InstitutionId { get; set; }

        [JsonProperty("accounts")]
        [Required]
        public IEnumerable<AccountDto> Accounts { get; set; }
    }
}
