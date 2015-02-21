using System.Windows;

namespace SorcererSoftware {
   public partial class App : Application {
      protected override void OnStartup(StartupEventArgs e) {
         base.OnStartup(e);
         string defaultFile = (e.Args.Length == 1) ? e.Args[0] : "AutoRun.rb";
         MainWindow = new MainWindow(defaultFile);
         MainWindow.Show();
      }
   }
}
