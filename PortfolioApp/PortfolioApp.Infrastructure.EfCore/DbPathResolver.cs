using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace PortfolioApp.Infrastructure.EfCore;

public static class DbPathResolver
{
    private const string DefaultRel = "Database/portfolio.db";
    private const string ConfigRelPath = "config/appsettings.shared.json";

    // Ищем ближайший вверх по дереву каталог, где есть config/appsettings.shared.json
    private static string? findConfigBase()
    {
        var cur = new DirectoryInfo(AppContext.BaseDirectory);

        while (cur != null)
        {
            var candidate = Path.Combine(cur.FullName, ConfigRelPath);
            if (File.Exists(candidate))
                return cur.FullName;

            // Доп. маркеры корня репо: Database/.git/*.sln
            if (Directory.Exists(Path.Combine(cur.FullName, "Database")) ||
                Directory.Exists(Path.Combine(cur.FullName, ".git")) ||
                cur.GetFiles("*.sln").Any())
            {
                // если есть маркер, но файла пока нет — возможно конфиг рядом с проектом
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
            // если нашли базу для конфига — читаем его; делаем optional:true, чтобы не падать
            builder.SetBasePath(configBasePath)
                   .AddJsonFile(path: ConfigRelPath, optional: true, reloadOnChange: false);
        }

        // Никаких ENV, как ты просил
        return builder.Build();
    }

    /// <summary>
    /// Только конфиг. Если конфиг не найден или ключ пуст — используем DefaultRel.
    /// Относительный путь трактуется от каталога, где найден конфиг, иначе — от ближайшего "корня" (где виден Database/.git/*.sln) или от папки запуска.
    /// </summary>
    public static string getAbsoluteDbPath(string key = "database:relativePath")
    {
        var cfg = buildConfig(out var cfgBase);

        // читаем значение; если пустое — возьмём дефолт
        var configured = cfg[key];
        var path = string.IsNullOrWhiteSpace(configured) ? DefaultRel : configured;

        // Определяем базовый каталог для относительных путей
        var baseDir = cfgBase ?? findNearestRoot() ?? AppContext.BaseDirectory;

        var abs = Path.IsPathRooted(path) ? path : Path.Combine(baseDir, path);
        Directory.CreateDirectory(Path.GetDirectoryName(abs)!);
        return abs;
    }

    // запасной "корень" если config не найден: поднимаемся, пока не увидим Database/.git/*.sln
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
