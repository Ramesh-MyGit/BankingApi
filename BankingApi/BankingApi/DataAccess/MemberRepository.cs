using BankingApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankingApi.DataAccess
{
    public interface IMemberRepository
    {
        Task<List<Member>> GetMembers(int? id = null);
        Task<Member> AddMember(Member member);
        Task<Member> UpdateMember(Member member);
        Task DeleteMember(Member member);
    }

    public class MemberRepository : IMemberRepository
    {
        private readonly BankingDbContext _dbContext;

        public MemberRepository(BankingDbContext context)
        {
            _dbContext = context;
        }        
       
        public async Task<Member> AddMember(Member member)
        {
            _dbContext.Members.Add(member);            
            await _dbContext.SaveChangesAsync();

            return member;
        }

        public async Task DeleteMember(Member member)
        {
            _dbContext.Members.Remove(member);
            await _dbContext.SaveChangesAsync();            
        }

        public async Task<List<Member>> GetMembers(int? id = null)
        {
            IQueryable<Member> members = _dbContext.Members.Include(account => account.Accounts);

            if (id.HasValue)
            {
                members = members.Where(x => x.MemberId == id.Value);
            }
            return await members.ToListAsync();
        }

        public async Task<Member> UpdateMember(Member member)
        {
            _dbContext.Members.Update(member);
            await _dbContext.SaveChangesAsync();

            return member;
        }        
    }
}
