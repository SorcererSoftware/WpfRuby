using System;
using System.Globalization;
using System.Windows.Data;

namespace SorcererSoftware {

   /// <summary>
   /// This is a markup extension that creates a value converter.
   /// The value converter does the conversion using a ruby script.
   /// </summary>
   public class RubyBindingExtension : Binding {
      public string Expression { set { Mode = BindingMode.OneWay; Converter = new RubyValueConverter { Expression = value }; } }
      public RubyBindingExtension() : base() { }
      public RubyBindingExtension(string path) : base(path) { }
   }

   /// <summary>
   /// This is a markup extension that creates a value converter.
   /// The value converter does the conversion using a ruby script.
   /// </summary>
   public class RubyMultiBindingExtension : MultiBinding {
      public string Expression { set { Converter = new RubyValueConverter { Expression = value }; Mode = BindingMode.OneWay; } }
   }

   public class RubyValueConverter : IMultiValueConverter, IValueConverter {
      public string Expression { get; set; }
      public RubyValueConverter() { }

      #region IMultiValueConverter

      public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
         Dlr.Scope.values = values;
         return Dlr.Execute(Expression);
      }

      public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) { throw new NotImplementedException(); }

      #endregion

      #region IValueConverter

      public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
         Dlr.Scope.value = value;
         return Dlr.Execute(Expression);
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }

      #endregion
   }
}
