using CursoMongo.Api.Domain.Enums;
using CursoMongo.Api.Domain.ValueObjects;

using FluentValidation;
using FluentValidation.Results;

namespace CursoMongo.Api.Domain.Entities;

public class Restaurante : AbstractValidator<Restaurante>
{
    #region Constructor

    public Restaurante(string id, string nome, ECozinha cozinha)
    {
        Id = id;
        Nome = nome;
        Cozinha = cozinha;
    }

    #endregion

    #region Properties

    public string Id { get; private set; }
    public string Nome { get; private set; }
    public ECozinha Cozinha { get; private set; }
    public Endereco Endereco { get; private set; }
    
    public ValidationResult ValidationResult { get; set; }

    #endregion

    #region Public Methods

    public void AtribuirEndereco(Endereco endereco)
    {
        Endereco = endereco;
    }

    #endregion
    
    #region Validations

    public virtual bool Validar()
    {
        ValidarNome();
        ValidationResult = Validate(this);

        ValidarEndereco();

        return ValidationResult.IsValid;
    }

    public void ValidarNome()
    {
        RuleFor(c => c.Nome)
            .NotEmpty().WithMessage("Nome não pode ser vazio.")
            .MaximumLength(30).WithMessage("Nome pode ter no máximo 30 caracteres.");
    }

    private void ValidarEndereco()
    {
        if (Endereco.Validar())
            return;

        foreach (var error in Endereco.ValidationResult.Errors)
            ValidationResult.Errors.Add(error);
    }

    #endregion
    
}