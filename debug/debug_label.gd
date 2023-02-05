extends Label

func _process(_delta):
	var entities = get_node("/root/Main/EntityArray").get_children()

	self.text = ""
	for entity in entities:
		self.text += entity.name + " " + str(entity.position.snapped(Vector3.ONE*0.1))
		if entity.get("velocity"):
			self.text += " " + str(entity.velocity.snapped(Vector3.ONE*0.1))

		self.text += "\n"
