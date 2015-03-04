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

      public static void RouteCommandExecutedToDataContext(object sender, ExecutedRoutedEventArgs args) {
         var element = sender as FrameworkElement;
         if (element == null) return;
         var context = element.DataContext as ExpandoDependencyObject;
         if (context == null) return;
         context.TryExecute(args);
      }
      public static void RouteCommandCanExecuteToDataContext(object sender, CanExecuteRoutedEventArgs args) {
         var element = sender as FrameworkElement;
         if (element == null) return;
         var context = element.DataContext as ExpandoDependencyObject;
         if (context == null) return;
         context.TryCanExecute(sender, args);
      }

      static void RoutedEventCommandsChanged(object sender, NotifyCollectionChangedEventArgs e) {
         RoutedEventCommandCollection collection = (RoutedEventCommandCollection)sender;
         if (e.Action != NotifyCollectionChangedAction.Add) throw new Exception("RoutedEventCommands do not currently support having elements removed, changed, or moved.");
         foreach (RoutedEventCommand eventCommand in e.NewItems) {
            var command = eventCommand.Command;
            RoutedEventHandler handler = (obj, args) => command.Execute(args, collection.Element);
            collection.Element.AddHandler(eventCommand.Event, handler);
            collection.Element.CommandBindings.Add(new CommandBinding(command, RouteCommandExecutedToDataContext, RouteCommandCanExecuteToDataContext));
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

   public class DataContextCommandBindingExtension : MarkupExtension {
      public string Command { get; set; }
      public override object ProvideValue(IServiceProvider serviceProvider) {
         return new CommandBinding(DynamicCommands.GetCommand(Command), Events.RouteCommandExecutedToDataContext, Events.RouteCommandCanExecuteToDataContext);
      }
   }
}
