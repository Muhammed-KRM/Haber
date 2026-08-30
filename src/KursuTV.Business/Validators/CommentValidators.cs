using FluentValidation;
using KursuTV.Business.DTOs;

namespace KursuTV.Business.Validators;

public class CreateCommentValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentValidator()
    {
        RuleFor(x => x.NewsId)
            .NotEmpty().WithMessage("Haber ID'si zorunludur.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Yorum metni boş bırakılamaz.")
            .MaximumLength(2000).WithMessage("Yorum metni en fazla 2000 karakter olabilir.");
    }
}
