extends SubViewport

func _ready():
	var vp_rect = get_tree().root.get_viewport().get_visible_rect()
	self.size = vp_rect.size