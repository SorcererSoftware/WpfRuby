using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace SorcererSoftware {
   public class RoutedEventCommandCollection : ObservableCollection<RoutedEventCommand> { }

   public class RoutedEventCommand {
      public RoutedEvent Event { get; set; }
      public RoutedUICommand Command { get; set; }
      public RoutedEventHandler Handler { get { return _handler; } }
      void _handler(object sender, RoutedEventArgs e) {
         var element = (UIElement)sender;
         Command.Execute(e, element);
      }
   }

   public static class Events {
      #region Attached Property: RoutedEventCommands

      public static readonly DependencyProperty RoutedEventCommandsProperty = DependencyProperty.RegisterAttached("RoutedEventCommandsInternal", typeof(RoutedEventCommandCollection), typeof(UIElement), new FrameworkPropertyMetadata(null, RoutedEventCommandsChanged));

      public static RoutedEventCommandCollection GetRoutedEventCommands(this UIElement element) {
         var collection = element.GetValue(RoutedEventCommandsProperty) as RoutedEventCommandCollection;
         if (collection == null) {
            collection = new RoutedEventCommandCollection();
            element.SetValue(RoutedEventCommandsProperty, collection);
         }
         return collection;
      }

      public static void SetRoutedEventCommands(this UIElement element, RoutedEventCommandCollection collection) {
         element.SetValue(RoutedEventCommandsProperty, collection);
      }

      static readonly IDictionary<UIElement, NotifyCollectionChangedEventHandler> _routedEventComandsChangedHandlers = new Dictionary<UIElement, NotifyCollectionChangedEventHandler>();

      static NotifyCollectionChangedEventHandler CreateHandlerForElement(UIElement element) {
         return (sender, args) => {
            if (args.OldItems != null) {
               foreach (RoutedEventCommand item in args.OldItems) {
                  element.RemoveHandler(item.Event, item.Handler);
               }
            }
            if (args.NewItems != null) {
               foreach (RoutedEventCommand item in args.NewItems) {
                  element.AddHandler(item.Event, item.Handler);
               }
            }
         };
      }

      static void RoutedEventCommandsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
         var element = (UIElement)d;
         if (e.OldValue != null) {
            var collection = (RoutedEventCommandCollection)e.OldValue;
            foreach (var item in collection) {
               element.RemoveHandler(item.Event, item.Handler);
            }
            collection.CollectionChanged -= _routedEventComandsChangedHandlers[element];
         }
         if (e.NewValue != null) {
            var collection = (RoutedEventCommandCollection)e.NewValue;
            foreach (var item in collection) {
               element.AddHandler(item.Event, item.Handler);
            }
            _routedEventComandsChangedHandlers[element] = CreateHandlerForElement(element);
            collection.CollectionChanged += _routedEventComandsChangedHandlers[element];
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
      public ICommand Command { get; set; }
      public override object ProvideValue(IServiceProvider serviceProvider) {
         return new CommandBinding(Command, RouteCommandExecutedToDataContext, RouteCommandCanExecuteToDataContext);
      }

      static void RouteCommandExecutedToDataContext(object sender, ExecutedRoutedEventArgs args) {
         var element = sender as FrameworkElement;
         if (element == null) return;
         var context = element.DataContext as ExpandoDependencyObject;
         if (context == null) return;
         context.TryExecute(args);
      }
      static void RouteCommandCanExecuteToDataContext(object sender, CanExecuteRoutedEventArgs args) {
         var element = sender as FrameworkElement;
         if (element == null) return;
         var context = element.DataContext as ExpandoDependencyObject;
         if (context == null) return;
         context.TryCanExecute(sender, args);
      }
   }
}
