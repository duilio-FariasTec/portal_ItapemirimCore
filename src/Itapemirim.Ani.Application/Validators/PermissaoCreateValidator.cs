namespace Itapemirim.Ani.Application.Validators;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Permissoes;

public class PermissaoCreateValidator : AbstractValidator<PermissaoCreateRequest>
{
    public PermissaoCreateValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("O cÃ³digo Ã© obrigatÃ³rio.")
            .MaximumLength(150).WithMessage("O cÃ³digo deve ter no mÃ¡ximo 150 caracteres.");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome Ã© obrigatÃ³rio.")
            .MaximumLength(150).WithMessage("O nome deve ter no mÃ¡ximo 150 caracteres.");

        RuleFor(x => x.Recurso)
            .NotEmpty().WithMessage("O recurso Ã© obrigatÃ³rio.")
            .MaximumLength(100).WithMessage("O recurso deve ter no mÃ¡ximo 100 caracteres.");

        RuleFor(x => x.Acao)
            .NotEmpty().WithMessage("A ação é obrigatória.")
            .MaximumLength(50).WithMessage("A ação deve ter no máximo 50 caracteres.");

        RuleFor(x => x.DscUrl)
            .MaximumLength(250).WithMessage("A rota/caminho da tela (DscUrl) deve ter no máximo 250 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.DscUrl));
    }
}