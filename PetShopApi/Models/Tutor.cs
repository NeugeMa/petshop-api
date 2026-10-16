namespace PetShopApi.Models;

public class Tutor
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;

    public List<Pet> Pets { get; set; } = [];
}
