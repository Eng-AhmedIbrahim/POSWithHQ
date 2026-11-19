using System.Windows;

namespace HQ.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(IServiceProvider serviceProvider) : this()
    {
        blazorWebView.Services = serviceProvider;
    }
}
