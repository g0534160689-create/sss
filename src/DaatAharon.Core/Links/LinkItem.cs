namespace DaatAharon.Core.Links;

public sealed record LinkItem(
    Guid Id,
    string Title,
    string Url,
    string Category,
    DateTimeOffset CreatedUtc);
