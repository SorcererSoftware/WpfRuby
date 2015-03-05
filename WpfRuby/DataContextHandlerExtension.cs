using System;
using System.Windows;
using System.Windows.Markup;

namespace SorcererSoftware {
   public class DataContextHandlerExtension : MarkupExtension {
      public string MethodName { get; set; }
      public DataContextHandlerExtension() { }
      public DataContextHandlerExtension(string methodName) { MethodName = methodName; }
      public override object ProvideValue(IServiceProvider serviceProvider) {
         RoutedEventHandler handler = (sender, e) => {
            ExceptionHandler.Try(() => {
               var element = (FrameworkElement)sender;
               var context = element.DataContext as ExpandoDependencyObject;
               if (context == null) throw new InvalidOperationException("The DataContext wasn't an ExpandoDependencyObject");
               dynamic method = context[MethodName];
               if (method == null) throw new InvalidOperationException("The DataContext didn't have a method named " + MethodName);
               method(sender, e);
            }, "Error running event handler " + MethodName);
         };
         return handler;
      }
   }
}
