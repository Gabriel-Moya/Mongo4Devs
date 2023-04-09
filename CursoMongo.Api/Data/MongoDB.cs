using CursoMongo.Api.Data.Schemas;
using CursoMongo.Api.Domain.Enums;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace CursoMongo.Api.Data;

public class MongoDB
{
    public MongoDB(IConfiguration configuration)
    {
        try
        {
            var client = new MongoClient(configuration["ConnectionString"]);
            DB = client.GetDatabase(configuration["NomeBanco"]);
            MapClasses();
        }
        catch (Exception ex)
        {
            throw new MongoException("Não foi possível se conectar ao MongoDB", ex);
        }
    }
    
    public IMongoDatabase DB { get; }

    private void MapClasses()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(RestauranteSchema)))
        {
            BsonClassMap.RegisterClassMap<RestauranteSchema>(i =>
            {
                i.AutoMap();
                i.MapIdMember(c => c.Id);
                i.MapMember(c => c.Cozinha).SetSerializer(new EnumSerializer<ECozinha>(BsonType.Int32));
                i.SetIgnoreExtraElements(true);
            });
        }
    }
}