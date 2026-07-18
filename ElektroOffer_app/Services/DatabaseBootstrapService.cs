using System.Data;
using System.IO;
using ElektroOffer_app.Data;
using Microsoft.EntityFrameworkCore;

namespace ElektroOffer_app.Services
{
    /// <summary>
    /// Připraví lokální SQLite databázi při startu aplikace.
    /// Pro verzi 1.9.0 je zdrojem pravdy SQL skript, ne ručně psaná seed data v C#.
    /// </summary>
    public static class DatabaseBootstrapService
    {
        private const string SeedScriptName = "elektrooffer_1_9_0.sql";

        public static void EnsureReady(AppDbContext db)
        {
            var scriptPath = ResolveSeedScriptPath();
            var script = File.ReadAllText(scriptPath);
            var dataMarker = script.IndexOf("-- 3) DATA", StringComparison.Ordinal);
            if (dataMarker < 0)
                throw new InvalidDataException($"Seed SQL skript {SeedScriptName} neobsahuje oddělovač dat.");

            // CREATE TABLE/INDEX IF NOT EXISTS is safe for both new and existing databases.
            ExecuteScript(db, script[..dataMarker]);

            if (HasCatalogData(db))
                return;

            ExecuteScript(db, "PRAGMA foreign_keys = ON;" + Environment.NewLine + script[dataMarker..]);
        }

        private static bool HasCatalogData(AppDbContext db)
        {
            return db.Tasks.Any() ||
                   db.Specifications.Any() ||
                   db.BaseMaterials.Any() ||
                   db.Positions.Any() ||
                   db.Materials.Any() ||
                   db.Categories.Any() ||
                   db.Suppliers.Any() ||
                   db.MaterialPrices.Any();
        }

        private static void ExecuteScript(AppDbContext db, string script)
        {
            var connection = db.Database.GetDbConnection();
            var closeAfterExecute = connection.State != ConnectionState.Open;

            if (closeAfterExecute)
                connection.Open();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = script;
                command.ExecuteNonQuery();
            }
            finally
            {
                if (closeAfterExecute)
                    connection.Close();
            }
        }

        private static string ResolveSeedScriptPath()
        {
            var candidates = new List<string>
            {
                Path.Combine(AppContext.BaseDirectory, "Data", "Seed", SeedScriptName),
                Path.Combine(Environment.CurrentDirectory, "Data", "Seed", SeedScriptName)
            };

            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            for (var i = 0; i < 8 && directory != null; i++, directory = directory.Parent)
            {
                candidates.Add(Path.Combine(directory.FullName, "ElektroOffer_app", "Data", "Seed", SeedScriptName));
                candidates.Add(Path.Combine(directory.FullName, "Data", "Seed", SeedScriptName));
            }

            var scriptPath = candidates.FirstOrDefault(File.Exists);
            if (scriptPath != null)
                return scriptPath;

            throw new FileNotFoundException(
                $"Seed SQL skript nebyl nalezen. Očekáván soubor {SeedScriptName}.",
                SeedScriptName);
        }
    }
}
