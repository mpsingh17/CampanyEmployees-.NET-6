using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository;

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
            try
            {
                var companies = _repositoryManager.CompanyRepository
                    .GetAllCompanies(trackChanges: false);

                var companiesDTO = _mapper.Map<IEnumerable<CompanyDTO>>(companies);

                return Ok(companiesDTO);
            }
            catch (Exception ex)
            {
                _loggerManager.LogError($"Something went wrong in {nameof(GetAllCompanies)} action {ex}");

                return StatusCode(500, "Internal server error.");                
            }
        }
    }
}
