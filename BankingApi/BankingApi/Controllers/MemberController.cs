using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BankingApi.DataAccess;
using BankingApi.Dto;
using BankingApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankingApi.Controllers
{
    /// <summary>
    /// Controller for Member actions
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IInstitutionRepository _institutionRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IMapper _mapper;

        public MemberController(IInstitutionRepository institutionRepository, 
            IMemberRepository memberRepository, IMapper mapper)
        {
            _institutionRepository = institutionRepository;
            _memberRepository = memberRepository;
            _mapper = mapper;            
        }

        /// <summary>
        /// List all members
        /// </summary>
        /// <returns>200 OK response</returns>
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var members = await _memberRepository.GetMembers();
            return Ok(_mapper.Map<List<MemberDto>>(members));
        }

        /// <summary>
        /// Retrieves an individual member
        /// </summary>
        /// <param name="id">Id of the member to retrieve</param>
        /// <returns>
        /// 404 NotFound response when member not found
        /// 200 OK response otherwise
        /// </returns>
        [HttpGet()]
        [Route("{id:int}")]
        public async Task<ActionResult> GetById(int id)
        {
            var member = await _memberRepository.GetMembers(id);

            if (member?.Count > 0)
            {
                return Ok(_mapper.Map<MemberDto>(member.First()));
            }

            return NotFound();
        }

        /// <summary>
        /// Adds a member
        /// </summary>
        /// <param name="memberDto">Member data transfer object</param>
        /// <returns>
        /// 404 NotFound response when institution not found
        /// 201 Created response when add is success
        /// </returns>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] MemberDto memberDto)
        {
            var institution = await _institutionRepository.GetInstitutions(memberDto.InstitutionId);
            if (institution?.Count == 0)
            {
                ModelState.AddModelError("institutionId", "Institution not found.");
                return NotFound(ModelState);
            }            

            Member request = _mapper.Map<Member>(memberDto);
            var member = await _memberRepository.AddMember(request);
            var response = _mapper.Map<MemberDto>(member);

            return CreatedAtAction(nameof(GetById), new { id = response.MemberId }
                , response);
        }
        
        /// <summary>
        /// Updates a member
        /// </summary>
        /// <param name="id">Id of the member to update</param>
        /// <param name="memberDto">Member data transfer object</param>
        /// <returns>
        /// 204 NoContent for successful update
        /// 404 NotFound response when institution or member not found
        /// </returns>
        [HttpPut]
        [Route("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] MemberDto memberDto)
        {
            var institution = await _institutionRepository.GetInstitutions(memberDto.InstitutionId);
            if (institution?.Count == 0)
            {
                ModelState.AddModelError("institutionId", "Institution not found.");
                return NotFound(ModelState);
            }

            var member = await _memberRepository.GetMembers(id);

            if (member?.Count == 0)
            {
                ModelState.AddModelError("memberId", "Member not found.");
                return NotFound(ModelState);
            }

            Member request = _mapper.Map(memberDto, member.First());
            await _memberRepository.UpdateMember(request);
            return NoContent();
        }

        /// <summary>
        /// Deletes a member
        /// </summary>
        /// <param name="id">Id of the member to delete</param>
        /// <returns>
        /// 204 NoContent for successful delete
        /// 404 NotFound response when member not found
        /// </returns>
        [HttpDelete]
        [Route("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var member = await _memberRepository.GetMembers(id);

            if (member?.Count == 0)
            {
                return NotFound();
            }

            await _memberRepository.DeleteMember(member.First());
            return NoContent();
        }       
    }
}
