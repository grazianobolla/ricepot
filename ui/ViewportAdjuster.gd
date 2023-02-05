extends SubViewport

func _physics_process(_delta): #TODO: not physics! use event
	var vp_rect = get_tree().root.get_viewport().get_visible_rect()
	self.size = vp_rect.size