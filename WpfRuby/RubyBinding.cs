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
         object returnValue = null;
         ExceptionHandler.Try(() => returnValue = Dlr.Execute(Expression), "MultiValue Conversion Fail: " + Expression);
         return returnValue;
      }

      public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
         Dlr.Scope.value = value;
         object[] returnValues = null;
         ExceptionHandler.Try(() => returnValues = (object[])Dlr.Execute(Expression), "MultiValue BackConversion Fail: " + BackExpression);
         return returnValues;
      }

      #endregion

      #region IValueConverter

      public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
         Dlr.Scope.value = value;
         object returnValue = null;
         ExceptionHandler.Try(() => returnValue = Dlr.Execute(Expression), "Value Conversion Fail: " + Expression);
         return returnValue;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
         Dlr.Scope.value = value;
         object returnValue = null;
         ExceptionHandler.Try(() => returnValue = Dlr.Execute(BackExpression), "Value BackConversion Fail: " + BackExpression);
         return returnValue;
      }

      #endregion
   }
}
