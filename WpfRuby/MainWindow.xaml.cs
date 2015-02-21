using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SorcererSoftware {
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window {

      public static readonly ICommand DebugCommand = new RoutedUICommand("Debug", "Debug", typeof(MainWindow));
      public static readonly ICommand RunPromptCommand = new RoutedUICommand("Evaluate", "RumPrompt", typeof(MainWindow));

      readonly AppHelper _appHelper;
      string _autoStart;

      public MainWindow(string autoStart) {
         InitializeComponent();
         _autoStart = autoStart;
         _appHelper = new AppHelper(Body);
         Dlr.Scope.app = _appHelper;
         _appHelper.SendDebugText += (sender, e) => Print(e.Value);
         _appHelper.Watch(_autoStart, Dlr.ExecuteFile);
      }

      #region Events

      protected override void OnDrop(DragEventArgs e) {
         base.OnDrop(e);
         if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
         var data = (string[])e.Data.GetData(DataFormats.FileDrop);
         _appHelper.RemoveWatch(_autoStart);
         _autoStart = data[0];
         _appHelper.Watch(_autoStart, Dlr.ExecuteFile);
      }

      #endregion

      #region Commands

      void DebugExecute(object sender, EventArgs e) {
         DebugColumn.Width = new GridLength(1 - DebugColumn.Width.Value, GridUnitType.Star);
         if (DebugColumn.Width.Value == 1) {
            Keyboard.Focus(DebugExpression);
            DebugResults.CaretIndex = DebugResults.Text.Length;
            DebugResults.ScrollToEnd();
         }
      }

      void RunPromptExecute(object sender, EventArgs e) {
         try {
            var result = Dlr.Execute(DebugExpression.Text);
            string output = ProcessObject(result);
            Print(output);
         } catch (Exception ex) {
            Print("Error:");
            while (ex != null) {
               Print(ex.Message);
               ex = ex.InnerException;
            }
         }
      }

      #endregion

      #region Helpers

      string ProcessObject(object input) {
         if (input is string) return (string)input;
         if (input is IEnumerable) {
            var list = (IEnumerable)input;
            string output = string.Empty;
            foreach (var item in list) {
               output += ProcessObject(item) + Environment.NewLine;
            }
            return output;
         }
         return input.ToString();
      }

      void Print(string str) {
         DebugResults.Text += str + Environment.NewLine;
         DebugResults.CaretIndex = DebugResults.Text.Length;
         DebugResults.ScrollToEnd();
      }

      #endregion
   }
}
