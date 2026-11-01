namespace Itapemirim.Ani.Application.Validators;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Sistemas;

public class SistemaUpdateValidator : AbstractValidator<SistemaUpdateRequest>
{
    public SistemaUpdateValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("O código do sistema é obrigatório.")
            .MaximumLength(50).WithMessage("O código deve ter no máximo 50 caracteres.")
            .Matches("^[A-Z0-9_-]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            .WithMessage("O código do sistema deve conter apenas letras, números, hífen ou underline.");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome do sistema é obrigatório.")
            .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Descricao)
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descricao));
    }
}
