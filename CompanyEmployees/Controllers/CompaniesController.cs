using Contracts;
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

                return Ok(companies);
            }
            catch (Exception)
            {
                _loggerManager.LogError($"Something went wrong in {nameof(GetAllCompanies)} action {ex}");

                return StatusCode(500, "Internal server error.");                
            }
        }
    }
}
