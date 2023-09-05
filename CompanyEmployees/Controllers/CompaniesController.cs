using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.DataTransferObjects;
using Entities.ErrorModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private IRepositoryManager _repositoryManager;
        private ILoggerManager _loggerManager;
        private IMapper _mapper;

        public CompaniesController(
            IRepositoryManager repositoryManager,
            ILoggerManager loggerManager,
            IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _loggerManager = loggerManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCompanies()
        {
            var companies = await _repositoryManager.CompanyRepository
                .GetAllCompaniesAsync(trackChanges: false);

            var companiesDTO = _mapper.Map<IEnumerable<CompanyDTO>>(companies);

            return Ok(companiesDTO);
        }

        [HttpGet("collection/({ids})", Name = "CompanyCollection")]
        public async Task<IActionResult> GetCompanyCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
        {
            if(ids == null)
            {
                _loggerManager.LogError("Parameter ids is null");
                return BadRequest("Parameter ids is null");
            }

            var companiesInDb = await _repositoryManager.CompanyRepository
                .GetByIdsAsync(ids, trackChanges: false);

            if (ids.Count() != companiesInDb.Count())
            {
                _loggerManager.LogError("Some IDs sent by client are not valid.");
                return NotFound();
            }

            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDTO>>(companiesInDb);

            return Ok(companiesToReturn);
        }

        [HttpGet("{id}", Name = "CompanyById")]
        public async Task<IActionResult> GetCompany(Guid id)
        {
            var company = await _repositoryManager.CompanyRepository
                .GetCompanyAsync(id, trackChanges: false);

            if (company == null)
            {
                _loggerManager.LogInfo($"Company with ID = {id} doesn't exist in our database");

                return NotFound(new ErrorDetails 
                {
                    StatusCode = (int) HttpStatusCode.NotFound,
                    Message = $"The company with ID = {id} does not found."
                
                });
            }

            var companyDTO = _mapper.Map<CompanyDTO>(company);
            return Ok(companyDTO);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDTO createCompanyDTO)
        {
            var companyToCreate = _mapper.Map<Company>(createCompanyDTO);

            _repositoryManager.CompanyRepository.CreateCompany(companyToCreate);
            
            await _repositoryManager.SaveAsync();

            var companyToReturn = _mapper.Map<CompanyDTO>(companyToCreate);

            return CreatedAtRoute("CompanyById", new { companyToReturn.Id }, companyToReturn);

        }

        [HttpPost("collection")]
        public async Task<IActionResult> CreateCompanyCollection([FromBody]IEnumerable<CreateCompanyDTO> companyCollection)
        {
            if (companyCollection == null)
            {
                _loggerManager.LogError("Company collection sent from client is null.");
                return BadRequest("Company collection is null");
            }

            var companiesToCreate = _mapper.Map<IEnumerable<Company>>(companyCollection);

            foreach (var company in companiesToCreate)
            {
                _repositoryManager.CompanyRepository.CreateCompany(company);
            }
            await _repositoryManager.SaveAsync();

            var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDTO>>(companiesToCreate);

            var ids = string.Join(",", companyCollectionToReturn.Select(c => c.Id));

            return CreatedAtRoute("CompanyCollection", new { ids }, companyCollectionToReturn);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            var companyInDb = await _repositoryManager.CompanyRepository
                .GetCompanyAsync(id, trackChanges: false);

            if (companyInDb == null)
            {
                _loggerManager.LogError($"Company with ID = {id} doesn't exist in our DB.");
                return NotFound();
            }

            _repositoryManager.CompanyRepository.DeleteCompany(companyInDb);
            await _repositoryManager.SaveAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<ActionResult> UpdateCompany(Guid id, [FromBody] CompanyForUpdateDTO company)
        {
            var companyInDb = await _repositoryManager.CompanyRepository
                .GetCompanyAsync(id, trackChanges: true);

            if(companyInDb == null)
            {
                _loggerManager.LogError($"Company with iD {id} doesn't exist in our DB.");
                return NotFound();
            }

            _mapper.Map(company, companyInDb);
            await _repositoryManager.SaveAsync();

            return NoContent();
        }
    }
}
