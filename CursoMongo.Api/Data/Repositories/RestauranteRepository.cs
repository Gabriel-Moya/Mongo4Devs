using CursoMongo.Api.Data.Schemas;
using CursoMongo.Api.Domain.Entities;
using CursoMongo.Api.Domain.Enums;
using CursoMongo.Api.Domain.ValueObjects;

using MongoDB.Bson;
using MongoDB.Driver;

namespace CursoMongo.Api.Data.Repositories;

public class RestauranteRepository
{
    private readonly IMongoCollection<RestauranteSchema> _restaurantes;
    private readonly IMongoCollection<AvaliacaoSchema> _avaliacoes;

    public RestauranteRepository(MongoDB mongoDB)
    {
        _restaurantes = mongoDB.DB.GetCollection<RestauranteSchema>("restaurante");
        _avaliacoes = mongoDB.DB.GetCollection<AvaliacaoSchema>("avaliacoes");
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

    public bool AlterarCompleto(Restaurante restaurante)
    {
        var document = new RestauranteSchema
        {
            Id = restaurante.Id,
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

        var resultado = _restaurantes.ReplaceOne(x => x.Id == document.Id, document);

        return resultado.ModifiedCount > 0;
    }

    public bool AlterarCozinha(string id, ECozinha cozinha)
    {
        var atualizacao = Builders<RestauranteSchema>.Update.Set(x => x.Cozinha, cozinha);

        var resultado = _restaurantes.UpdateOne(x => x.Id == id, atualizacao);

        return resultado.ModifiedCount > 0;
    }

    public IEnumerable<Restaurante> ObterPorNome(string nome)
    {
        var restaurantes = new List<Restaurante>();
        
        _restaurantes.AsQueryable()
            .Where(x => x.Nome.ToLower().Contains(nome.ToLower()))
            .ToList()
            .ForEach(x => restaurantes.Add(x.ConverterParaDomain()));

        return restaurantes;
    }

    public void Avaliar(string restauranteId, Avaliacao avaliacao)
    {
        var document = new AvaliacaoSchema
        {
            RestauranteId = restauranteId,
            Estrelas = avaliacao.Estrelas,
            Comentario = avaliacao.Comentario
        };

        _avaliacoes.InsertOne(document);
    }
}