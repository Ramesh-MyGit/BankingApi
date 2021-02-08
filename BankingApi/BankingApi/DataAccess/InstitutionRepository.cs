using BankingApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankingApi.DataAccess
{
    public interface IInstitutionRepository
    {
        Task<List<Institution>> GetInstitutions(int? id = null);
        Task<List<Institution>> GetInstitutionByName(string name);
        Task<int> AddInstitution(Institution institution);
    }

    public class InstitutionRepository : IInstitutionRepository
    {
        private readonly BankingDbContext _dbContext;

        public InstitutionRepository(BankingDbContext context)
        {
            _dbContext = context;
        }

        public async Task<List<Institution>> GetInstitutions(int? id = null)
        {
            IQueryable<Institution> institutions = _dbContext.Institutions;

            if (id.HasValue)
            {
                institutions = institutions.Where(x => x.InstitutionId == id.Value);
            }
            return await institutions.ToListAsync();
        }

        public async Task<List<Institution>> GetInstitutionByName(string name)
        {
            IQueryable<Institution> institutions = _dbContext.Institutions;
            institutions = institutions.Where(x => x.Name == name.Trim());
            
            return await institutions.ToListAsync();
        }

        public async Task<int> AddInstitution(Institution institution)
        {
            _dbContext.Institutions.Add(institution);
            await _dbContext.SaveChangesAsync();

            return institution.InstitutionId;
        }
    }
}
