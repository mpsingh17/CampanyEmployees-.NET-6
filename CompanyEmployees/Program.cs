using Microsoft.AspNetCore.HttpOverrides;
using NLog;

using CompanyEmployees.Extensions;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.ConfigureSqlConnection(builder.Configuration);

builder.Services.AddControllers();

builder.Services.ConfigureCORS();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureLoggerService();


// Loading NLog configuration.
//LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
LogManager.Setup().LoadConfigurationFromFile(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

var app = builder.Build();

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
