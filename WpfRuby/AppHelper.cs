using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Reflection;

namespace SorcererSoftware {
   public class AppHelper {
      readonly IDictionary<string, Action<string>> _observeActions = new Dictionary<string, Action<string>>();
      readonly FileSystemWatcher _watcher = new FileSystemWatcher();
      readonly Dispatcher _dispatcher;
      readonly Decorator _body;
      string _appResourceSource;

      public event EventHandler<ValueEventArgs<string>> SendOutputText;
      public event EventHandler RequestClear;

      public UIElement WindowContent {
         get { return _body.Child; }
         set { _body.Child = value; }
      }

      public AppHelper(Decorator body) {
         _body = body;
         _dispatcher = body.Dispatcher;
         ExceptionHandler.App = this;
         _watcher.Path = AppDomain.CurrentDomain.BaseDirectory;
         _watcher.IncludeSubdirectories = true;
         _watcher.Created += OnChanged;
         _watcher.Changed += OnChanged;
         _watcher.Renamed += OnChanged;
         _watcher.EnableRaisingEvents = true;
      }

      /// <summary>
      /// Given a file and an action, causes the action to be called any time the file changes.
      /// It will also call the function once immediately if the file exists.
      /// Only one function can be added for each file.
      /// </summary>
      public void Watch(string file, Action<string> action) {
         _observeActions[file] = action;
         if (File.Exists(file)) Update(file);
      }

      /// <summary>
      /// Stops watching a given file. This is not needed if you're _changing_ the watch action, only if you want to stop watching for some reason.
      /// </summary>
      /// <param name="file"></param>
      public void RemoveWatch(string file) {
         if (!_observeActions.ContainsKey(file)) return;
         _observeActions.Remove(file);
      }

      public void Handle(UIElement element, RoutedEvent re, RoutedEventHandler action) {
         RoutedEventHandler wrapper = (o,e) => {
            ExceptionHandler.Try(() => {
               action(o, e);
            }, "Error running event handler for " + re.Name);
         };
         element.AddHandler(re, wrapper);
      }

      public object Print(string str) {
         if (SendOutputText != null) SendOutputText(this, str);
         return null;
      }

      public void Clear() {
         if (RequestClear != null) RequestClear(this, EventArgs.Empty);
      }

      /// <summary>
      /// This has to exist so we can load xaml in the context of the original application.
      /// This keeps all the statics and globals in the same app domain.
      /// </summary>
      public object LoadXaml(string file) {
         var content = File.ReadAllText(file);
         var parsed = XamlReader.Parse(content);
         return parsed;
      }

      public void UpdateApplicationResources(string xamlfile) {
         if (_appResourceSource != null) RemoveWatch(_appResourceSource);
         _appResourceSource = xamlfile;
         Watch(xamlfile, file => {
            var dictionary = (ResourceDictionary)LoadXaml(file);
            var appResources = Application.Current.Resources.MergedDictionaries;
            appResources.Clear();
            appResources.Add(dictionary);
         });
      }

      #region Lookups

      public string LookupStatics(string classname) {
         var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.Name == classname);
         string output = "";
         string line = Environment.NewLine;
         foreach (var type in types) {
            output += type.FullName + ":" + line;
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static).OrderBy(m => m.Name)) {
               if (method.Name[0] != method.Name.ToUpper()[0]) continue;
               output += "  " + method.Name + line;
               foreach (var parameter in method.GetParameters()) output += "    " + parameter.ParameterType + line;
            }
            output += line;
         }
         return output;
      }

      public string FindTypes(string partialnames) {
         string output = "";
         string line = Environment.NewLine;
         var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
               partialnames.ToLower().Split(' ').All(type.FullName.ToLower().Contains));
         foreach (var type in types) {
            output += type.FullName + line;
         }
         return output;
      }

      public string LookupProperties(string classname) {
         var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.Name == classname);
         string output = "";
         string line = Environment.NewLine;
         foreach (var type in types) {
            output += type.FullName + ":" + line;
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(p => p.Name)) {
               if (property.Name[0] != property.Name.ToUpper()[0]) continue;
               output += "  " + property.Name + line;
            }
            output += line;
         }
         return output;
      }

      public string LookupEnum(string classname) {
         var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.Name == classname);
         string output = "";
         string line = Environment.NewLine;
         foreach (var type in types) {
            if (!type.IsEnum) continue;
            output += type.FullName + ":" + line;
            foreach (var member in type.GetEnumNames()) output += "  " + member + line;
            output += line;
         }
         return output;
      }

      public string LookupMethods(string classname) {
         var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.Name == classname);
         string output = "";
         string line = Environment.NewLine;
         foreach (var type in types) {
            output += type.FullName + ":" + line;
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance).OrderBy(m => m.Name)) {
               if (method.Name[0] != method.Name.ToUpper()[0]) continue;
               output += "  " + method.Name + line;
               foreach (var parameter in method.GetParameters()) output += "    " + parameter.ParameterType + line;
            }
            output += line;
         }
         return output;
      }

      #endregion

      /// <summary>
      /// Provides a list of all Ruby helper functions in the AppHelper, along with their usage.
      /// Note that this is not a list of all public methods - it's just a list of members meant for use from Ruby.
      /// </summary>
      public string Help() {
         return @"
WindowContent
  sets the content of the main window
  example:
    r = Rectangle.new
    r.Fill = Brushes.Green
    app.WindowContent = r

Watch 'file', ->(f){}
  Watches a file.
  Whenever the file changes, runs the action.
  Also runs the action once immediately if the file exists.
  example:
    app.Watch 'myclasses.rb', ->(f) { load f }

RemoveWatch 'file'
  Stops watching a file.

UpdateApplicationResources 'file'
  Loads a ResourceDictionary from xaml and stores it in the application resources.
  Useful for adding adding styles or other resources to reference in other xaml.

Print 'output'
  Prints some text to the debug output.

Clear()
  Clears the debug output.

FindTypes 'partial type names'
  Given a space-separated list of strings, looks for all classnames that match each of those pieces.
  example:
    app.FindTypes 'open dialog'

LookupStatics 'classname'
  Lists all public static methods of a type.

LookupMethods 'classname'
  Lists all public methods of a type.

LookupProperties 'classname'
  Lists all public properties of a type.

LookupEnum 'enumname'
  Lists all the members of the given enumeration type.
  example: app.LookupEnum 'Dock'
";
      }

      #region Helpers

      void OnChanged(object sender, FileSystemEventArgs e) { Update(e.Name); }

      void Update(string file) {
         if (!_observeActions.ContainsKey(file)) return;
         var newline = Environment.NewLine;
         string debugText = "Observed change to " + file;
         _dispatcher.Invoke(() => {
            ExceptionHandler.Try(() => {
               Thread.Sleep(500); // wait a moment for the previous user to release the lock
               _observeActions[file](file);
            }, "Error running action when " + file + " changed");
         });
      }

      #endregion
   }

   public static class ExceptionHandler {
      internal static AppHelper App;

      public static void Try(Action a, string errorHeader) {
         try {
            a();
         } catch (Exception ex) {
            var newline = Environment.NewLine;
            string debugText = errorHeader + ":";
            while (ex != null) {
               debugText += newline + ex.Message;
               ex = ex.InnerException;
            }
            App.Print(debugText + newline);
         }
      }
   }
}
