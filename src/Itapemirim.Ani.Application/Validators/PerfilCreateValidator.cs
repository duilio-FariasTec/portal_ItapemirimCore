namespace Itapemirim.Ani.Application.Validators;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Perfis;

public class PerfilCreateValidator : AbstractValidator<PerfilCreateRequest>
{
    public PerfilCreateValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome do perfil Ã© obrigatÃ³rio.")
            .MaximumLength(100).WithMessage("O nome deve ter no mÃ¡ximo 100 caracteres.");

        RuleFor(x => x.Descricao)
            .MaximumLength(500).WithMessage("A descriÃ§Ã£o deve ter no mÃ¡ximo 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descricao));
    }
}