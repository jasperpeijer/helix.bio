using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Helix.Bio.UI.Converters;

public class FileIconConverter : IValueConverter
{
    public static readonly FileIconConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string fileName)
        {
            if (fileName.EndsWith(".ipynb", StringComparison.OrdinalIgnoreCase))
            {
                Application.Current!.TryFindResource("JupyterIcon", out var jupyterResource);

                return jupyterResource;
            }

            Application.Current!.TryFindResource("TextFileIcon", out var textResource);

            return textResource;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}