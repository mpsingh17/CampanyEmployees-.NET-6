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

        public CompaniesController(
            IRepositoryManager repositoryManager,
            ILoggerManager loggerManager)
        {
            _repositoryManager = repositoryManager;
            _loggerManager = loggerManager;
        }

        [HttpGet]
        public IActionResult GetAllCompanies()
        {
            try
            {
                var companies = _repositoryManager.CompanyRepository
                    .GetAllCompanies(trackChanges: false);

                var companiesDTO = companies.Select(c => new CompanyDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    FullAddress = string.Join(' ', c.Address, c.Country)
                }).ToList();

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
