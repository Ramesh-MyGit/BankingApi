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
    /// Auto mapper profile to map model to dto objects
    /// </summary>
    public class ModelToDtoProfile : Profile
    {
        public ModelToDtoProfile()
        {
            //Id is set to auto increment in db. So ignoring this in the mapping.
            CreateMap<Institution, InstitutionDto>();
            CreateMap<Member, MemberDto>();
            CreateMap<Account, AccountDto>();
        }
    }
}
