using BankingApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankingApi.DataAccess
{
    public class BankingDbContext: DbContext
    {
        public BankingDbContext(DbContextOptions<BankingDbContext> options)
            : base(options)
        { }

        public DbSet<Institution> Institutions { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Account> Accounts { get; set; }
    }
}
