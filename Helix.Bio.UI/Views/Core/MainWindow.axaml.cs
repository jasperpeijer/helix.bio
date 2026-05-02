using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Helix.Bio.UI.ViewModels;

namespace Helix.Bio.UI.Views;

public partial class MainWindow : Window
{
    private WorkspaceTabViewModel? _draggedTab;

    public MainWindow()
    {
        InitializeComponent();
        
        // We ONLY need this for physical vertical mouse wheels now. 
        // Your trackpad handles horizontal scrolling natively now!
        WorkspaceTabControl.AddHandler(InputElement.PointerWheelChangedEvent, WorkspaceTabs_PointerWheelChanged, RoutingStrategies.Tunnel);
    }

    // ========================================================
    // 1. VERTICAL MOUSE WHEEL CONVERSION
    // ========================================================
    private void WorkspaceTabs_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        // Only trigger if it's a vertical scroll
        if (e.Delta.Y != 0)
        {
            // SAFETY CHECK: Ensure the mouse is hovering over the top 50 pixels (the tab strip area)
            var point = e.GetCurrentPoint(WorkspaceTabControl).Position;
            if (point.Y < 50) 
            {
                // Grab the VERY FIRST ScrollViewer (which is now the one we injected into the XAML)
                var tabScrollViewer = WorkspaceTabControl.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                
                if (tabScrollViewer != null)
                {
                    // Convert the vertical math to horizontal movement
                    double newOffset = tabScrollViewer.Offset.X - (e.Delta.Y * 50);
                    tabScrollViewer.Offset = new Vector(Math.Max(0, newOffset), tabScrollViewer.Offset.Y);
                    e.Handled = true; 
                }
            }
        }
    }

    // ========================================================
    // 2. STARTING THE DRAG
    // ========================================================
    private async void TabItem_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;
        
        if (properties.IsLeftButtonPressed && sender is Control control && control.DataContext is WorkspaceTabViewModel draggedTab)
        {
            _draggedTab = draggedTab;
            var emptyDragData = new DataTransfer();
            await DragDrop.DoDragDropAsync(e, emptyDragData, DragDropEffects.Move);
            _draggedTab = null;
        }
    }

    // ========================================================
    // 3. DROPPING AND REORDERING
    // ========================================================
    private void WorkspaceTabs_Drop(object? sender, DragEventArgs e)
    {
        if (_draggedTab != null && 
            e.Source is Control dropTarget && 
            dropTarget.DataContext is WorkspaceTabViewModel targetTab &&
            _draggedTab != targetTab)
        {
            var vm = (MainWindowViewModel)DataContext!;
            
            int oldIndex = vm.OpenTabs.IndexOf(_draggedTab);
            int newIndex = vm.OpenTabs.IndexOf(targetTab);

            if (oldIndex >= 0 && newIndex >= 0)
            {
                vm.OpenTabs.Move(oldIndex, newIndex);
                vm.ActiveTab = _draggedTab; 
            }
        }
    }
    
    private void WorkspaceTabs_DragOver(object? sender, DragEventArgs e)
    {
        // Only run this math if we are actively dragging a tab
        if (_draggedTab != null)
        {
            var tabScrollViewer = WorkspaceTabControl.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
            
            if (tabScrollViewer != null)
            {
                // Get the exact X/Y pixel coordinate of the mouse relative to the scrolling area
                var position = e.GetPosition(tabScrollViewer);
                
                double edgeZone = 40;      // Trigger scroll when mouse is 40px from the edge
                double scrollSpeed = 15;   // How many pixels to jump per frame

                // Check if mouse is pushing against the LEFT edge
                if (position.X < edgeZone)
                {
                    double newOffset = tabScrollViewer.Offset.X - scrollSpeed;
                    tabScrollViewer.Offset = new Vector(Math.Max(0, newOffset), tabScrollViewer.Offset.Y);
                }
                // Check if mouse is pushing against the RIGHT edge
                else if (position.X > tabScrollViewer.Bounds.Width - edgeZone)
                {
                    double newOffset = tabScrollViewer.Offset.X + scrollSpeed;
                    tabScrollViewer.Offset = new Vector(newOffset, tabScrollViewer.Offset.Y);
                }
            }
        }
    }
}