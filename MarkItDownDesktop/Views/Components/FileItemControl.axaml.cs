using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MarkItDownDesktop.Views.Components;

public partial class FileItemControl : UserControl
{
    public FileItemControl()
    {
        InitializeComponent();
    }

    public bool IsOutbox
    {
        get => GetValue(IsOutboxProperty);
        set => SetValue(IsOutboxProperty, value);
    }

    public static readonly StyledProperty<bool> IsOutboxProperty =
        AvaloniaProperty.Register<FileItemControl, bool>(nameof(IsOutbox));
}