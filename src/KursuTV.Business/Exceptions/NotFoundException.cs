namespace KursuTV.Business.Exceptions;

public class NotFoundException : BusinessException
{
    public NotFoundException(string entityName, object key)
        : base($"'{entityName}' bulunamadÄ±. Aranan anahtar: {key}") { }
}
