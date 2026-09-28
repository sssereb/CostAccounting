using Microsoft.Extensions.Configuration;

namespace PortfolioApp.Infrastructure.EfCore;

public static class DbPathResolver
{
    private const string DefaultRel = "Database/portfolio.db";
    private const string ConfigRelPath = "config/appsettings.shared.json";

    // Walks up to the nearest directory that contains config/appsettings.shared.json
    private static string? findConfigBase()
    {
        var cur = new DirectoryInfo(AppContext.BaseDirectory);

        while (cur != null)
        {
            var candidate = Path.Combine(cur.FullName, ConfigRelPath);
            if (File.Exists(candidate))
                return cur.FullName;

            // Extra repo root markers: Database/, .git, *.sln
            if (Directory.Exists(Path.Combine(cur.FullName, "Database")) ||
                Directory.Exists(Path.Combine(cur.FullName, ".git")) ||
                cur.GetFiles("*.sln").Any())
            {
                // A marker is present but the file is not: the config may sit next to the project
                var localProjCfg = Path.Combine(cur.FullName, ConfigRelPath);
                if (File.Exists(localProjCfg))
                    return cur.FullName;
            }

            cur = cur.Parent;
        }

        return null;
    }

    private static IConfiguration buildConfig(out string? configBasePath)
    {
        configBasePath = findConfigBase();

        var builder = new ConfigurationBuilder();

        if (configBasePath != null)
        {
            // Config base found: read it; optional so a missing file does not throw
            builder.SetBasePath(configBasePath)
                   .AddJsonFile(path: ConfigRelPath, optional: true, reloadOnChange: false);
        }

        // Environment variables are intentionally not used
        return builder.Build();
    }

    /// <summary>
    /// Config only. If the config is missing or the key is empty, DefaultRel is used.
    /// A relative path resolves against the config directory, otherwise against the nearest "root" (where Database/, .git or *.sln is visible), otherwise against the app base directory.
    /// </summary>
    public static string getAbsoluteDbPath(string key = "database:relativePath")
    {
        var cfg = buildConfig(out var cfgBase);

        var configured = cfg[key];
        var path = string.IsNullOrWhiteSpace(configured) ? DefaultRel : configured;

        // Base directory for relative paths
        var baseDir = cfgBase ?? findNearestRoot() ?? AppContext.BaseDirectory;

        var abs = Path.IsPathRooted(path) ? path : Path.Combine(baseDir, path);
        Directory.CreateDirectory(Path.GetDirectoryName(abs)!);
        return abs;
    }

    // Fallback root when no config is found: walk up until Database/, .git or *.sln is visible
    private static string? findNearestRoot()
    {
        var cur = new DirectoryInfo(AppContext.BaseDirectory);
        while (cur != null)
        {
            if (Directory.Exists(Path.Combine(cur.FullName, "Database")) ||
                Directory.Exists(Path.Combine(cur.FullName, ".git")) ||
                cur.GetFiles("*.sln").Any())
                return cur.FullName;
            cur = cur.Parent;
        }
        return null;
    }
}
