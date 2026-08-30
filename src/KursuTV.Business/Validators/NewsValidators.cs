using FluentValidation;
using KursuTV.Business.DTOs;

namespace KursuTV.Business.Validators;

public class NewsCreateValidator : AbstractValidator<NewsCreateDto>
{
    public NewsCreateValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Haber başlığı zorunludur.")
            .MaximumLength(300).WithMessage("Haber başlığı en fazla 300 karakter olabilir.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Haber içeriği boş bırakılamaz.");

        RuleFor(x => x.Spot)
            .MaximumLength(500).WithMessage("Haber spotu en fazla 500 karakter olabilir.");

        RuleFor(x => x.CategoryIds)
            .NotEmpty().WithMessage("En az bir kategori seçilmelidir.");
    }
}

public class NewsUpdateValidator : AbstractValidator<NewsUpdateDto>
{
    public NewsUpdateValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Geçerli bir haber ID'si gereklidir.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Haber başlığı zorunludur.")
            .MaximumLength(300).WithMessage("Haber başlığı en fazla 300 karakter olabilir.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Haber içeriği boş bırakılamaz.");

        RuleFor(x => x.Spot)
            .MaximumLength(500).WithMessage("Haber spotu en fazla 500 karakter olabilir.");

        RuleFor(x => x.CategoryIds)
            .NotEmpty().WithMessage("En az bir kategori seçilmelidir.");
    }
}
