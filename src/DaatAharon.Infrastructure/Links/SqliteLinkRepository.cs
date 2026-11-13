using DaatAharon.Core.Links;
using Microsoft.Data.Sqlite;

namespace DaatAharon.Infrastructure.Links;

public sealed class SqliteLinkRepository : ILinkRepository
{
    private readonly string _connectionString;

    public SqliteLinkRepository(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        var fullPath = Path.GetFullPath(databasePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = fullPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            Pooling = true
        }.ToString();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            PRAGMA journal_mode=WAL;
            PRAGMA foreign_keys=ON;
            CREATE TABLE IF NOT EXISTS Links (
                Id TEXT NOT NULL PRIMARY KEY,
                Title TEXT NOT NULL,
                Url TEXT NOT NULL,
                Category TEXT NOT NULL,
                CreatedUtc TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS IX_Links_Category ON Links(Category);
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LinkItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Url, Category, CreatedUtc FROM Links ORDER BY Category, Title COLLATE NOCASE;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var links = new List<LinkItem>();
        while (await reader.ReadAsync(cancellationToken))
        {
            links.Add(new LinkItem(
                Guid.Parse(reader.GetString(0)),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                DateTimeOffset.Parse(reader.GetString(4), System.Globalization.CultureInfo.InvariantCulture)));
        }
        return links;
    }

    public async Task AddAsync(LinkItem link, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(link);
        if (!LinkAddress.TryNormalize(link.Url, out var safeUrl))
            throw new ArgumentException("Only valid absolute HTTP or HTTPS URLs are accepted.", nameof(link));

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Links(Id, Title, Url, Category, CreatedUtc) VALUES($id, $title, $url, $category, $created);";
        command.Parameters.AddWithValue("$id", link.Id.ToString("D"));
        command.Parameters.AddWithValue("$title", link.Title.Trim());
        command.Parameters.AddWithValue("$url", safeUrl);
        command.Parameters.AddWithValue("$category", link.Category.Trim());
        command.Parameters.AddWithValue("$created", link.CreatedUtc.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Links WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString("D"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
