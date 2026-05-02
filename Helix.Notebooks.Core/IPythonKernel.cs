using Python.Runtime;

namespace Helix.Notebooks.Core;

/// <summary>
/// Defines the core execution and package management capabilities 
/// required by the Helix Studio environment.
/// </summary>
public interface IPythonKernel : IDisposable
{
    /// <summary>
    /// Creates an isolated execution scope for a specific notebook tab.
    /// </summary>
    PyModule CreateScope();

    /// <summary>
    /// Executes Python code within a specific scope and captures the output.
    /// </summary>
    string Execute(string pythonCode, PyModule scope, string? notebookDir, string? workspaceDir);

    /// <summary>
    /// Updates the working directory of the kernel to match the file being edited.
    /// </summary>
    Task SetWorkingDirectoryAsync(string fullFilePath, PyModule scope, string? workspaceRoot);

    /// <summary>
    /// Scans the environment to list all currently installed PyPI packages.
    /// </summary>
    Dictionary<string, string> GetInstalledPackages();

    /// <summary>
    /// Installs a new package via pip into the global bundled engine.
    /// </summary>
    Task<string> InstallPackagesAsync(string packageName);
}