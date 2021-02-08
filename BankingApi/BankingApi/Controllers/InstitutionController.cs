using AutoMapper;
using BankingApi.DataAccess;
using BankingApi.Dto;
using BankingApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankingApi.Controllers
{
    /// <summary>
    /// Controller for Institution actions
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class InstitutionController : ControllerBase
    {
        private readonly IInstitutionRepository _institutionRepository;
        private readonly IMapper _mapper;

        public InstitutionController(IInstitutionRepository institutionRepository,
            IMapper mapper)
        {
            _institutionRepository = institutionRepository;
            _mapper = mapper;
        }
        
        /// <summary>
        /// List all institutions
        /// </summary>
        /// <returns>200 OK response</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Get()
        {
            var institutions = await _institutionRepository.GetInstitutions();
            return Ok(_mapper.Map<List<InstitutionDto>>(institutions));
        }

        /// <summary>
        /// Get institution by id
        /// </summary>
        /// <param name="id">Institution Id</param>
        /// <returns>
        /// 404 NotFound response when institution is not found
        /// 200 OK response otherwise
        /// </returns>
        [HttpGet]
        [Route("{id:int}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetById(int id)
        {
            var institution = await _institutionRepository.GetInstitutions(id);

            if (institution?.Count > 0)
            {
                return Ok(_mapper.Map<InstitutionDto>(institution.First()));
            }

            return NotFound();
        }
        
        /// <summary>
        /// Adds an institution
        /// </summary>
        /// <param name="institutionDto">Institution data transfer object</param>
        /// <returns>
        /// 400 BadRequest when institution already exist
        /// 201 Created response for successful add
        /// </returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> Post([FromBody] InstitutionDto institutionDto)
        {
            var institution = await _institutionRepository.GetInstitutionByName(institutionDto.Name);

            if (institution?.Count > 0)
            {
                ModelState.AddModelError("institutionId", "Institution already exists.");
                return BadRequest(ModelState);
            }

            Institution request = _mapper.Map<Institution>(institutionDto);
            institutionDto.InstitutionId = await _institutionRepository.AddInstitution(request);
            
            return CreatedAtAction(nameof(GetById), new { id = institutionDto.InstitutionId }
                , institutionDto);
        }       
    }
}
