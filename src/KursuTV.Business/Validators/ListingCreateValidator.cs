using FluentValidation;
using KursuTV.Business.DTOs;

namespace KursuTV.Business.Validators;

public class ListingCreateValidator : AbstractValidator<ListingCreateDto>
{
    public ListingCreateValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Ä°lan baÅŸlÄ±ÄŸÄ± zorunludur.")
            .MinimumLength(10).WithMessage("Ä°lan baÅŸlÄ±ÄŸÄ± en az 10 karakter olmalÄ±dÄ±r.")
            .MaximumLength(150).WithMessage("Ä°lan baÅŸlÄ±ÄŸÄ± en fazla 150 karakter olabilir.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Ä°lan aÃ§Ä±klamasÄ± zorunludur.")
            .MinimumLength(10).WithMessage("Ä°lan aÃ§Ä±klamasÄ± en az 10 karakter olmalÄ±dÄ±r.")
            .MaximumLength(3000).WithMessage("Ä°lan aÃ§Ä±klamasÄ± en fazla 3000 karakter olabilir.");

        RuleFor(x => x.HourlyPrice)
            .GreaterThan(0).WithMessage("Saatlik Ã¼cret 0'dan bÃ¼yÃ¼k olmalÄ±dÄ±r.")
            .LessThanOrEqualTo(10000).WithMessage("Saatlik Ã¼cret 10.000 TL'yi geÃ§emez.");

        RuleFor(x => x.BranchId)
            .GreaterThan(0).WithMessage("BranÅŸ seÃ§imi zorunludur.");

        RuleFor(x => x.DistrictId)
            .GreaterThan(0).WithMessage("Ä°lÃ§e seÃ§imi zorunludur.");

        RuleFor(x => x.LessonType)
            .IsInEnum().WithMessage("GeÃ§erli bir ders tÃ¼rÃ¼ seÃ§iniz.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("GeÃ§erli bir ilan tÃ¼rÃ¼ seÃ§iniz.");
    }
}
