  using Microsoft.EntityFrameworkCore;
using SwitchAutomation.Server;
using SwitchAutomation.Server.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<DeviceRepository>();

// Enable CORS for specific origins (like your frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("https://localhost:55849") // your frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
    options.AddPolicy("AllowFrontend",
      policy =>
      {
          policy.WithOrigins("https://localhost:55849")
                .AllowAnyHeader()
                .AllowAnyMethod();
      });
});


var app = builder.Build();

// Use CORS policy
app.UseCors("AllowSpecificOrigin");

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapGet("/", () => Results.Redirect("/swagger/index.html"));
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
