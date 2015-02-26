using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace SorcererSoftware {
   public class RoutedEventCommandCollection : ObservableCollection<RoutedEventCommand> {
      public readonly UIElement Element;
      public RoutedEventCommandCollection(UIElement element) { Element = element; }
   }

   public class RoutedEventCommand {
      public RoutedEvent Event { get; set; }
      public RoutedUICommand Command { get; set; }
   }

   public class Events {

      #region Attached Property: RoutedEventCommands

      public static readonly DependencyProperty RoutedEventCommandsProperty = DependencyProperty.RegisterAttached("RoutedEventCommandsInternal", typeof(RoutedEventCommandCollection), typeof(UIElement), new PropertyMetadata(null));
      public static RoutedEventCommandCollection GetRoutedEventCommands(UIElement element) {
         var collection = element.GetValue(RoutedEventCommandsProperty) as RoutedEventCommandCollection;
         if (collection == null) {
            collection = new RoutedEventCommandCollection(element);
            collection.CollectionChanged += RoutedEventCommandsChanged;
            element.SetValue(RoutedEventCommandsProperty, collection);
         }
         return collection;
      }

      static void RoutedEventCommandsChanged(object sender, NotifyCollectionChangedEventArgs e) {
         RoutedEventCommandCollection collection = (RoutedEventCommandCollection)sender;
         if (e.Action != NotifyCollectionChangedAction.Add) throw new Exception("RoutedEventCommands do not currently support having elements removed, changed, or moved.");
         foreach (RoutedEventCommand eventCommand in e.NewItems) {
            var command = eventCommand.Command;
            RoutedEventHandler handler = (obj, args) => command.Execute(args, collection.Element);
            collection.Element.AddHandler(eventCommand.Event, handler);
            collection.Element.CommandBindings.Add(new CommandBinding(command, (obj, args) => {
               // Execute Event Handler
               var frameworkElement = collection.Element as FrameworkElement;
               if (frameworkElement == null) return;
               var context = frameworkElement.DataContext as ExpandoDependencyObject;
               if (context != null) context.TryExecute(args);
            }, (obj, args) => {
               // Can Execute Event Handler
               args.CanExecute = true;
               var frameworkElement = collection.Element as FrameworkElement;
               if (frameworkElement == null) return;
               var context = frameworkElement.DataContext as ExpandoDependencyObject;
               if (context != null) context.TryCanExecute(obj, args);
            }));
         }
      }

      #endregion

   }

   public static class DynamicCommands {
      static readonly IDictionary<string, RoutedCommand> _commands = new Dictionary<string, RoutedCommand>();
      public static RoutedCommand GetCommand(string name){
         if (_commands.ContainsKey(name)) return _commands[name];
         return _commands[name] = new RoutedUICommand(name, name, typeof(UIElement));
      }
   }

   public class CommandExtension : MarkupExtension {
      public string Name { get; set; }
      public CommandExtension(string name) { Name = name; }
      public override object ProvideValue(IServiceProvider serviceProvider) { return DynamicCommands.GetCommand(Name); }
   }
}
