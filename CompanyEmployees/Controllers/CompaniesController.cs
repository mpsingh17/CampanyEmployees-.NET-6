using AutoMapper;
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
        public IActionResult GetAllCompanies()
        {
            var companies = _repositoryManager.CompanyRepository
                .GetAllCompanies(trackChanges: false);

            var companiesDTO = _mapper.Map<IEnumerable<CompanyDTO>>(companies);

            return Ok(companiesDTO);
        }

        [HttpGet("{id}", Name = "CompanyById")]
        public IActionResult GetCompany(Guid id)
        {
            var company = _repositoryManager.CompanyRepository
                .GetCompany(id, trackChanges: false);

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

        public IActionResult CreateCompany([FromBody] CreateCompanyDTO createCompanyDTO)
        {
            if (createCompanyDTO == null)
            {
                _loggerManager.LogError("The CreateCompanyDTO object sent by client is null");
                return BadRequest("Null sent for CreateCompanyDTO object");
            }

            var companyToCreate = _mapper.Map<Company>(createCompanyDTO);

            _repositoryManager.CompanyRepository.CreateCompany(companyToCreate);
            _repositoryManager.Save();

            var companyToReturn = _mapper.Map<CompanyDTO>(companyToCreate);

            return CreatedAtRoute("CompanyById", new { companyToReturn.Id }, companyToReturn);

        }
    }
}
