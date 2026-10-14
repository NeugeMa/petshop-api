using System.ComponentModel.DataAnnotations;
using PetShopApi.Models;

namespace PetShopApi.Dtos;

// ---------- Tutor ----------
public record TutorRequest(
    [property: Required] [property: StringLength(120)] string Nome,
    [property: Required] [property: EmailAddress] [property: StringLength(150)] string Email,
    [property: Required] [property: StringLength(20)] string Telefone);

public record TutorResponse(int Id, string Nome, string Email, string Telefone)
{
    public static TutorResponse De(Tutor t) => new(t.Id, t.Nome, t.Email, t.Telefone);
}

// ---------- Pet ----------
public record PetRequest(
    [property: Required] [property: StringLength(80)] string Nome,
    [property: Required] [property: StringLength(50)] string Especie,
    [property: StringLength(80)] string? Raca,
    DateOnly? DataNascimento,
    [property: Range(1, int.MaxValue)] int TutorId);

public record PetResponse(int Id, string Nome, string Especie, string? Raca, DateOnly? DataNascimento, int TutorId)
{
    public static PetResponse De(Pet p) => new(p.Id, p.Nome, p.Especie, p.Raca, p.DataNascimento, p.TutorId);
}

// ---------- Agendamento ----------
public record AgendamentoRequest(
    [property: Range(1, int.MaxValue)] int PetId,
    DateTime DataHora,
    TipoServico Servico,
    [property: Range(0.01, 100000)] decimal Valor,
    StatusAgendamento Status = StatusAgendamento.Agendado);

public record AgendamentoResponse(
    int Id, int PetId, DateTime DataHora, TipoServico Servico, decimal Valor, StatusAgendamento Status)
{
    public static AgendamentoResponse De(Agendamento a) =>
        new(a.Id, a.PetId, a.DataHora, a.Servico, a.Valor, a.Status);
}
