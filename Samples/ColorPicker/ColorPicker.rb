
def FromHSB(hue, sat, bright)
   hue -= hue.floor
   r,g,b = 0,0,0
   hue6 = hue*6
   hue6 = hue6-hue6.floor # ranges from 0 to 1 6, times
   case
   when hue<1.0/6
      r, g = 1, hue6
   when hue<2.0/6
      r, g = 1-hue6, 1
   when hue<3.0/6
      g, b = 1, hue6
   when hue<4.0/6
      g, b = 1-hue6, 1
   when hue<5.0/6
      b, r = 1, hue6
   when hue<6.0/6
      b, r = 1-hue6, 1
   end
   r *= bright
   g *= bright
   b *= bright
   max = [r,g,b].max
   r = (r-max)*sat + max
   g = (g-max)*sat + max
   b = (b-max)*sat + max
   r *= 255
   g *= 255
   b *= 255
   [r, g, b]
end

def ToHSB(color)
   r = color.R / 255.0
   g = color.G / 255.0
   b = color.B / 255.0
   bright = [r,g,b].max
   if r==g && g==b && b==r
      return [0,0,bright]
   end
   sat = 1 - [r/bright,g/bright,b/bright].min
   r = (r-bright)/sat + bright
   g = (g-bright)/sat + bright
   b = (b-bright)/sat + bright
   r /= bright
   g /= bright
   b /= bright
   max = [r,g,b].max
   min = [r,g,b].min
   hue = 0.0/3.0 + g/6.0 if r==max && b==min
   hue = 1.0/3.0 - r/6.0 if g==max && b==min
   hue = 1.0/3.0 + b/6.0 if g==max && r==min
   hue = 2.0/3.0 - g/6.0 if b==max && r==min
   hue = 2.0/3.0 + r/6.0 if b==max && g==min
   hue = 3.0/3.0 - b/6.0 if r==max && g==min
   [hue, sat, bright]
   # [r,g,b]
end

def UpdateHSB(context, rgb)
   color = Color.FromRgb rgb[0], rgb[1], rgb[2]
   hsb = ToHSB color
   context.Hue = hsb[0]
   context.Saturation = hsb[1]
   context.Brightness = hsb[2]
end

def FillColorPickerContext(control, context)
   context.Red = 0
   context.Green = 0
   context.Blue = 0
   context.Hue = 0
   context.Saturation = 0
   context.Brightness = 0
   context.SaturationPosition = 0
   context.BrightnessPosition = 0

   context.SelectSaturationBrightnessExecuted = ->(obj, e) {
      p = e.GetPosition obj
      sat = p.X / obj.ActualWidth
      brt = 1 - p.Y / obj.ActualHeight
      color = FromHSB context.Hue, sat, brt
      context.Red = color[0]
      context.Green = color[1]
      context.Blue = color[2]  
   }

   ignoreUpdates = false
   updateHsbFromRgb = ->() {
      return if ignoreUpdates
      ignoreUpdates = true
      rgb = [context.Red, context.Green, context.Blue]
      UpdateHSB context, rgb
      ignoreUpdates = false
   }
   context.RedChanged = updateHsbFromRgb
   context.GreenChanged = updateHsbFromRgb
   context.BlueChanged = updateHsbFromRgb
   context.HueChanged = ->() {
      return if ignoreUpdates
      ignoreUpdates = true
      color = FromHSB context.Hue, context.Saturation, context.Brightness
      context.Red = color[0]
      context.Green = color[1]
      context.Blue = color[2]
      ignoreUpdates = false
   }
   context.SaturationChanged = ->() {
      x = (context.Saturation - 0.5) * control.SBPicker.ActualWidth
      control.SBIndicator.RenderTransform.X = x
   }
   context.BrightnessChanged = ->() {
      y = (0.5 - context.Brightness) * control.SBPicker.ActualHeight
      control.SBIndicator.RenderTransform.Y = y
   }
end
