using System;
using System.Windows;
using System.Windows.Controls;

namespace SorcererSoftware {
   public class RubyControl : UserControl {
      readonly dynamic _appContext;
      string _currentFile;

      public RubyControl(dynamic context) { _appContext = context; }

      public void InitializeContent(string fileName, Action<RubyControl> initAction) {
         // get the app helper as a dynamic.
         // RubyControls are made in Ruby's Context, where as the app helper came from
         // the exe's default context. Casting to the AppHelper class in this context would fail.
         if (_currentFile != null) _appContext.RemoveWatch(_currentFile);
         _currentFile = fileName;
         Action<string> fullAction = s => {
            Content = _appContext.LoadXaml(s);
            initAction(this);
         };
         _appContext.Watch(_currentFile, fullAction);
      }

      /// <summary>
      /// This method resolves dynamic calls to the Control from Ruby.
      /// It allows you to access Named Xaml elements from ruby.
      /// </summary>
      public object method_missing(object name) {
         return LogicalTreeHelper.FindLogicalNode(this, name.ToString());
      }
   }
}
