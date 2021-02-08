using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BankingApi.Models
{
    public class Institution
    {
        [Key]
        public int InstitutionId { get; set; }
        public string Name { get; set; }
        public ICollection<Member> Members { get; set; }
    }
}
