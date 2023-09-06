using Microsoft.AspNetCore.HttpOverrides;
using NLog;
using CompanyEmployees.Extensions;
using Contracts;
using LoggerService;
using Microsoft.AspNetCore.Mvc;
using CompanyEmployees.ActionFilters;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        builder.Services.ConfigureSqlConnection(builder.Configuration);
        builder.Services.ConfigureRepositoryManager();

        builder.Services.AddControllers(config =>
        {
            config.RespectBrowserAcceptHeader = true;
            config.ReturnHttpNotAcceptable = true;

        })
        .AddNewtonsoftJson()
        .AddXmlDataContractSerializerFormatters()
        .AddCustomCSVFormater();

        builder.Services.ConfigureCORS();
        builder.Services.ConfigureIISIntegration();
        builder.Services.ConfigureLoggerService();
        builder.Services.AddAutoMapper(typeof(Program));

        // Register action filters.
        builder.Services.AddScoped<ValidationFilterAttribute>();
        builder.Services.AddScoped<ValidateCompanyExistsAttribute>();
        builder.Services.AddScoped<ValidateEmployeeForCompanyExistsAttribute>();

        // Loading NLog configuration.
        //LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
        LogManager.Setup().LoadConfigurationFromFile(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

        var app = builder.Build();

        //var logger = app.Services.GetService<LoggerManager>();
        app.ConfigureExceptionHandler();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.All
        }); ;

        app.UseRouting();

        // Important! this should be called between UseRounting() and UseAuthorization() method.
        app.UseCors("CorsPolicy");

        app.UseAuthorization();

        app.MapRazorPages();


        app.UseEndpoints(endpoints => endpoints.MapControllers());

        app.Run();
    }
}