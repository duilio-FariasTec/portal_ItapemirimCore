namespace Itapemirim.Ani.Application.Validators;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Auth;

public class TrocarSenhaValidator : AbstractValidator<TrocarSenhaDto>
{
    public TrocarSenhaValidator()
    {
        RuleFor(x => x.ValorUsuario)
            .NotEmpty()
            .WithName("usuario")
            .WithMessage("O campo 'usuario' (login ou e-mail) é obrigatório.")
            .MaximumLength(254)
            .WithName("usuario")
            .WithMessage("O campo 'usuario' deve ter no máximo 254 caracteres.");

        RuleFor(x => x.SenhaAtual)
            .NotEmpty()
            .WithMessage("A senha atual é obrigatória.");

        RuleFor(x => x.NovaSenha)
            .NotEmpty()
            .WithMessage("A nova senha é obrigatória.")
            .MinimumLength(8)
            .WithMessage("A nova senha deve conter ao menos 8 caracteres.")
            .NotEqual(x => x.SenhaAtual)
            .WithMessage("A nova senha deve ser diferente da senha atual.");
    }
}

