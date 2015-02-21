﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;

namespace SorcererSoftware {
   public class AppHelper {
      readonly IDictionary<string, Action<string>> _observeActions = new Dictionary<string, Action<string>>();
      readonly FileSystemWatcher _watcher = new FileSystemWatcher();
      readonly Dispatcher _dispatcher;
      readonly Decorator _body;
      string _appResourceSource;

      public UIElement WindowContent { set { _body.Child = value; } }

      public AppHelper(Decorator body) {
         _body = body;
         _dispatcher = body.Dispatcher;
         _watcher.Path = AppDomain.CurrentDomain.BaseDirectory;
         _watcher.IncludeSubdirectories = true;
         _watcher.Created += OnChanged;
         _watcher.Changed += OnChanged;
         _watcher.Renamed += OnChanged;
         _watcher.EnableRaisingEvents = true;
      }

      public event EventHandler<ValueEventArgs<string>> SendDebugText;

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
            var xaml = File.ReadAllText(file);
            var dictionary = (ResourceDictionary)XamlReader.Parse(xaml);
            var appResources = Application.Current.Resources.MergedDictionaries;
            appResources.Clear();
            appResources.Add(dictionary);
         });
      }

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
";
      }

      #region Helpers

      void OnChanged(object sender, FileSystemEventArgs e) { Update(e.Name); }

      void Update(string file) {
         if (!_observeActions.ContainsKey(file)) return;
         var newline = Environment.NewLine;
         string debugText = "Observed change to " + file;
         _dispatcher.Invoke(() => {
            try {
               Thread.Sleep(500); // wait a moment for the previous user to release the lock
               _observeActions[file](file);
            } catch (Exception ex) {
               debugText += newline + "Error running attached action:";
               while (ex != null) {
                  debugText += newline + ex.Message;
                  ex = ex.InnerException;
               }
            }

            if (SendDebugText != null) SendDebugText(this, debugText + newline + newline);
         });
      }

      #endregion
   }
}