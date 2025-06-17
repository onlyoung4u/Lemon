using FluentValidation;
using Lemon.Dtos;

namespace Lemon.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("用户名不能为空")
            .Length(6, 64)
            .WithMessage("用户名长度必须在6到64个字符之间");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("密码不能为空")
            .Length(6, 20)
            .WithMessage("密码长度必须在6到20个字符之间");
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.OldPassword).NotEmpty().WithMessage("旧密码不能为空");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("新密码不能为空")
            .Length(6, 20)
            .WithMessage("新密码长度必须在6到20个字符之间");
    }
}
