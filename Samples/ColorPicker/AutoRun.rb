
context = ExpandoDependencyObject.new
control = RubyControl.new app
control.DataContext = context

app.Watch "ColorPicker.rb", ->(file) { load file; FillColorPickerContext control, context }
control.InitializeContent "ColorPicker.xaml", ->(ctl){ }

app.WindowContent = control