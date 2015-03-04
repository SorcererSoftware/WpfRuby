control = RubyControl.new app
control.InitializeContent "Squares.xaml", ->(ctl){}
context = ExpandoDependencyObject.new
control.DataContext = context
app.WindowContent = control

# --- the recursive drawing
context.CreateBrush = ->() {
	# helper function: create an image brush that renders the control
	buffer = RenderTargetBitmap.new control.Root.Width, control.Root.Height, 96, 96, PixelFormats.Pbgra32
	brush = ImageBrush.new buffer
}
# make two of those brushes, so we can have one active while we use the other to render.
currentBrush = 0
brushes = [(context.CreateBrush()), (context.CreateBrush())]
context.UpdateRenderExecuted = ->() {
	brush = brushes[currentBrush]
	currentBrush = 1 - currentBrush
	brush.ImageSource.Clear()
	brush.ImageSource.Render control.Root
	control.Red.Fill = brush
	control.Blue.Fill = brush
}

# --- use the left and right mouse buttons to move the squares around
context.GetPoint = ->(sender, e) {
	# helper function: translates the mouse coordinate to a point usable for transforms
	source = e.GetPosition(control.Root)
   source.X -= control.Root.Width / 2
   source.Y -= control.Root.Height / 2
   translation = sender.RenderTransform.Children[2]
   source.X -= translation.X
   source.Y -= translation.Y
   return source
}
# store the mouse-down point, so we can watch the mouse move
p = Point.new
context.MouseDownExecuted = ->(sender,e) {
	p = context.GetPoint sender,e
	sender.CaptureMouse()
}
context.MouseMoveExecuted = ->(sender,e){
	return if not sender.IsMouseCaptured
	newP = context.GetPoint sender,e
	if e.LeftButton == MouseButtonState.Pressed
		# translate
		dif = newP - p
		sender.RenderTransform.Children[2].X += dif.X
		sender.RenderTransform.Children[2].Y += dif.Y
	end
	if e.RightButton == MouseButtonState.Pressed
		# scale
	   sourceMagnitude = Math.sqrt p.X * p.X + p.Y * p.Y
	   movedMagnitude = Math.sqrt newP.X * newP.X + newP.Y * newP.Y
	   scale = sender.RenderTransform.Children[0]
	   scale.ScaleX *= movedMagnitude / sourceMagnitude
	   scale.ScaleY *= movedMagnitude / sourceMagnitude

	   # rotate
	   rotation = sender.RenderTransform.Children[1]
	   sourceAngle = Math.atan2 p.Y, p.X
	   movedAngle = Math.atan2 newP.Y, newP.X
	   # if (_source.X < 0) sourceAngle += Math.PI;
	   # if (moved.X < 0) movedAngle += Math.PI;
	   rotation.Angle += (movedAngle - sourceAngle) * 180 / Math::PI;

	   p = newP
	end
}
context.MouseUpExecuted = ->(sender,e) {
	return if not sender.IsMouseCaptured
	sender.ReleaseMouseCapture()
	p = Point.new
}


