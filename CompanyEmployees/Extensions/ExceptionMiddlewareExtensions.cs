using Contracts;
using Entities.ErrorModels;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace CompanyEmployees.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(
            this IApplicationBuilder app)
        {
            var loggerManager = app.ApplicationServices.GetService<ILoggerManager>();

            app.UseExceptionHandler(appError =>
            {
                appError.Run(async httpContext => 
                { 
                    httpContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                    httpContext.Response.ContentType = "application/json";

                    var contextFeature = httpContext.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        if (loggerManager != null)
                            loggerManager.LogError($"Something went wrong {contextFeature.Error}");
                        else
                            Console.WriteLine("Logger manager is null");


                        await httpContext.Response.WriteAsync(new ErrorDetails
                        {
                            Message = "Internal server error",
                            StatusCode = httpContext.Response.StatusCode
                        }.ToString());
                    }

                });
            });
        }
    }
}
