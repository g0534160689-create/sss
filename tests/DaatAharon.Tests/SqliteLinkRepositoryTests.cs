using DaatAharon.Core.Links;
using DaatAharon.Infrastructure.Links;
using Xunit;

namespace DaatAharon.Tests;

public sealed class SqliteLinkRepositoryTests
{
    [Fact]
    public async Task AddUpdateGetAndDelete_PersistChanges()
    {
        using var directory = Directory.CreateTempSubdirectory("DaatAharon.Tests.");
        var repository = new SqliteLinkRepository(Path.Combine(directory.FullName, "links.db"));
        await repository.InitializeAsync();

        var original = new LinkItem(
            Guid.NewGuid(),
            "מערכת בדיקה",
            "https://example.com/start",
            "בדיקות",
            DateTimeOffset.UtcNow);

        await repository.AddAsync(original);

        var edited = original with
        {
            Title = "מערכת מעודכנת",
            Url = "https://example.com/updated",
            Category = "מערכות"
        };
        await repository.UpdateAsync(edited);

        var afterUpdate = await repository.GetAllAsync();
        var stored = Assert.Single(afterUpdate);
        Assert.Equal(edited.Title, stored.Title);
        Assert.Equal(edited.Url, stored.Url);
        Assert.Equal(edited.Category, stored.Category);

        await repository.DeleteAsync(edited.Id);
        Assert.Empty(await repository.GetAllAsync());
    }

    [Fact]
    public async Task AddAsync_RejectsNonHttpSchemes()
    {
        using var directory = Directory.CreateTempSubdirectory("DaatAharon.Tests.");
        var repository = new SqliteLinkRepository(Path.Combine(directory.FullName, "links.db"));
        await repository.InitializeAsync();

        var invalid = new LinkItem(
            Guid.NewGuid(),
            "קישור לא תקין",
            "javascript:alert(1)",
            "בדיקות",
            DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<ArgumentException>(() => repository.AddAsync(invalid));
    }
}
