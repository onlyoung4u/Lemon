using FluentValidation;
using Lemon.Dtos;

namespace Lemon.Validators;

public class LogQueryRequestValidator : AbstractValidator<LogQueryRequest>
{
    public LogQueryRequestValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .When(x => x.UserId.HasValue)
            .WithMessage("用户ID必须大于0");

        RuleFor(x => x.StartTime)
            .LessThanOrEqualTo(x => x.EndTime)
            .When(x => x.StartTime.HasValue && x.EndTime.HasValue)
            .WithMessage("开始时间不能晚于结束时间");

        RuleFor(x => x.EndTime)
            .LessThanOrEqualTo(DateTime.Now)
            .When(x => x.EndTime.HasValue)
            .WithMessage("结束时间不能晚于当前时间");
    }
}
