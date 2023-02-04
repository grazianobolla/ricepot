extends Sprite3D

func randomize_angle():
	self.rotate_z(deg_to_rad(randi() % 360))
