using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BankingApi.Models
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; }
        public decimal Balance { get; set; }
        public int InstitutionId { get; set; }
        public Institution Institution { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
    }
}
