using CursoMongo.Api.Data.Schemas;
using CursoMongo.Api.Domain.Entities;
using CursoMongo.Api.Domain.Enums;
using CursoMongo.Api.Domain.ValueObjects;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

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
        
        Queryable.Where(_restaurantes.AsQueryable(), x => x.Nome.ToLower().Contains(nome.ToLower()))
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

    public async Task<Dictionary<Restaurante, double>> ObterTop3()
    {
        var retorno = new Dictionary<Restaurante, double>();

        var top3 = _avaliacoes.Aggregate()
            .Group(x => x.RestauranteId, 
                g => new
                {
                    RestauranteId = g.Key, MediaEstrelas = g.Average(a => a.Estrelas)
                })
            .SortByDescending(x => x.MediaEstrelas)
            .Limit(3);

        await top3.ForEachAsync(x =>
        {
            var restaurante = ObterPorId(x.RestauranteId);

            Queryable.Where(_avaliacoes.AsQueryable(), a => a.RestauranteId == x.RestauranteId)
                .ToList()
                .ForEach(a => restaurante.InserirAvaliacao(a.ConverterParaDomain()));
            
            retorno.Add(restaurante, x.MediaEstrelas);
        });
        
        return retorno;
    }

    public (long, long) Remover(string restauranteId)
    {
        var resultadoAvaliacoes = _avaliacoes.DeleteMany(x => x.RestauranteId == restauranteId);
        var resultadoRestaurante = _restaurantes.DeleteOne(x => x.Id == restauranteId);

        return (resultadoRestaurante.DeletedCount, resultadoAvaliacoes.DeletedCount);
    }

    public async Task<IEnumerable<Restaurante>> ObterPorBuscaTextual(string texto)
    {
        var restaurantes = new List<Restaurante>();

        var filter = Builders<RestauranteSchema>.Filter.Text(texto);

        await _restaurantes.AsQueryable()
            .Where(x => filter.Inject())
            .ForEachAsync(d => restaurantes.Add(d.ConverterParaDomain()));

        return restaurantes;
    }

    public async Task<Dictionary<Restaurante, double>> ObterTop3_ComLookup()
    {
        var retorno = new Dictionary<Restaurante, double>();

        var top3 = _avaliacoes.Aggregate()
            .Group(x => x.RestauranteId, g => new { RestauranteId = g.Key, MediaEstrelas = g.Average(a => a.Estrelas) })
            .SortByDescending(x => x.MediaEstrelas)
            .Limit(3)
            .Lookup<RestauranteSchema, RestauranteAvaliacaoSchema>("restaurante", "RestauranteId", "Id", "Restaurante")
            .Lookup<AvaliacaoSchema, RestauranteAvaliacaoSchema>("avaliacoes", "Id", "RestauranteId", "Avaliacoes");

        await top3.ForEachAsync(x =>
        {
            var restaurante = new Restaurante(x.Id, x.Restaurante[0].Nome, x.Restaurante[0].Cozinha);
            var endereco = new Endereco(
                x.Restaurante[0].Endereco.Logradouro,
                x.Restaurante[0].Endereco.Numero,
                x.Restaurante[0].Endereco.Cidade,
                x.Restaurante[0].Endereco.UF,
                x.Restaurante[0].Endereco.Cep);

            restaurante.AtribuirEndereco(endereco);
            
            x.Avaliacoes.ForEach(a => restaurante.InserirAvaliacao(a.ConverterParaDomain()));
            
            retorno.Add(restaurante, x.MediaEstrelas);
        });

        return retorno;
    }
}