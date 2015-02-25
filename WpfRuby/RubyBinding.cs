using System;
using System.Globalization;
using System.Windows.Data;

namespace SorcererSoftware {

   /// <summary>
   /// This is a markup extension that creates a value converter.
   /// The value converter does the conversion using a ruby script.
   /// </summary>
   public class RubyBindingExtension : Binding {
      string _expression, _backExpression;
      public string Expression {
         set {
            _expression = value;
            UpdateConverter();
         }
      }
      public string BackExpression {
         set {
            _backExpression = value;
            UpdateConverter();
         }
      }
      public RubyBindingExtension() : base() { }
      public RubyBindingExtension(string path) : base(path) { }
      void UpdateConverter() {
         Converter = new RubyValueConverter { Expression = _expression, BackExpression = _backExpression };
      }
   }

   /// <summary>
   /// This is a markup extension that creates a value converter.
   /// The value converter does the conversion using a ruby script.
   /// </summary>
   public class RubyMultiBindingExtension : MultiBinding {
      string _expression, _backExpression;
      public string Expression {
         set {
            _expression = value;
            UpdateConverter();
         }
      }
      public string BackExpression {
         set {
            _backExpression = value;
            UpdateConverter();
         }
      }
      void UpdateConverter() {
         Converter = new RubyValueConverter { Expression = _expression, BackExpression = _backExpression };
      }
   }

   public class RubyValueConverter : IMultiValueConverter, IValueConverter {
      public string Expression { get; set; }
      public string BackExpression { get; set; }
      public RubyValueConverter() { }

      #region IMultiValueConverter

      public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
         Dlr.Scope.values = values;
         try {
            return Dlr.Execute(Expression);
         } catch (Exception ex) {
            // conversion failed
            return null;
         }
      }

      public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
         Dlr.Scope.value = value;
         try {
            return (object[])Dlr.Execute(Expression);
         } catch (Exception ex) {
            // conversion failed
            return null;
         }
      }

      #endregion

      #region IValueConverter

      public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
         Dlr.Scope.value = value;
         return Dlr.Execute(Expression);
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
         Dlr.Scope.value = value;
         return Dlr.Execute(Expression);
      }

      #endregion
   }
}
