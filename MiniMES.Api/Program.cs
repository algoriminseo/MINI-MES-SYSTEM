using Microsoft.EntityFrameworkCore;
using MiniMES.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<MiniMesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MiniMesDbContext>();
    await DatabaseInitializer.EnsureCreatedAsync(dbContext);
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
