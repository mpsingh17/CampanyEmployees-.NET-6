using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DataTransferObjects
{
    public class CreateCompanyDTO
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }

        public IEnumerable<CreateEmployeeDTO>? Employees { get; set; }

    }
}
