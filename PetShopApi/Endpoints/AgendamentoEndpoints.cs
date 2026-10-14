using Microsoft.EntityFrameworkCore;
using PetShopApi.Data;
using PetShopApi.Dtos;
using PetShopApi.Models;

namespace PetShopApi.Endpoints;

public static class AgendamentoEndpoints
{
    public static void MapAgendamentoEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/agendamentos").WithTags("Agendamentos");

        grupo.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Agendamentos.AsNoTracking()
                .OrderBy(a => a.DataHora)
                .Select(a => new AgendamentoResponse(a.Id, a.PetId, a.DataHora, a.Servico, a.Valor, a.Status))
                .ToListAsync(ct)))
            .WithSummary("Lista todos os agendamentos");

        grupo.MapGet("/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
        {
            var ag = await db.Agendamentos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);
            return ag is null ? Results.NotFound() : Results.Ok(AgendamentoResponse.De(ag));
        }).WithSummary("Busca um agendamento pelo id");

        grupo.MapPost("/", async (AgendamentoRequest req, AppDbContext db, CancellationToken ct) =>
        {
            if (Validacao.Validar(req) is { } erro) return erro;

            var regra = await ValidarRegrasAsync(req, null, db, ct);
            if (regra is not null) return regra;

            var ag = new Agendamento
            {
                PetId = req.PetId, DataHora = req.DataHora, Servico = req.Servico,
                Valor = req.Valor, Status = StatusAgendamento.Agendado
            };
            db.Agendamentos.Add(ag);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/agendamentos/{ag.Id}", AgendamentoResponse.De(ag));
        }).WithSummary("Agenda um serviço para um pet");

        grupo.MapPut("/{id:int}", async (int id, AgendamentoRequest req, AppDbContext db, CancellationToken ct) =>
        {
            if (Validacao.Validar(req) is { } erro) return erro;

            var ag = await db.Agendamentos.FindAsync([id], ct);
            if (ag is null) return Results.NotFound();

            var regra = await ValidarRegrasAsync(req, id, db, ct);
            if (regra is not null) return regra;

            ag.PetId = req.PetId;
            ag.DataHora = req.DataHora;
            ag.Servico = req.Servico;
            ag.Valor = req.Valor;
            ag.Status = req.Status;
            await db.SaveChangesAsync(ct);
            return Results.Ok(AgendamentoResponse.De(ag));
        }).WithSummary("Atualiza um agendamento (inclusive o status)");

        grupo.MapDelete("/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
        {
            var ag = await db.Agendamentos.FindAsync([id], ct);
            if (ag is null) return Results.NotFound();
            db.Agendamentos.Remove(ag);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithSummary("Remove um agendamento");
    }

    // Regras: pet existe, data no futuro (somente se ainda agendado) e sem choque de horário para o mesmo pet.
    private static async Task<IResult?> ValidarRegrasAsync(
        AgendamentoRequest req, int? idAtual, AppDbContext db, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == req.PetId, ct))
            return Results.NotFound(new { erro = $"Pet {req.PetId} não encontrado." });

        if (req.Status == StatusAgendamento.Agendado && req.DataHora <= DateTime.Now)
            return Results.BadRequest(new { erro = "A data/hora de um agendamento deve estar no futuro." });

        if (req.Status != StatusAgendamento.Cancelado &&
            await db.Agendamentos.AnyAsync(a => a.PetId == req.PetId
                && a.DataHora == req.DataHora
                && a.Status != StatusAgendamento.Cancelado
                && a.Id != idAtual, ct))
            return Results.Conflict(new { erro = "Este pet já possui um agendamento neste horário." });

        return null;
    }
}
