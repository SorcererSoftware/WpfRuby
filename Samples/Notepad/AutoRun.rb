control = RubyControl.new app
context = ExpandoDependencyObject.new
control.InitializeContent "Notepad.xaml", ->(ctl){}
control.DataContext = context
context.Text = "".ToString()
app.WindowContent = control

context.AboutExecuted = ->(sender,e) { MessageBox.Show "Made with Ruby!" }
context.NewExecuted = ->(sender,e) { context.Text = "" }
context.CloseExecuted = ->(sender,e) { Application.Current.MainWindow.Close() }

context.SaveExecuted = ->(sender,e) {
	dialog = Microsoft::Win32::SaveFileDialog.new
	dialog.Filter = "Text Files|*.txt"
	return if not dialog.ShowDialog()
	System::IO::File.WriteAllText dialog.FileName, context.Text
}

context.OpenExecuted = ->(sender,e) {
	dialog = Microsoft::Win32::OpenFileDialog.new
	dialog.Filter = "Text Files|*.txt"
	return if not dialog.ShowDialog()
	context.Text = System::IO::File.ReadAllText dialog.FileName
}
