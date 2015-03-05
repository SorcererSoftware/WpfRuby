
# make a standard DataCoentxt and Control
context = ExpandoDependencyObject.new
control = RubyControl.new app
control.DataContext = context

# by having ColorPicker.rb and .xaml, changes to the view don't reset the data,
# and changes to the data don't require reloading the view.
control.InitializeContent "ColorPicker.xaml", ->(ctl){ }
app.Watch "ColorPicker.rb", ->(file) { load file; FillColorPickerContext control, context }

# Put everything into the window.
app.WindowContent = control