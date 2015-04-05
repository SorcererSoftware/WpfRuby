control = RubyControl.new app
context = ExpandoDependencyObject.new
control.InitializeContent "Graphs.xaml", ->(ctl){}
control.DataContext = context
app.Watch "Graphs.rb", ->(file){load file; UpdateContext context}
app.WindowContent = control
