namespace PetShopApi.Models;

public enum TipoServico
{
    Banho,
    Tosa,
    BanhoETosa,
    Consulta,
    Vacina
}

public enum StatusAgendamento
{
    Agendado,
    Concluido,
    Cancelado
}

public class Agendamento
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public DateTime DataHora { get; set; }
    public TipoServico Servico { get; set; }
    public decimal Valor { get; set; }
    public StatusAgendamento Status { get; set; } = StatusAgendamento.Agendado;

    public Pet? Pet { get; set; }
}
