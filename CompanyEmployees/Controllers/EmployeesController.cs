using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _loggerManager;
        private readonly IMapper _mapper;

        public EmployeesController(
            IRepositoryManager repositoryManager,
            ILoggerManager loggerManager,
            IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _loggerManager = loggerManager;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetEmployees(Guid companyId)
        {
            var employeesInDb = _repositoryManager.EmployeeRepository
                .GetEmployees(companyId, trackChanges: false);

            if (employeesInDb.Any())
            {
                var employeesDTO = _mapper.Map<IEnumerable<EmployeeDTO>>(employeesInDb);

                return Ok(employeesDTO);
            }

            _loggerManager.LogInfo($"The company with ID = {companyId} does not exist in DB");
            return NotFound();
        }
    }
}
