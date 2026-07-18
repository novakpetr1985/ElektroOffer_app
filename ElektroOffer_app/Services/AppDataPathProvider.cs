using System.IO;

namespace ElektroOffer_app.Services;

/// <summary>
/// Stores mutable application data outside the installation directory and safely
/// adopts a legacy database on first use without overwriting an existing user file.
/// </summary>
public sealed class AppDataPathProvider : IAppDataPathProvider
{
    private const string ApplicationDirectoryName = "ElektroOffer";
    private const string DatabaseFileName = "elektrooffer.db";
    private readonly string _dataDirectory;
    private readonly IReadOnlyList<string> _legacyCandidates;

    public AppDataPathProvider()
        : this(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ApplicationDirectoryName),
            FindLegacyDatabaseCandidates())
    {
    }

    public AppDataPathProvider(string dataDirectory, IEnumerable<string>? legacyCandidates = null)
    {
        _dataDirectory = dataDirectory;
        _legacyCandidates = legacyCandidates?.ToArray() ?? [];
    }

    public string DatabasePath
    {
        get
        {
            Directory.CreateDirectory(_dataDirectory);
            var destination = Path.Combine(_dataDirectory, DatabaseFileName);
            AdoptLegacyDatabase(destination);
            return destination;
        }
    }

    private void AdoptLegacyDatabase(string destination)
    {
        if (File.Exists(destination))
            return;

        var legacyPath = _legacyCandidates
            .Select(Path.GetFullPath)
            .FirstOrDefault(path =>
                !string.Equals(path, destination, StringComparison.OrdinalIgnoreCase) &&
                File.Exists(path));

        if (legacyPath != null)
            File.Copy(legacyPath, destination, overwrite: false);
    }

    private static IReadOnlyList<string> FindLegacyDatabaseCandidates()
    {
        var candidates = new List<string>
        {
            Path.Combine(AppContext.BaseDirectory, DatabaseFileName),
            Path.Combine(Environment.CurrentDirectory, DatabaseFileName)
        };

        AddProjectDatabaseCandidate(candidates, AppContext.BaseDirectory);
        AddProjectDatabaseCandidate(candidates, Environment.CurrentDirectory);
        return candidates.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void AddProjectDatabaseCandidate(ICollection<string> candidates, string startPath)
    {
        var directory = new DirectoryInfo(startPath);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ElektroOffer_app.csproj")))
            {
                candidates.Add(Path.Combine(directory.FullName, DatabaseFileName));
                return;
            }

            directory = directory.Parent;
        }
    }
}
