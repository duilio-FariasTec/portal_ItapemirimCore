namespace Itapemirim.Ani.Application.Validators;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Usuarios;

public class UsuarioUpdateValidator : AbstractValidator<UsuarioUpdateRequest>
{
    public UsuarioUpdateValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome Ã© obrigatÃ³rio.")
            .MaximumLength(150).WithMessage("O nome deve ter no mÃ¡ximo 150 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("E-mail em formato invÃ¡lido.")
            .MaximumLength(254).WithMessage("O e-mail deve ter no mÃ¡ximo 254 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Status)
            .Must(s => s is "Ativo" or "Inativo" or "Bloqueado")
            .WithMessage("Status invÃ¡lido. Use Ativo, Inativo ou Bloqueado.");
    }
}