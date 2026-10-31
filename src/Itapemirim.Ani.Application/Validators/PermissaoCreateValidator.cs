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
            .NotEmpty().WithMessage("A aÃ§Ã£o Ã© obrigatÃ³ria.")
            .MaximumLength(50).WithMessage("A aÃ§Ã£o deve ter no mÃ¡ximo 50 caracteres.");
    }
}