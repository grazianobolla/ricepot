extends Label

func _process(delta):
	self.text = "%f" % (1.0 / delta)
