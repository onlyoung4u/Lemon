using FluentValidation;
using Lemon.Dtos;

namespace Lemon.Validators;

public class RoleCreateRequestValidator : AbstractValidator<RoleCreateRequest>
{
    public RoleCreateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("角色名称不能为空")
            .Length(2, 10)
            .WithMessage("角色名称长度必须在2到10个字符之间");

        RuleFor(x => x.Permissions).NotEmpty().WithMessage("权限不能为空");
    }
}

public class RoleUpdateRequestValidator : AbstractValidator<RoleUpdateRequest>
{
    public RoleUpdateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("角色名称不能为空")
            .Length(2, 10)
            .WithMessage("角色名称长度必须在2到10个字符之间");

        RuleFor(x => x.Permissions).NotEmpty().WithMessage("权限不能为空");
    }
}
