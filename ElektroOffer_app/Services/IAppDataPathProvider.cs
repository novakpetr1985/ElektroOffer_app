namespace ElektroOffer_app.Services;

/// <summary>
/// Provides writable per-user storage locations used by the desktop application.
/// </summary>
public interface IAppDataPathProvider
{
    string DatabasePath { get; }
}
