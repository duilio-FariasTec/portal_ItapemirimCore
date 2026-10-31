namespace Itapemirim.Ani.Application.Validators;

using FluentValidation;
using Itapemirim.Ani.Application.DTOs.Usuarios;

public class UsuarioCreateValidator : AbstractValidator<UsuarioCreateRequest>
{
    public UsuarioCreateValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome Ã© obrigatÃ³rio.")
            .MaximumLength(150).WithMessage("O nome deve ter no mÃ¡ximo 150 caracteres.");

        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("O login Ã© obrigatÃ³rio.")
            .MaximumLength(100).WithMessage("O login deve ter no mÃ¡ximo 100 caracteres.")
            .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("O login contÃ©m caracteres invÃ¡lidos.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("E-mail em formato invÃ¡lido.")
            .MaximumLength(254).WithMessage("O e-mail deve ter no mÃ¡ximo 254 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("A senha Ã© obrigatÃ³ria.")
            .MinimumLength(8).WithMessage("A senha deve conter ao menos 8 caracteres.");
    }
}