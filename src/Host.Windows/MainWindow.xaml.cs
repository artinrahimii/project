using System.Windows;
namespace WhiteP.Host.Windows;
public partial class MainWindow : Window
{
 public MainWindow()
 {
  InitializeComponent();
  var status=new HostStartup(new HostLifecycle(),new UnconfiguredInferenceBackend()).Start();
  LifecycleState.Text=$"{status.State}: {status.Message}";
 }
}
