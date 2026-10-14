using Microsoft.EntityFrameworkCore;
using PetShopApi.Data;
using PetShopApi.Dtos;
using PetShopApi.Models;

namespace PetShopApi.Endpoints;

public static class PetEndpoints
{
    public static void MapPetEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/pets").WithTags("Pets");

        grupo.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Pets.AsNoTracking()
                .OrderBy(p => p.Nome)
                .Select(p => new PetResponse(p.Id, p.Nome, p.Especie, p.Raca, p.DataNascimento, p.TutorId))
                .ToListAsync(ct)))
            .WithSummary("Lista todos os pets");

        grupo.MapGet("/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
        {
            var pet = await db.Pets.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
            return pet is null ? Results.NotFound() : Results.Ok(PetResponse.De(pet));
        }).WithSummary("Busca um pet pelo id");

        grupo.MapGet("/{id:int}/agendamentos", async (int id, AppDbContext db, CancellationToken ct) =>
        {
            if (!await db.Pets.AnyAsync(p => p.Id == id, ct)) return Results.NotFound();

            var lista = await db.Agendamentos.AsNoTracking()
                .Where(a => a.PetId == id)
                .OrderBy(a => a.DataHora)
                .Select(a => new AgendamentoResponse(a.Id, a.PetId, a.DataHora, a.Servico, a.Valor, a.Status))
                .ToListAsync(ct);
            return Results.Ok(lista);
        }).WithSummary("Lista os agendamentos de um pet");

        grupo.MapPost("/", async (PetRequest req, AppDbContext db, CancellationToken ct) =>
        {
            if (Validacao.Validar(req) is { } erro) return erro;

            if (!await db.Tutores.AnyAsync(t => t.Id == req.TutorId, ct))
                return Results.NotFound(new { erro = $"Tutor {req.TutorId} não encontrado." });

            var pet = new Pet
            {
                Nome = req.Nome, Especie = req.Especie, Raca = req.Raca,
                DataNascimento = req.DataNascimento, TutorId = req.TutorId
            };
            db.Pets.Add(pet);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/pets/{pet.Id}", PetResponse.De(pet));
        }).WithSummary("Cadastra um pet");

        grupo.MapPut("/{id:int}", async (int id, PetRequest req, AppDbContext db, CancellationToken ct) =>
        {
            if (Validacao.Validar(req) is { } erro) return erro;

            var pet = await db.Pets.FindAsync([id], ct);
            if (pet is null) return Results.NotFound();

            if (!await db.Tutores.AnyAsync(t => t.Id == req.TutorId, ct))
                return Results.NotFound(new { erro = $"Tutor {req.TutorId} não encontrado." });

            pet.Nome = req.Nome;
            pet.Especie = req.Especie;
            pet.Raca = req.Raca;
            pet.DataNascimento = req.DataNascimento;
            pet.TutorId = req.TutorId;
            await db.SaveChangesAsync(ct);
            return Results.Ok(PetResponse.De(pet));
        }).WithSummary("Atualiza um pet");

        grupo.MapDelete("/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
        {
            var pet = await db.Pets.FindAsync([id], ct);
            if (pet is null) return Results.NotFound();

            if (await db.Agendamentos.AnyAsync(a => a.PetId == id, ct))
                return Results.Conflict(new { erro = "Não é possível excluir um pet que possui agendamentos." });

            db.Pets.Remove(pet);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithSummary("Remove um pet sem agendamentos");
    }
}
