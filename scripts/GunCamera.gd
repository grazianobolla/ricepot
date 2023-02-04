extends Camera3D

@export var camera_path: NodePath
var _player_camera : Camera3D
const FOV_SPEED = 64

func _ready():
	_player_camera = get_node(camera_path)

func _process(_delta):
	self.global_transform = _player_camera.global_transform

func _physics_process(delta):
	if (Input.is_action_pressed("ui_page_up")):
		self.fov += delta * FOV_SPEED

	if (Input.is_action_pressed("ui_page_down")):
		self.fov -= delta * FOV_SPEED
