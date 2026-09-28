using Microsoft.Extensions.Configuration;

namespace PortfolioApp.Infrastructure.EfCore;

/// <summary>
/// Resolves the SQLite file. An explicit path (DB_PATH) wins, otherwise database:relativePath from
/// config/appsettings.shared.json is used. Relative paths are resolved against the folder that
/// contains config/, found by walking up from the app directory, so the Web app, the DbUpdater and
/// the Console client share one database.
/// </summary>
public static class DbPathResolver
{
    private const string SharedConfig = "config/appsettings.shared.json";
    private const string DefaultRelativePath = "Database/portfolio.db";

    public static string Resolve(string? dbPath = null)
    {
        var root = FindConfigRoot();
        var path = string.IsNullOrWhiteSpace(dbPath) ? ReadConfiguredPath(root) : dbPath;

        var fullPath = Path.GetFullPath(path, root ?? AppContext.BaseDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        return fullPath;
    }

    private static string ReadConfiguredPath(string? root)
    {
        if (root is null) return DefaultRelativePath;

        var configured = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(root, SharedConfig), optional: true)
            .Build()["database:relativePath"];

        return string.IsNullOrWhiteSpace(configured) ? DefaultRelativePath : configured;
    }

    private static string? FindConfigRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, SharedConfig)))
                return dir.FullName;
        }
        return null;
    }
}
