using AutoMapper;
using CompanyEmployees.ActionFilters;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
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
        public async Task<IActionResult> GetEmployeesForCompany(
            Guid companyId,
            [FromQuery] EmployeeParameters employeeParameters)
        {
            var employeesInDb = await _repositoryManager.EmployeeRepository
                .GetEmployeesAsync(companyId, employeeParameters, trackChanges: false);

            if (employeesInDb.Any())
            {
                var employeesDTO = _mapper.Map<IEnumerable<EmployeeDTO>>(employeesInDb);

                return Ok(employeesDTO);
            }

            _loggerManager.LogInfo($"The company with ID = {companyId} does not exist in DB");
            return NotFound();
        }

        [HttpGet("{employeeId}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployee(Guid companyId, Guid employeeId)
        {
            var employeeInDb = await _repositoryManager.EmployeeRepository
                .GetEmployeeAsync(companyId, employeeId, trackChanges: false);

            if (employeeInDb != null)
            {
                var employeeDTO = _mapper.Map<EmployeeDTO>(employeeInDb);
                return Ok(employeeDTO);
            }

            _loggerManager.LogInfo($"The employee with ID = {employeeId} doesn't exist in our database.");
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody]EmployeeForCreateDTO createEmployeeDTO)
        {
            if (!ModelState.IsValid)
            {
                _loggerManager.LogError("Invalid model state for the EmployeeForCreationDto object");
                
                // This line will add additional error message to Age property.
                ModelState.AddModelError(nameof(EmployeeForCreateDTO.Age), "Age field is required.");

                return UnprocessableEntity(ModelState);
            }

            if (createEmployeeDTO == null)
            {
                _loggerManager.LogError("Client sent a null CreateEmployeeDTO object.");
                return BadRequest("CreateEmployeeDTO object is null.");
            }

            var companyInDb = _repositoryManager.CompanyRepository
                .GetCompanyAsync(companyId, trackChanges: false);

            if (companyInDb == null)
            {
                _loggerManager.LogError($"Company with ID = {companyId} doesn't exist in our DB.");
                return NotFound($"Company with ID {companyId} is not found.");
            }

            var employeeToCreate = _mapper.Map<Employee>(createEmployeeDTO);
            _repositoryManager.EmployeeRepository.CreateEmployeeForCompany(companyId, employeeToCreate);
            await _repositoryManager.SaveAsync();

            var employeeToReturn = _mapper.Map<EmployeeDTO>(employeeToCreate);
            return CreatedAtRoute(
                "GetEmployeeForCompany",
                new { employeeId = employeeToReturn.Id, companyId },
                employeeToReturn);
        }

        [HttpDelete("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            var employeeInDb = HttpContext.Items["employee"] as Employee;

            _repositoryManager.EmployeeRepository.DeleteEmployee(employeeInDb);
            await _repositoryManager.SaveAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(
            Guid companyId, 
            Guid id, 
            [FromBody] EmployeeForUpdateDTO employee)
        {
            if (!ModelState.IsValid)
            {
                _loggerManager.LogError("Invalid model state for the EmployeeForUpdateDto object");
                return UnprocessableEntity(ModelState);
            }            var employeeInDb = HttpContext.Items["employee"] as Employee;
            _mapper.Map(employee, employeeInDb);
            await _repositoryManager.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(
            Guid companyId,
            Guid id,
            [FromBody] JsonPatchDocument<EmployeeForUpdateDTO> patchDoc)
        {

            if (patchDoc == null)
            {
                _loggerManager.LogError("patchDoc object sent from client is null.");
                return BadRequest("patchDoc object is null");
            }

            var employeeEntity = HttpContext.Items["employee"] as Employee;

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDTO>(employeeEntity);

            patchDoc.ApplyTo(employeeToPatch, ModelState);

            TryValidateModel(employeeToPatch);

            if (!ModelState.IsValid)
            {
                _loggerManager.LogError("Invalid model state for the patch document");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(employeeToPatch, employeeEntity);
            await _repositoryManager.SaveAsync();
            
            return NoContent();
        }
    }
}
