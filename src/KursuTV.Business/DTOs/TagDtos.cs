namespace KursuTV.Business.DTOs;

public record TagDto(
    int Id,
    string Name,
    string Slug
);

public record CreateTagDto(
    string Name
);
