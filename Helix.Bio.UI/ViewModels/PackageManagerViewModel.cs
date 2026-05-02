using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Helix.Bio.UI.Models;
using Helix.Notebooks.Core;

namespace Helix.Bio.UI.ViewModels;

[ObservableObject]
public partial class PackageManagerViewModel
{
    private readonly PythonKernel _kernel;

    [ObservableProperty] 
    private string _searchText = string.Empty;
    
    [ObservableProperty] 
    private string _terminalOutput = "Ready.";

    [ObservableProperty] 
    private bool _isBusy;

    public ObservableCollection<PythonPackage> InstalledPackages { get; } = new();

    public PackageManagerViewModel(PythonKernel kernel)
    {
        _kernel = kernel;
        _ = RefreshAsync();
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        IsBusy = true;
        TerminalOutput = "Refreshing package list...";

        try
        {
            var packages = await Task.Run(() => _kernel.GetInstalledPackages());

            InstalledPackages.Clear();

            foreach (var kvp in packages.OrderBy(x => x.Key))
            {
                InstalledPackages.Add(new PythonPackage
                {
                    Name = kvp.Key,
                    Version = kvp.Value
                });
            }

            TerminalOutput = $"Found {InstalledPackages.Count} packages.";
        }
        catch (Exception e)
        {
            TerminalOutput = $"Error refreshing packages: {e.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task InstallAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText)) return;

        IsBusy = true;

        string pkg = SearchText;
        TerminalOutput = $"Attempting to install '{pkg}' via pip...\n";
        string result = await _kernel.InstallPackagesAsync(pkg);

        TerminalOutput = result;

        if (result.Contains("SUCCESS"))
        {
            SearchText = string.Empty;
            await RefreshAsync();
        }
        
        IsBusy = false;
    }

}