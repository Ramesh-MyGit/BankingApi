using AutoMapper;
using BankingApi.Dto;
using BankingApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankingApi.MapProfiles
{
    /// <summary>
    /// Auto Mapper profile to map dto to model object
    /// </summary>
    public class DtoToModelProfile : Profile
    {
        public DtoToModelProfile()
        {
            //Id is set to auto increment in db. So ignoring this in the mapping.
            CreateMap<InstitutionDto, Institution>().ForMember(x => x.InstitutionId, opts => opts.Ignore());            
            CreateMap<AccountDto, Account>();
            CreateMap<MemberDto, Member>()
                .ForMember(x => x.MemberId, opts => opts.Ignore())
                .AfterMap((s, d) => { //Map child object foreign key properties from parent
                    foreach (var account in d.Accounts)
                    {
                        account.InstitutionId = d.InstitutionId;                        
                    }                        
                });
        }
    }
}
