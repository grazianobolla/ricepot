extends Label

@onready var character_body = get_node("/root/Scene/Player")
@onready var character_movement = character_body.get_node("PlayerMovement")

func _physics_process(_delta):
	var vel = character_body.velocity
	var lvel = character_movement.GetLocalVelocity()
	
	self.text = ""
	self.text += "%d" %  Engine.get_frames_per_second()
	self.text += "\npos x:%0.1f" % character_body.global_position.x + " y:%0.1f" % character_body.global_position.y + " z:%0.1f" % character_body.global_position.z
	self.text += "\nvel x:%0.1f" % vel.x + " y:%0.1f" % vel.y + " z:%0.1f" % vel.z + " n:%0.1f" % character_movement.GetVelocityNormalized().length()
	self.text += "\nlocvel x:%0.1f" % lvel.x + " y:%0.1f" % lvel.y + " z:%0.1f" % lvel.z
