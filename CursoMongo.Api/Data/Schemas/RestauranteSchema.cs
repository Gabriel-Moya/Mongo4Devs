using CursoMongo.Api.Domain.Enums;

namespace CursoMongo.Api.Data.Schemas;

public class RestauranteSchema
{
    public string Id { get; set; }
    public string Nome { get; set; }
    public ECozinha Cozinha { get; set; }
    public EnderecoSchema Endereco { get; set; }
}