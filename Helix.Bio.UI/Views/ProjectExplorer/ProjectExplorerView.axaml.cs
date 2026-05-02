using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using Helix.Bio.UI.ViewModels;

namespace Helix.Bio.UI.Views;

public partial class ProjectExplorerView : UserControl
{
    public ProjectExplorerView()
    {
        InitializeComponent();
    }

    private void TreeView_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source is Control control)
        {
            if (control.DataContext is FileSystemNode clickedNode)
            {
                if (!clickedNode.IsDirectory)
                {
                    WeakReferenceMessenger.Default.Send(new OpenFileMessage(clickedNode.FullPath));
                    e.Handled = true;
                }
            }
        }
    }
}