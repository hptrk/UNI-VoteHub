using Voter.DataAccess;
using Voter.WebAPI.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddWebApi();

string appDataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(appDataDirectory);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler(_ => { });

if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize database on startup
try
{
    // RESET DATABASE
    //await DbResetter.ResetDatabase(app.Services);
    await DbInitializer.SeedDatabase(app.Services);

    Console.WriteLine("Database initialization completed successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"Error during database initialization: {ex.Message}");
}

app.Run();

// Make the Program class public for integration testing
public partial class Program { }
