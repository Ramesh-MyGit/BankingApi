using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BankingApi.Models
{
    public class Member
    {
        [Key]
        public int MemberId { get; set; }

        public string GivenName { get; set; }

        public string Surname { get; set; }

        public int InstitutionId { get; set; }

        public Institution Institution { get; set; }

        public ICollection<Account> Accounts { get; set; }
    }
}
