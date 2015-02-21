using Microsoft.Scripting.Hosting;
using System.Linq;
using System.Windows;

namespace SorcererSoftware {
   public static class Dlr {
      static readonly ScriptScope _scope;

      public static readonly dynamic Scope;

      static Dlr() {
         _scope = IronRuby.Ruby.CreateEngine().CreateScope();
         // by default, include everything usually included for Wpf Controls
         string script = @"
require '" + typeof(DependencyObject).Assembly.FullName /* WindowsBase */ + @"'
require '" + typeof(Enumerable).Assembly.FullName /* System.Core */ + @"'
require '" + typeof(Application).Assembly.FullName /* PresentationFramework */ + @"'
require '" + typeof(DataFormats).Assembly.FullName /* PresentationCore */ + @"'
require 'WpfRuby'
include System
include System::Collections::Generic
include System::Linq
include System::Text
include System::Threading::Tasks
include System::Windows
include System::Windows::Controls
include System::Windows::Data
include System::Windows::Documents
include System::Windows::Input
include System::Windows::Media
include System::Windows::Media::Imaging
include System::Windows::Navigation
include System::Windows::Shapes
include SorcererSoftware
";
         Execute(script);
         Scope = _scope;
      }

      public static object Execute(string script) {
         return _scope.Engine.Execute(script, _scope);
      }

      public static object ExecuteFile(string file) {
         return _scope.Engine.ExecuteFile(file, _scope);
      }
   }
}
