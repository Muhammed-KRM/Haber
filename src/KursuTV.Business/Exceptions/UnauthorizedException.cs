namespace KursuTV.Business.Exceptions;

public class UnauthorizedException : BusinessException
{
    public UnauthorizedException(string message = "Bu iÅŸlem iÃ§in yetkiniz bulunmamaktadÄ±r.")
        : base(message) { }
}
