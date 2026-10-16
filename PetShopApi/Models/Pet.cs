namespace PetShopApi.Models;

public class Pet
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string? Raca { get; set; }
    public DateOnly? DataNascimento { get; set; }
    public int TutorId { get; set; }

    public Tutor? Tutor { get; set; }
    public List<Agendamento> Agendamentos { get; set; } = [];
}
