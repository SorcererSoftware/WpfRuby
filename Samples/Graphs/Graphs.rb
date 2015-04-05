def PrepareUpdate(set, i)
	Dlr.Execute set.Variable.ToString() + " = " + set.Data[i].ToString()
end

def Update(set, i)
	val = Dlr.Execute set.Formula
	set.Data[i+1] = val
end

def UpdateAll(context)
	for set in context.GraphSet
		set.Data[0] = set.Start
	end
	for i in 0..298
		for set in context.GraphSet
			PrepareUpdate(set, i)
		end
		for set in context.GraphSet
			Update(set, i)
		end
	end
	for set in context.GraphSet
		data = set.Data
		set.Data = 0
		set.Data = data
	end
end

def UpdateContext(context)
	formulaSpace = ExpandoDependencyObject.new
	context.GraphSet = "abcdef".split(//).map do |c|
		dataset = ExpandoDependencyObject.new
		dataset.Variable = c
		dataset.Formula = c
		dataset.Start = 0
		dataset.Data = Array.new(300) { 0 }
		dataset.StartChanged = ->() { UpdateAll context }
		dataset.FormulaChanged = ->() { UpdateAll context }
		dataset
	end
end