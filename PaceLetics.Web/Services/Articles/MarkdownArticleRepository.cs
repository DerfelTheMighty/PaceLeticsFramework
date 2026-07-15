using System.Globalization;
using System.Collections.Concurrent;
using Markdig;
using PaceLetics.TrainingModule.CodeBase.Workouts.Models;
using PaceLetics.Web.Services.Localization;

namespace PaceLetics.Web.Services.Articles;

public sealed class MarkdownArticleRepository : IArticleRepository
{
    private static readonly ConcurrentDictionary<string, IReadOnlyList<Article>> Cache = new(StringComparer.Ordinal);
    private static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private readonly string _academyContentRoot;
    private readonly AppCultureService? _appCulture;

    public MarkdownArticleRepository(string contentRootPath, AppCultureService? appCulture = null)
    {
        _academyContentRoot = Path.Combine(contentRootPath, "Content", "Academy");
        _appCulture = appCulture;
    }

    public IReadOnlyList<Article> GetArticles()
    {
        var culture = _appCulture?.CurrentCulture ?? CultureInfo.CurrentUICulture;
        var cacheKey = $"{_academyContentRoot}|{culture.Name}";
        return Cache.GetOrAdd(cacheKey, _ => LoadArticles(culture));
    }

    private IReadOnlyList<Article> LoadArticles(CultureInfo culture)
    {
        var cultureNames = GetCultureFallbacks(culture).ToList();
        var articleIds = cultureNames
            .Select(cultureName => Path.Combine(_academyContentRoot, cultureName))
            .Where(Directory.Exists)
            .SelectMany(directory => Directory.EnumerateFiles(directory, "*.md"))
            .Select(path => Path.GetFileNameWithoutExtension(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return articleIds
            .Select(id => LoadArticle(id, cultureNames))
            .Where(article => article is not null)
            .Cast<Article>()
            .ToList();
    }

    private Article? LoadArticle(string articleId, IReadOnlyList<string> cultureNames)
    {
        foreach (var cultureName in cultureNames)
        {
            var path = Path.Combine(_academyContentRoot, cultureName, $"{articleId}.md");
            if (!File.Exists(path))
                continue;

            var document = MarkdownArticleDocument.Parse(File.ReadAllText(path));
            var id = document.GetString("id", articleId);
            var bodyHtml = Markdown.ToHtml(document.BodyMarkdown, MarkdownPipeline);

            return new Article
            {
                Id = id,
                Title = document.GetString("title", id),
                Summary = document.GetString("summary", string.Empty),
                Category = document.GetString("category", ArticleCategories.Fundamentals),
                SourceModule = document.GetString("sourceModule", "Markdown"),
                Tags = document.GetList("tags"),
                BodyBlocks = document.BodyMarkdown
                    .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
                    .Select(block => block.Trim())
                    .Where(block => !string.IsNullOrWhiteSpace(block))
                    .ToList(),
                BodyHtml = bodyHtml,
                References = document.GetReferences(),
                ContentKind = document.GetEnum("contentKind", ArticleContentKind.Generic),
                SortOrder = document.GetInt("sortOrder", 100)
            };
        }

        return null;
    }

    private static IEnumerable<string> GetCultureFallbacks(CultureInfo culture)
    {
        if (!string.IsNullOrWhiteSpace(culture.Name))
            yield return culture.Name;

        if (!string.IsNullOrWhiteSpace(culture.TwoLetterISOLanguageName)
            && !string.Equals(culture.TwoLetterISOLanguageName, culture.Name, StringComparison.OrdinalIgnoreCase))
        {
            yield return culture.TwoLetterISOLanguageName;
        }

        yield return "en";
        yield return "default";
    }

    private sealed class MarkdownArticleDocument
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<string>> _lists = new(StringComparer.OrdinalIgnoreCase);

        public string BodyMarkdown { get; private set; } = string.Empty;

        public static MarkdownArticleDocument Parse(string raw)
        {
            var document = new MarkdownArticleDocument();
            var normalized = raw.Replace("\r\n", "\n", StringComparison.Ordinal);

            if (!normalized.StartsWith("---\n", StringComparison.Ordinal))
            {
                document.BodyMarkdown = raw;
                return document;
            }

            var frontmatterEnd = normalized.IndexOf("\n---\n", 4, StringComparison.Ordinal);
            if (frontmatterEnd < 0)
            {
                document.BodyMarkdown = raw;
                return document;
            }

            var frontmatter = normalized[4..frontmatterEnd];
            document.BodyMarkdown = normalized[(frontmatterEnd + 5)..].Trim();
            document.ParseFrontmatter(frontmatter);
            return document;
        }

        public string GetString(string key, string fallback)
        {
            return _values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : fallback;
        }

        public IReadOnlyList<string> GetList(string key)
        {
            return _lists.TryGetValue(key, out var values)
                ? values.Where(value => !string.IsNullOrWhiteSpace(value)).ToList()
                : Array.Empty<string>();
        }

        public IReadOnlyList<ContentReference> GetReferences()
        {
            return GetList("references")
                .Select(ParseReference)
                .Where(reference => !string.IsNullOrWhiteSpace(reference.Title)
                    && !string.IsNullOrWhiteSpace(reference.Url))
                .ToList();
        }

        public int GetInt(string key, int fallback)
        {
            return int.TryParse(GetString(key, string.Empty), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value
                : fallback;
        }

        public TEnum GetEnum<TEnum>(string key, TEnum fallback)
            where TEnum : struct
        {
            return Enum.TryParse<TEnum>(GetString(key, string.Empty), ignoreCase: true, out var value)
                ? value
                : fallback;
        }

        private void ParseFrontmatter(string frontmatter)
        {
            string? currentList = null;

            foreach (var rawLine in frontmatter.Split('\n'))
            {
                var line = rawLine.TrimEnd();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var trimmed = line.TrimStart();
                if (trimmed.StartsWith("- ", StringComparison.Ordinal) && currentList is not null)
                {
                    AddListValue(currentList, Unquote(trimmed[2..].Trim()));
                    continue;
                }

                var separatorIndex = line.IndexOf(':', StringComparison.Ordinal);
                if (separatorIndex < 0)
                    continue;

                var key = line[..separatorIndex].Trim();
                var value = Unquote(line[(separatorIndex + 1)..].Trim());

                if (string.IsNullOrWhiteSpace(value))
                {
                    currentList = key;
                    if (!_lists.ContainsKey(key))
                        _lists[key] = new List<string>();
                }
                else
                {
                    currentList = null;
                    _values[key] = value;
                }
            }
        }

        private void AddListValue(string key, string value)
        {
            if (!_lists.TryGetValue(key, out var values))
            {
                values = new List<string>();
                _lists[key] = values;
            }

            values.Add(value);
        }

        private static ContentReference ParseReference(string value)
        {
            var parts = value.Split('|', StringSplitOptions.TrimEntries);
            return new ContentReference
            {
                Title = parts.ElementAtOrDefault(0) ?? string.Empty,
                Url = parts.ElementAtOrDefault(1) ?? string.Empty,
                SourceType = parts.ElementAtOrDefault(2) ?? "study"
            };
        }

        private static string Unquote(string value)
        {
            return value.Length >= 2
                && ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\''))
                ? value[1..^1]
                : value;
        }
    }
}
