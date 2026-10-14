using System.ComponentModel.DataAnnotations;

namespace PetShopApi.Endpoints;

public static class Validacao
{
    /// <summary>Valida um DTO via DataAnnotations; retorna null se válido ou um 400 com os erros.</summary>
    public static IResult? Validar<T>(T dto) where T : notnull
    {
        var erros = new List<ValidationResult>();
        if (Validator.TryValidateObject(dto, new ValidationContext(dto), erros, validateAllProperties: true))
            return null;

        var detalhes = erros
            .SelectMany(e => (e.MemberNames.Any() ? e.MemberNames : [""])
                .Select(campo => (campo, mensagem: e.ErrorMessage ?? "Valor inválido.")))
            .GroupBy(x => x.campo)
            .ToDictionary(g => g.Key, g => g.Select(x => x.mensagem).ToArray());

        return Results.ValidationProblem(detalhes);
    }
}
