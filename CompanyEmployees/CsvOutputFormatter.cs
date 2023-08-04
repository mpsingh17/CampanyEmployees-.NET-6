using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace CompanyEmployees
{
    public class CsvOutputFormatter : TextOutputFormatter
    {
        public CsvOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type? type)
        {
            if (typeof(CompanyDTO).IsAssignableFrom(type) ||
                typeof(IEnumerable<CompanyDTO>).IsAssignableFrom(type))
            {
                return base.CanWriteType(type);
            }
            return false;
        }

        public override async Task WriteResponseBodyAsync(
            OutputFormatterWriteContext context,
            Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            var buffer = new StringBuilder();

            if (context.Object is IEnumerable<CompanyDTO>)
            {
                foreach(var company in (IEnumerable<CompanyDTO>)context.Object)
                {
                    FormatCSV(buffer, company);
                }
            }
            else
            {
                FormatCSV(buffer, (CompanyDTO)context.Object);
            }

            await response.WriteAsJsonAsync(buffer.ToString());
        }

        private static void FormatCSV(StringBuilder buffer, CompanyDTO company)
        {
            buffer.AppendLine($"{company.Id}, {company.Name}, {company.FullAddress}");
        }
    }
}
