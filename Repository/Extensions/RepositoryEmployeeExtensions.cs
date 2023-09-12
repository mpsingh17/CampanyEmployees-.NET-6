using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Extensions
{
    public static class RepositoryEmployeeExtensions
    {
        public static IQueryable<Employee> FilterEmployees(
            this IQueryable<Employee> employees,
            uint minAge,
            uint maxAge)
        {
            return employees.Where(emp => emp.Age >= minAge && emp.Age <= maxAge);
        }

        public static IQueryable<Employee> Search(
            this IQueryable<Employee> employees, 
            string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return employees;
            }

            var sanitisedSearchTerm = searchTerm.Trim().ToLower();

            return employees.Where(emp => emp.Name.ToLower().Contains(sanitisedSearchTerm));
        }
    }
}
