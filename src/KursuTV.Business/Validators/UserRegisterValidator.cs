using FluentValidation;
using KursuTV.Business.DTOs;

namespace KursuTV.Business.Validators;

public class UserRegisterValidator : AbstractValidator<UserRegisterDto>
{
    public UserRegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi zorunludur.")
            .EmailAddress().WithMessage("GeÃ§erli bir e-posta adresi giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Åifre zorunludur.")
            .MinimumLength(8).WithMessage("Åifre en az 8 karakter olmalÄ±dÄ±r.")
            .Matches(@"[A-Z]").WithMessage("Åifre en az bir bÃ¼yÃ¼k harf iÃ§ermelidir.")
            .Matches(@"[a-z]").WithMessage("Åifre en az bir kÃ¼Ã§Ã¼k harf iÃ§ermelidir.")
            .Matches(@"[0-9]").WithMessage("Åifre en az bir rakam iÃ§ermelidir.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad Soyad zorunludur.")
            .MinimumLength(3).WithMessage("Ad Soyad en az 3 karakter olmalÄ±dÄ±r.")
            .MaximumLength(100).WithMessage("Ad Soyad en fazla 100 karakter olabilir.");
    }
}
