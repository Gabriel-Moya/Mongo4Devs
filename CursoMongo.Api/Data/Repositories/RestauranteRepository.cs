using CursoMongo.Api.Data.Schemas;
using CursoMongo.Api.Domain.Entities;
using CursoMongo.Api.Domain.ValueObjects;

using MongoDB.Driver;

namespace CursoMongo.Api.Data.Repositories;

public class RestauranteRepository
{
    private readonly IMongoCollection<RestauranteSchema> _restaurantes;

    public RestauranteRepository(MongoDB mongoDB)
    {
        _restaurantes = mongoDB.DB.GetCollection<RestauranteSchema>("restaurante");
    }

    public void Inserir(Restaurante restaurante)
    {
        var document = new RestauranteSchema
        {
            Nome = restaurante.Nome,
            Cozinha = restaurante.Cozinha,
            Endereco = new EnderecoSchema
            {
                Logradouro = restaurante.Endereco.Logradouro,
                Numero = restaurante.Endereco.Numero,
                Cidade = restaurante.Endereco.Cidade,
                Cep = restaurante.Endereco.Cep,
                UF = restaurante.Endereco.UF
            }
        };
        
        _restaurantes.InsertOne(document);
    }

    public async Task<IEnumerable<Restaurante>> ObterTodos()
    {
        var restaurantes = new List<Restaurante>();

        await _restaurantes.AsQueryable().ForEachAsync(d =>
        {
            var r = new Restaurante(d.Id, d.Nome, d.Cozinha);
            var e = new Endereco(d.Endereco.Logradouro, d.Endereco.Numero, d.Endereco.Cidade, d.Endereco.UF, d.Endereco.Cep);
            r.AtribuirEndereco(e);
            restaurantes.Add(r);
        });

        return restaurantes;
    }

    public Restaurante ObterPorId(string id)
    {
        var document = _restaurantes.AsQueryable().FirstOrDefault(x => x.Id == id);

        if (document is null)
            return null;

        return document.ConverterParaDomain();
    }
}