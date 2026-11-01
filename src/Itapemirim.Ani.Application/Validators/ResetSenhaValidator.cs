namespace Itapemirim.Ani.Application.Validators;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Auth;

public class ResetSenhaValidator : AbstractValidator<ResetSenhaDto>
{
    public ResetSenhaValidator()
    {
        RuleFor(x => x.ValorIdentificador)
            .NotEmpty()
            .WithName("identificador")
            .WithMessage("É obrigatório informar o 'login', 'email' ou 'identificador' do usuário.")
            .MaximumLength(254)
            .WithName("identificador")
            .WithMessage("O identificador informado deve ter no máximo 254 caracteres.");
    }
}

