using Microsoft.AspNetCore.HttpOverrides;

using CompanyEmployees.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Extend services container to add more services.
builder.Services.ConfigureCORS();
builder.Services.ConfigureIISIntegration();

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
app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapRazorPages();

app.Run();
