using System.Globalization;
using System.Text.RegularExpressions;

namespace RoboCopyGui;

internal readonly record struct ProgressSnapshot(
    bool Running,
    bool Finished,
    bool Failed,
    bool Cancelled,
    bool ForceComplete,
    long TotalBytes,
    long CompletedBytes,
    int EstimatedFiles,
    int CompletedFiles,
    long CurrentFileBytes,
    double CurrentFileFraction,
    bool CurrentFileAccounted,
    string CurrentFileName,
    string Eta);

internal sealed record WorkEstimate(Dictionary<string, long> Sizes, long TotalBytes, int FileCount);

internal sealed class ScanRequest
{
    public required string Source { get; init; }

    public bool Recurse { get; init; }

    public int MaxLevels { get; init; }

    public bool ArchiveOnly { get; init; }

    public bool SkipDirectoryLinks { get; init; }

    public bool SkipFileLinks { get; init; }

    public long? MaxBytes { get; init; }

    public long? MinBytes { get; init; }

    public IReadOnlyList<string> IncludePatterns { get; init; } = [];

    public IReadOnlyList<string> ExcludeFilePatterns { get; init; } = [];

    public IReadOnlyList<string> ExcludeDirectoryPatterns { get; init; } = [];
}

internal static class RobocopyWorkEstimator
{
    public static WorkEstimate Estimate(ScanRequest request, CancellationToken cancellationToken)
    {
        var sizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        long totalBytes = 0;
        var fileCount = 0;

        if (Directory.Exists(request.Source))
        {
            Walk(request.Source, 1);
        }

        return new WorkEstimate(sizes, totalBytes, fileCount);

        void Walk(string directory, int level)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(directory);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return;
            }

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var info = new FileInfo(file);
                    if (request.SkipFileLinks && info.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        continue;
                    }

                    if (request.ArchiveOnly && !info.Attributes.HasFlag(FileAttributes.Archive))
                    {
                        continue;
                    }

                    if (!IsIncluded(info.Name, request.IncludePatterns))
                    {
                        continue;
                    }

                    if (IsExcluded(info.Name, info.FullName, request.ExcludeFilePatterns))
                    {
                        continue;
                    }

                    if (request.MaxBytes is long maxBytes && info.Length > maxBytes)
                    {
                        continue;
                    }

                    if (request.MinBytes is long minBytes && info.Length < minBytes)
                    {
                        continue;
                    }

