using FluentValidation;
using Lemon.Dtos;

namespace Lemon.Validators;

public class ConfigRequestValidator : AbstractValidator<ConfigRequest>
{
    public ConfigRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("名称不能为空")
            .Length(2, 60)
            .WithMessage("名称长度必须在2到60个字符之间");

        RuleFor(x => x.Key)
            .NotEmpty()
            .WithMessage("键名不能为空")
            .Length(2, 60)
            .WithMessage("键名长度必须在2到60个字符之间");

        RuleFor(x => x.Value).NotEmpty().WithMessage("值不能为空");

        RuleFor(x => x.Remark)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Remark))
            .WithMessage("备注长度不能超过255个字符");
    }
}
