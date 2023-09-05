using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
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

        [HttpGet("{employeeId}", Name = "GetEmployeeForCompany")]
        public IActionResult GetEmployee(Guid companyId, Guid employeeId)
        {
            var employeeInDb = _repositoryManager.EmployeeRepository
                .GetEmployee(companyId, employeeId, trackChanges: false);

            if (employeeInDb != null)
            {
                var employeeDTO = _mapper.Map<EmployeeDTO>(employeeInDb);
                return Ok(employeeDTO);
            }

            _loggerManager.LogInfo($"The employee with ID = {employeeId} doesn't exist in our database.");
            return NotFound();
        }

        [HttpPost]
        public IActionResult CreateEmployeeForCompany(Guid companyId, [FromBody]EmployeeForCreateDTO createEmployeeDTO)
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
                .GetCompany(companyId, trackChanges: false);

            if (companyInDb == null)
            {
                _loggerManager.LogError($"Company with ID = {companyId} doesn't exist in our DB.");
                return NotFound($"Company with ID {companyId} is not found.");
            }

            var employeeToCreate = _mapper.Map<Employee>(createEmployeeDTO);
            _repositoryManager.EmployeeRepository.CreateEmployeeForCompany(companyId, employeeToCreate);
            _repositoryManager.Save();

            var employeeToReturn = _mapper.Map<EmployeeDTO>(employeeToCreate);
            return CreatedAtRoute(
                "GetEmployeeForCompany",
                new { employeeId = employeeToReturn.Id, companyId },
                employeeToReturn);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            var companyInDb = _repositoryManager.CompanyRepository
                .GetCompany(companyId, trackChanges: false);
            if (companyInDb == null)
            {
                _loggerManager.LogError($"Company with ID = {companyId} doesn't exists in DB.");
                return NotFound("Company not found.");
            }

            var employeeInDb = _repositoryManager.EmployeeRepository
                .GetEmployee(companyId, id, trackChanges: false);
            if (employeeInDb == null)
            {
                _loggerManager.LogError($"Employee with ID = {id} doesn't exists in DB.");
                return NotFound("Employee not found.");
            }

            _repositoryManager.EmployeeRepository.DeleteEmployee(employeeInDb);
            _repositoryManager.Save();

            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEmployeeForCompany(
            Guid companyId, 
            Guid id, 
            [FromBody] EmployeeForUpdateDTO employee)
        {
            if (!ModelState.IsValid)
            {
                _loggerManager.LogError("Invalid model state for the EmployeeForUpdateDto object");
                return UnprocessableEntity(ModelState);
            }
            if (employee == null)
            {
                _loggerManager.LogError($"Client sent a null object to update employee.");
                return BadRequest();
            }

            var companyInDb = _repositoryManager.CompanyRepository
                .GetCompany(companyId, trackChanges: false);
            if (companyInDb == null)
            {
                _loggerManager.LogError($"Company with ID = {companyId} doesn't exist in our DB.");
                return NotFound();
            }

            var employeeInDb = _repositoryManager.EmployeeRepository
                .GetEmployee(companyId, id, trackChanges: true);
            if (employeeInDb == null)
            {
                _loggerManager.LogError($"Employee with ID = {id} doesn't exist in our DB.");
                return NotFound();
            }

            _mapper.Map(employee, employeeInDb);
            _repositoryManager.Save();

            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdateEmployeeForCompany(
            Guid companyId,
            Guid id,
            [FromBody] JsonPatchDocument<EmployeeForUpdateDTO> patchDoc)
        {

            if (patchDoc == null)
            {
                _loggerManager.LogError("patchDoc object sent from client is null.");
                return BadRequest("patchDoc object is null");
            }

            var company = _repositoryManager.CompanyRepository
                .GetCompany(companyId, trackChanges: false);
            if (company == null)
            {
                _loggerManager.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeEntity = _repositoryManager.EmployeeRepository
                .GetEmployee(companyId, id, trackChanges: true);
            if (employeeEntity == null)
            {
                _loggerManager.LogInfo($"Employee with id: {id} doesn't exist in the database.");
                return NotFound();
            }
            var employeeToPatch = _mapper.Map<EmployeeForUpdateDTO>(employeeEntity);

            patchDoc.ApplyTo(employeeToPatch, ModelState);

            TryValidateModel(employeeToPatch);

            if (!ModelState.IsValid)
            {
                _loggerManager.LogError("Invalid model state for the patch document");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(employeeToPatch, employeeEntity);
            _repositoryManager.Save();
            
            return NoContent();
        }
    }
}
