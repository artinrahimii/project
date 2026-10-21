using System.Windows;
namespace WhiteP.Host.Windows;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LifecycleState.Text = "Stopped";
    }
}
