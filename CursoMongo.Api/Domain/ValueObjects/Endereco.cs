using FluentValidation;
using FluentValidation.Results;

namespace CursoMongo.Api.Domain.ValueObjects;

public class Endereco : AbstractValidator<Endereco>
{
    #region Constructor

    public Endereco(string logradouro, string numero, string cidade, string uf, string cep)
    {
        Logradouro = logradouro;
        Numero = numero;
        Cidade = cidade;
        UF = uf;
        Cidade = cidade;
        Cep = cep;
    }

    #endregion
    
    #region Properties
    
    public string Logradouro { get; private set; }
    public string Numero { get; private set; }
    public string Cidade { get; private set; }
    public string UF { get; private set; }
    public string Cep { get; private set; }
    
    public ValidationResult ValidationResult { get; set; }
    
    #endregion
    
    #region Validations

    public bool Validar()
    {
        ValidarLogradouro();
        ValidarCidade();
        ValidarUf();
        ValidarCep();

        ValidationResult = Validate(this);
        
        return ValidationResult.IsValid;
    }

    private void ValidarLogradouro()
    {
        RuleFor(c => c.Logradouro)
            .NotEmpty().WithMessage("Logradouro não pode ser vazio.")
            .MaximumLength(50).WithMessage("Logradouro pode ter no máximo 50 caracteres.");
    }

    private void ValidarCidade()
    {
        RuleFor(c => c.Cidade)
            .NotEmpty().WithMessage("Cidade não pode ser vazio.")
            .MaximumLength(100).WithMessage("Cidade pode ter no máximo 100 caracteres.");
    }

    private void ValidarUf()
    {
        RuleFor(c => c.UF)
            .NotEmpty().WithMessage("UF não pode ser vazio.")
            .Length(2).WithMessage("UF deve ter 2 caracteres.");
    }

    private void ValidarCep()
    {
        RuleFor(c => c.Cep)
            .NotEmpty().WithMessage("CEP não pode ser vazio.")
            .Length(8).WithMessage("CEP deve ter 8 caracteres.");
    }
    
    #endregion
}