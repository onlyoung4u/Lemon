using FluentValidation;
using Lemon.Dtos;

namespace Lemon.Validators;

public class UserQueryRequestValidator : AbstractValidator<UserQueryRequest>
{
    public UserQueryRequestValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .When(x => x.UserId.HasValue)
            .WithMessage("用户ID必须大于0");
    }
}

public class UserCreateRequestValidator : AbstractValidator<UserCreateRequest>
{
    public UserCreateRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("账号不能为空")
            .Length(4, 20)
            .WithMessage("账号长度必须在4到20个字符之间")
            .Matches("^[a-zA-Z0-9]+$")
            .WithMessage("账号只能包含数字和字母");

        RuleFor(x => x.Nickname)
            .NotEmpty()
            .WithMessage("名称不能为空")
            .Length(2, 10)
            .WithMessage("名称长度必须在2到10个字符之间");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("密码不能为空")
            .Length(6, 20)
            .WithMessage("密码长度必须在6到20个字符之间");

        RuleFor(x => x.IsActive).NotNull().WithMessage("状态不能为空");

        RuleFor(x => x.Roles).NotEmpty().WithMessage("角色不能为空");
    }
}

public class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateRequestValidator()
    {
        RuleFor(x => x.Nickname)
            .NotEmpty()
            .WithMessage("名称不能为空")
            .Length(2, 10)
            .WithMessage("名称长度必须在2到10个字符之间");

        RuleFor(x => x.Password)
            .Length(6, 20)
            .When(x => !string.IsNullOrEmpty(x.Password))
            .WithMessage("密码长度必须在6到20个字符之间");

        RuleFor(x => x.IsActive).NotNull().WithMessage("状态不能为空");

        RuleFor(x => x.Roles).NotEmpty().WithMessage("角色不能为空");
    }
}

public class UserActiveRequestValidator : AbstractValidator<UserActiveRequest>
{
    public UserActiveRequestValidator()
    {
        RuleFor(x => x.IsActive).NotNull().WithMessage("状态不能为空");
    }
}
