using CursoMongo.Api.Controllers.Inputs;
using CursoMongo.Api.Domain.Entities;
using CursoMongo.Api.Domain.Enums;
using CursoMongo.Api.Domain.ValueObjects;

using Microsoft.AspNetCore.Mvc;

namespace CursoMongo.Api.Controllers;

[ApiController]
public class RestauranteController : ControllerBase
{
    [HttpPost("restaurante")]
    public ActionResult IncluirRestaurante([FromBody] RestauranteInclusao restauranteInclusao)
    {
        var cozinha = ECozinhaHelper.ConverterDeInteiro(restauranteInclusao.Cozinha);

        var restaurante = new Restaurante(restauranteInclusao.Nome, cozinha);
        var endereco = new Endereco(
            restauranteInclusao.Logradouro,
            restauranteInclusao.Numero,
            restauranteInclusao.Cidade,
            restauranteInclusao.Uf,
            restauranteInclusao.Cep);
        
        restaurante.AtribuirEndereco(endereco);

        if (!restaurante.Validar())
        {
            return BadRequest(new { errors = restaurante.ValidationResult.Errors.Select(_ => _.ErrorMessage) });
        }

        _restauranteRepository.Inserir(restaurante);

        return Ok(new { data = "Restaurante inserido com sucesso" });
    }
}