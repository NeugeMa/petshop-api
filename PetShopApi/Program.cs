using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PetShopApi.Data;
using PetShopApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Enums trafegam como texto (ex.: "Banho") em vez de números.
builder.Services.ConfigureHttpJsonOptions(opt =>
    opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Aplica as migrations pendentes ao iniciar, para a API rodar sem passos extras.
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi();
app.UseSwaggerUI(opt => opt.SwaggerEndpoint("/openapi/v1.json", "PetShop API v1"));
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapTutorEndpoints();
app.MapPetEndpoints();
app.MapAgendamentoEndpoints();

await app.RunAsync();
