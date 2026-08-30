using FluentValidation;
using KursuTV.Business.DTOs;

namespace KursuTV.Business.Validators;

public class MessageSendValidator : AbstractValidator<MessageSendDto>
{
    public MessageSendValidator()
    {
        RuleFor(x => x.ReceiverId)
            .NotEmpty().WithMessage("AlÄ±cÄ± belirtilmelidir.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Mesaj iÃ§eriÄŸi boÅŸ olamaz.")
            .MinimumLength(5).WithMessage("Mesaj en az 5 karakter olmalÄ±dÄ±r.")
            .MaximumLength(2000).WithMessage("Mesaj en fazla 2000 karakter olabilir.");
    }
}
