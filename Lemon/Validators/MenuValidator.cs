using FluentValidation;
using Lemon.Dtos;

namespace Lemon.Validators;

public class MenuCreateRequestValidator : AbstractValidator<MenuCreateRequest>
{
    public MenuCreateRequestValidator()
    {
        RuleFor(x => x.MenuType).InclusiveBetween(1, 2).WithMessage("类型错误");

        RuleFor(x => x.ParentId).GreaterThanOrEqualTo(0).WithMessage("未知的父级菜单");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("标题不能为空")
            .Length(2, 10)
            .WithMessage("标题长度必须在2到10个字符之间");

        RuleFor(x => x.Permission)
            .NotEmpty()
            .WithMessage("权限不能为空")
            .Length(2, 64)
            .WithMessage("权限长度必须在2到64个字符之间");

        RuleFor(x => x.Path).NotEmpty().When(x => x.MenuType == 1).WithMessage("菜单路径不能为空");

        RuleFor(x => x.Icon).MaximumLength(64).WithMessage("图标长度不能超过64个字符");

        RuleFor(x => x.Link)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.Link))
            .WithMessage("链接必须是有效的URL格式");

        RuleFor(x => x.Sort).GreaterThanOrEqualTo(0).WithMessage("排序值必须大于等于0");
    }

    private static bool BeValidUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var result)
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

public class MenuUpdateRequestValidator : AbstractValidator<MenuUpdateRequest>
{
    public MenuUpdateRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("标题不能为空")
            .Length(2, 10)
            .WithMessage("标题长度必须在2到10个字符之间");

        RuleFor(x => x.Permission)
            .NotEmpty()
            .WithMessage("权限不能为空")
            .Length(2, 64)
            .WithMessage("权限长度必须在2到64个字符之间");

        RuleFor(x => x.Icon).MaximumLength(64).WithMessage("图标长度不能超过64个字符");

        RuleFor(x => x.Link)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.Link))
            .WithMessage("链接必须是有效的URL格式");

        RuleFor(x => x.Sort).GreaterThanOrEqualTo(0).WithMessage("排序值必须大于等于0");
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var result)
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
