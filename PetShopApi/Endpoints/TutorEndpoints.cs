using Microsoft.EntityFrameworkCore;
using PetShopApi.Data;
using PetShopApi.Dtos;
using PetShopApi.Models;

namespace PetShopApi.Endpoints;

public static class TutorEndpoints
{
    public static void MapTutorEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/tutores").WithTags("Tutores");

        grupo.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Tutores.AsNoTracking()
                .OrderBy(t => t.Nome)
                .Select(t => new TutorResponse(t.Id, t.Nome, t.Email, t.Telefone))
                .ToListAsync(ct)))
            .WithSummary("Lista todos os tutores");

        grupo.MapGet("/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
        {
            var tutor = await db.Tutores.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);
            return tutor is null ? Results.NotFound() : Results.Ok(TutorResponse.De(tutor));
        }).WithSummary("Busca um tutor pelo id");

        grupo.MapGet("/{id:int}/pets", async (int id, AppDbContext db, CancellationToken ct) =>
        {
            if (!await db.Tutores.AnyAsync(t => t.Id == id, ct)) return Results.NotFound();

            var pets = await db.Pets.AsNoTracking()
                .Where(p => p.TutorId == id)
                .OrderBy(p => p.Nome)
                .Select(p => new PetResponse(p.Id, p.Nome, p.Especie, p.Raca, p.DataNascimento, p.TutorId))
                .ToListAsync(ct);
            return Results.Ok(pets);
        }).WithSummary("Lista os pets de um tutor");

        grupo.MapPost("/", async (TutorRequest req, AppDbContext db, CancellationToken ct) =>
        {
            if (Validacao.Validar(req) is { } erro) return erro;

            if (await db.Tutores.AnyAsync(t => t.Email == req.Email, ct))
                return Results.Conflict(new { erro = "Já existe um tutor com este e-mail." });

            var tutor = new Tutor { Nome = req.Nome, Email = req.Email, Telefone = req.Telefone };
            db.Tutores.Add(tutor);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/tutores/{tutor.Id}", TutorResponse.De(tutor));
        }).WithSummary("Cadastra um tutor");

        grupo.MapPut("/{id:int}", async (int id, TutorRequest req, AppDbContext db, CancellationToken ct) =>
        {
            if (Validacao.Validar(req) is { } erro) return erro;

            var tutor = await db.Tutores.FindAsync([id], ct);
            if (tutor is null) return Results.NotFound();

            if (await db.Tutores.AnyAsync(t => t.Email == req.Email && t.Id != id, ct))
                return Results.Conflict(new { erro = "Já existe um tutor com este e-mail." });

            tutor.Nome = req.Nome;
            tutor.Email = req.Email;
            tutor.Telefone = req.Telefone;
            await db.SaveChangesAsync(ct);
            return Results.Ok(TutorResponse.De(tutor));
        }).WithSummary("Atualiza um tutor");

        grupo.MapDelete("/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
        {
            var tutor = await db.Tutores.FindAsync([id], ct);
            if (tutor is null) return Results.NotFound();

            if (await db.Pets.AnyAsync(p => p.TutorId == id, ct))
                return Results.Conflict(new { erro = "Não é possível excluir um tutor que possui pets cadastrados." });

            db.Tutores.Remove(tutor);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithSummary("Remove um tutor sem pets");
    }
}