                    sizes[Path.GetFullPath(info.FullName)] = info.Length;
                    totalBytes += info.Length;
                    fileCount++;
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                {
                    // Keep estimating the files that can be read.
                }
            }

            if (!request.Recurse || (request.MaxLevels > 0 && level >= request.MaxLevels))
            {
                return;
            }

            IEnumerable<string> directories;
            try
            {
                directories = Directory.EnumerateDirectories(directory);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return;
            }

            foreach (var subdirectory in directories)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var attributes = File.GetAttributes(subdirectory);
                    if (request.SkipDirectoryLinks && attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        continue;
                    }

                    var name = Path.GetFileName(subdirectory);
                    if (IsExcluded(name, subdirectory, request.ExcludeDirectoryPatterns))
                    {
                        continue;
                    }

                    Walk(subdirectory, level + 1);
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                {
                    // Skip folders that cannot be inspected.
                }
            }
        }
    }

    private static bool IsIncluded(string fileName, IReadOnlyList<string> patterns)
    {
        if (patterns.Count == 0 || patterns.Any(pattern => pattern is "*" or "*.*"))
        {
            return true;
        }

        return patterns.Any(pattern => WildcardMatch(fileName, pattern));
    }

    private static bool IsExcluded(string name, string fullPath, IReadOnlyList<string> patterns)
    {
        foreach (var pattern in patterns)
        {
            if (pattern.Contains('\\') || pattern.Contains(':'))
            {
                var normalizedPattern = NormalizePath(pattern);
                var normalizedFullPath = NormalizePath(fullPath);
                if (normalizedFullPath.Equals(normalizedPattern, StringComparison.OrdinalIgnoreCase) ||
                    normalizedFullPath.StartsWith(normalizedPattern + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            if (WildcardMatch(name, pattern) || WildcardMatch(fullPath, pattern))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizePath(string path)
    {
        var trimmed = path.Trim().TrimEnd('\\', '/');

        try
        {
            return Path.GetFullPath(trimmed);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return trimmed;
        }
    }

    internal static bool WildcardMatch(string text, string pattern)
    {
        if (pattern is "*" or "*.*")
        {
            return true;
        }

        var expression = "^" + Regex.Escape(pattern).Replace("\\*", ".*", StringComparison.Ordinal).Replace("\\?", ".", StringComparison.Ordinal) + "$";
        return Regex.IsMatch(text, expression, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}

/// <summary>
/// Tracks overall copy progress from Robocopy's console output.
/// Redirected Robocopy writes each percentage on its own line.
/// </summary>
internal sealed class RobocopyProgressTracker
{
    private static readonly Regex PercentRegex = new(
        @"^\s*(?<pct>\d{1,3}(?:[.,]\d+)?)%\s*(?<rest>.*)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex EntryRegex = new(
        @"^\s*(?<class>\*EXTRA File|\*EXTRA Dir|New File|New Dir|Newer|Older|Same|Tweaked|Modified|Changed|Lonely)\s+(?<size>\d[\d,]*(?:\.\d+)?)(?:\s+(?<unit>[kmgKMG]))?\s+(?<name>.+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex TimeSuffixRegex = new(
        @"\s+(?:\d{4}/\d{1,2}/\d{1,2}\s+)?\d{1,2}:\d{2}(?::\d{2})?(?:\s*->\s*(?:\d{4}/\d{1,2}/\d{1,2}\s+)?\d{1,2}:\d{2}(?::\d{2})?)?\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly object _gate = new();
    private Dictionary<string, long> _sizes = new(StringComparer.OrdinalIgnoreCase);

    private bool _running;
    private bool _finished;
    private bool _failed;
    private bool _cancelled;
    private bool _forceComplete;
    private long _totalBytes;
    private long _completedBytes;
    private int _estimatedFiles;
    private int _completedFiles;
    private long _currentFileBytes;
    private double _currentFileFraction;
    private bool _currentFileAccounted;
    private string _currentFileName = "";
    private string _currentFilePath = "";
    private string _currentDirectory = "";
    private string _eta = "";

    public void Reset()
    {
        lock (_gate)
        {
            _sizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            _running = false;
            _finished = false;
            _failed = false;
            _cancelled = false;
            _forceComplete = false;
            _totalBytes = 0;
            _completedBytes = 0;
            _estimatedFiles = 0;
            _completedFiles = 0;
            _currentFileBytes = 0;
            _currentFileFraction = 0;
            _currentFileAccounted = false;
            _currentFileName = "";
            _currentFilePath = "";
            _currentDirectory = "";
            _eta = "";
        }
    }

    public void MarkRunning()
    {
        lock (_gate)
        {
            _running = true;
            _finished = false;
            _failed = false;
            _cancelled = false;
            _forceComplete = false;
        }
    }

    public void MarkCancelled()
    {
        lock (_gate)
        {
            _cancelled = true;
            _running = false;
        }
    }

    public void MarkFinished(bool success)
    {
        lock (_gate)
        {
            if (_cancelled)
            {
                _running = false;
                return;
            }

            if (success)
            {
                CommitCurrentFile();
            }

            _finished = true;
            _failed = !success;
            _running = false;
            _forceComplete = success;

            if (success && _totalBytes > 0)
            {
                _completedBytes = _totalBytes;
            }
        }
    }

    public void SetEstimate(WorkEstimate estimate)
    {
        lock (_gate)
        {
            _sizes = estimate.Sizes;
            _totalBytes = estimate.TotalBytes;
            _estimatedFiles = estimate.FileCount;

            if (!_currentFileAccounted &&
                _sizes.TryGetValue(_currentFilePath, out var exactSize))
            {
                _currentFileBytes = exactSize;
            }
        }
    }

    /// <returns>True when the line is a percentage tick and can be left out of the log.</returns>
    public bool ApplyOutputLine(string line)
    {
        lock (_gate)
        {
            var percent = PercentRegex.Match(line);
            if (percent.Success)
            {
                var raw = percent.Groups["pct"].Value.Replace(',', '.');
                if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                {
                    _currentFileFraction = Math.Clamp(value / 100d, 0d, 1d);
                    _eta = percent.Groups["rest"].Value.Trim();

                    if (_currentFileFraction >= 1d)
                    {
                        CommitCurrentFile();
                    }
                }

                return true;
            }

            var entry = EntryRegex.Match(line);
            if (!entry.Success)
            {
                return false;
            }

            var entryClass = entry.Groups["class"].Value;
            var displayedSize = entry.Groups["size"].Value;
            if (entry.Groups["unit"].Success)
            {
                displayedSize += entry.Groups["unit"].Value;
            }

            var name = CleanName(entry.Groups["name"].Value);
            var isDirectory = entryClass.Contains("Dir", StringComparison.OrdinalIgnoreCase) ||
                              name.EndsWith('\\') ||
                              name.EndsWith('/');

            if (isDirectory)
            {
                CommitCurrentFile();
                _currentFileName = "";
                _currentFilePath = "";
                _currentFileBytes = 0;
                _currentFileFraction = 0;
                _currentFileAccounted = false;
                _eta = "";
                _currentDirectory = NormalizePath(name);
                return false;
            }

            BeginFile(name, displayedSize);
            return false;
        }
    }

    public ProgressSnapshot Snapshot()
    {
        lock (_gate)
        {
            return new ProgressSnapshot(
                _running,
                _finished,
                _failed,
                _cancelled,
                _forceComplete,
                _totalBytes,
                _completedBytes,
                _estimatedFiles,
                _completedFiles,
                _currentFileBytes,
                _currentFileFraction,
                _currentFileAccounted,
                _currentFileName,
                _eta);
        }
    }

    private void BeginFile(string name, string displayedSize)
    {
        CommitCurrentFile();

        _currentFileAccounted = false;
        _currentFileFraction = 0;
        _eta = "";
        _currentFileName = Path.GetFileName(name.TrimEnd('\\', '/'));

        _currentFilePath = IsFullPath(name)
            ? NormalizePath(name)
            : NormalizePath(Path.Combine(_currentDirectory, name));

        if (_sizes.TryGetValue(_currentFilePath, out var exactSize))
        {
            _currentFileBytes = exactSize;
        }
        else if (TryParseSize(displayedSize, out var parsedSize))
        {
            _currentFileBytes = parsedSize;
        }
        else
        {
            _currentFileBytes = 0;
        }
    }

    private void CommitCurrentFile()
    {
        if (_currentFileAccounted || string.IsNullOrEmpty(_currentFileName))
        {
            return;
        }

        _completedBytes += _currentFileBytes;
        _completedFiles++;
        _currentFileFraction = 1;
        _currentFileAccounted = true;
    }

    private static string CleanName(string name)
    {
        return TimeSuffixRegex.Replace(name.Trim(), "").Trim();
    }

    private static string NormalizePath(string path)
    {
        var trimmed = path.Trim().TrimEnd('\\', '/');

        try
        {
            return Path.GetFullPath(trimmed);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return trimmed;
        }
    }

    private static bool IsFullPath(string value)
    {
        return value.StartsWith(@"\\", StringComparison.Ordinal) ||
               (value.Length >= 3 && value[1] == ':' && (value[2] == '\\' || value[2] == '/'));
    }

    internal static bool TryParseSize(string text, out long bytes)
    {
        bytes = 0;
        var cleaned = text.Trim().Replace(",", "", StringComparison.Ordinal).Replace(" ", "", StringComparison.Ordinal);
        if (cleaned.Length == 0)
        {
            return false;
        }

        double factor = 1;
        var suffix = cleaned[^1];
        if (suffix is 'k' or 'K' or 'm' or 'M' or 'g' or 'G' or 't' or 'T')
        {
            factor = char.ToLowerInvariant(suffix) switch
            {
                'k' => 1024d,
                'm' => 1024d * 1024d,
                'g' => 1024d * 1024d * 1024d,
                't' => 1024d * 1024d * 1024d * 1024d,
                _ => 1d
            };
            cleaned = cleaned[..^1];
        }

        if (!double.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            return false;
        }

        bytes = (long)Math.Round(value * factor);
        return bytes >= 0;
    }
}
