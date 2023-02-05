extends Node3D

var _server_scene = preload("res://scenes/Server.tscn")
var _client_scene = preload("res://scenes/Client.tscn")

func _ready():
	var args =_read_args()
	if "server" in args:
		$Control/Label.text = "Server Side"
		_create_server(int(args["server"]))
		return
	
func _on_button_pressed(host:bool):
	var created = false

	if host:
		$Control/Label.text = "Server Side"
		_create_server(7777)
		created = true
	else:
		$Control/Label.text = "Client Side"
		if _create_client($Buttons/TextEdit.text):
			created = true

	if created:
		$Buttons.queue_free()

func _read_args():
	var arguments = {}
	
	for argument in OS.get_cmdline_args():
		if argument.find("=") > -1:
			var key_value = argument.split("=")
			arguments[key_value[0].lstrip("--")] = key_value[1]
		else:
			arguments[argument.lstrip("--")] = ""

	return arguments

func _create_server(port: int):
	var server = _server_scene.instantiate()
	server.set("ListeningPort", port)
	self.add_child(server)

func _create_client(fullAddress: String):
	var data = fullAddress.split(":", false)
	
	if data.size() != 2:
		return false

	var client = _client_scene.instantiate()
	client.set("ServerAddress", data[0])
	client.set("ServerPort", data[1])
	self.add_child(client)
	return true