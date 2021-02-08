using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BankingApi.Dto
{
    public class InstitutionDto
    {
        [JsonProperty("institutionId")]
        [Required]
        public int InstitutionId { get; set; }
        [JsonProperty("name")]
        [Required]
        public string Name { get; set; }
    }
}
