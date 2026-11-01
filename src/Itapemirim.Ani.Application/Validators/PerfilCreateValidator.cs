namespace Itapemirim.Ani.Application.Validators;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Perfis;

public class PerfilCreateValidator : AbstractValidator<PerfilCreateRequest>
{
    public PerfilCreateValidator()
    {
        RuleFor(x => x.SistemaId)
            .NotEmpty().WithMessage("O identificador do sistema (SistemaId) é obrigatório.");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome do perfil é obrigatório.")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Descricao)
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descricao));
    }
}