namespace DaatAharon.Core.Links;

public interface ILinkRepository
{
    Task<IReadOnlyList<LinkItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(LinkItem link, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
